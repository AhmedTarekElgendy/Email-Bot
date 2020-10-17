using GoogleApi.Entities.Translate.Detect.Response;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Limilabs.Mail.Appointments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Limilabs.Mail.Fluent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenPop.Mime;
using OpenPop.Pop3;
using RestSharp;
using RestSharp.Authenticators;
using S22.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Net;
using System.Text.RegularExpressions;

namespace ConsoleApp3
{
    public class EmailBot
    {
        #region Initialize
        string DetectedLanguage = "";
        bool hasSpecial, Entered;
        string senderMail;
        IRestResponse response;
        private JObject jobj;
        RestClient restClient;
        RestRequest request;
        Pop3Client popClient;
        MailAddress fromMail;
        MailAddress toMail;
        MailMessage MSG;
        string fullbody = "";
        MessageDTO messageDTO;
        int newMessagesCount;
        FullMessageDTO fullMessageDTO = new FullMessageDTO();
        int lastCount, i;
        JArray json;
        #endregion


        #region send

        public void SendMSG(string mailTo, string Msg, string msgSubject, string hostMail)
        {
            SmtpClient client = new SmtpClient()
            {
                Host = "smtp." + hostMail + ".com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential()
                {
                    UserName = "bot_mail",
                    Password = "bot_pass"
                }

            };

            fromMail = new MailAddress("bot_mail", "Bot sender");

            toMail = new MailAddress(mailTo, "Receiver_name");

            MSG = new MailMessage()
            {
                From = fromMail,
                Subject = msgSubject,
                Body = Msg

            };

            MSG.To.Add(toMail);

            client.Send(MSG);
        }

        #endregion


        #region Check if it has a special char

        public bool CheckSpecialChars(string str, string langCode)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!(Regex.IsMatch(str[i].ToString(), "[a-z0-9 ]+", RegexOptions.IgnoreCase)))
                {
                    if (langCode == "en")
                        SendMSG("receiver_mail", "Please send a valid message without special characters", "Important", "gmail");

                    else
                        SendMSG("receiver_mail", "رجاء إرسال رسالة صالحة بدون أحرف خاصة فى البريد الالكترونى", "هام", "gmail");

                    return true;
                }
            }
            return false;
        }
        #endregion


        #region LanguageDetection
        //note there is another way to detect and it's to get the hexa of a char 
        //each Language has a range of Hexa
        // so check if the hexa of the char fall into the range of this language

        public string DetectLanguage(string str)
        {

            restClient = new RestClient("http://ws.detectlanguage.com");

            restClient.Authenticator = new HttpBasicAuthenticator("8c07d1c4dd3e80d80a2075f7ef8c3a45", "");

            request = new RestRequest("/0.2/detect",
               RestSharp.Method.POST);

            request.AddParameter("q", str);

            //uncomment to check diffrent language
            //request.AddParameter("q", " اللغة العربية");
            response = restClient.Execute(request);

            jobj = JObject.Parse(response.Content);

            var check = jobj["data"]["detections"][0]["isReliable"].ToString();

            if (jobj["data"]["detections"][0]["isReliable"].ToString() == "True")
                return jobj["data"]["detections"][0]["language"].ToString();

            return "Can't detect the language";
        }
        #endregion

        #region receive
        public void ReceiveMsg()
        {
            popClient = new Pop3Client();
            popClient.Connect("pop.gmail.com", 995, true);
            popClient.Authenticate("bot_mail", "bot_pass");
            var count = popClient.GetMessageCount();

            restClient = new RestClient("https://localhost:44342/api/mail");
            request = new RestRequest(RestSharp.Method.GET);
            response = restClient.Execute(request);
            json = JArray.Parse(response.Content);

            lastCount = (int)json[0]["last"];
            newMessagesCount = count - lastCount;


            for (i = 1; i <= newMessagesCount; i++)
            {
                Message Message = popClient.GetMessage(lastCount + i);
                MSGValidation(Message);
            }
        }

        #endregion

        #region Message Validation
        public void MSGValidation(Message message)
        {

            if (message.Headers.From.Address != "bot_mail")
            {
                try
                {
                    fullbody = message.MessagePart.GetBodyAsText();
                }
                catch (Exception ex)
                {
                    fullbody = message.MessagePart.MessageParts[0].GetBodyAsText();
                }

                Entered = true;
                //this line is for deleting unwanted text like "This email has been checked for viruses by Avast antivirus software."
                var len = fullbody.IndexOf("-- \r\nThis email");

                if (len != -1)
                    fullbody = fullbody.Substring(0, len);

                fullbody = fullbody.Replace("\n", "").Replace("\r", "").Replace(".", "").Replace(",", "");

                DetectedLanguage = DetectLanguage(fullbody);

                hasSpecial = CheckSpecialChars(fullbody, DetectedLanguage);

                if (hasSpecial)
                    return;

                AnalyzeBody(fullbody, DetectedLanguage);

                senderMail = message.Headers.From.Address;


                lastCount += 1;
                var split1 = senderMail.Split('@');
                var hostMail = split1[1].Split('.');

                if (!(hasSpecial) && DetectedLanguage == "en")
                    SendMSG(senderMail, "Your demand are being processed", "Notification", "gmail");

                else if (!(hasSpecial) && DetectedLanguage == "ar")
                    SendMSG(senderMail, "يتم معالجة طالبك", "اشعار", "gmail");

                restClient = new RestClient("https://localhost:44342/api/mail");
                request = new RestRequest(RestSharp.Method.POST);
                request.RequestFormat = DataFormat.Json;

                messageDTO = new MessageDTO()
                {
                    Msg = fullbody,
                    Last = lastCount,
                    Email = senderMail,
                    Host = hostMail[0]
                };

                request.AddJsonBody(messageDTO);

                response = restClient.Execute(request);
            }
            if (i == newMessagesCount && !(Entered))
            {
                restClient = new RestClient("https://localhost:44342/api/mail");
                request = new RestRequest(RestSharp.Method.PUT);
                request.RequestFormat = DataFormat.Json;

                fullMessageDTO = new FullMessageDTO()
                {
                    ID = (int)json[0]["id"],
                    Email = json[0]["email"].ToString(),
                    Host = json[0]["host"].ToString(),
                    Msg = json[0]["msg"].ToString(),
                    Last = lastCount + newMessagesCount
                };

                request.AddJsonBody(fullMessageDTO);

                response = restClient.Execute(request);
            }
        }
        #endregion

        #region Text Analysis
        public void AnalyzeBody(string msg, string language)
        {
            DataAnalysisAPI dataAnalysisAPI = new DataAnalysisAPI();
            dataAnalysisAPI.documents = new documents[1];
            dataAnalysisAPI.documents[0] = new documents()
            {
                id =1,
                text = msg,
                language = language
            };

            restClient = new RestClient("https://rapidapi.p.rapidapi.com/keyPhrases");
            request = new RestRequest(RestSharp.Method.POST);
            request.AddHeader("content-type", "application/json");

            request.AddHeader("x-rapidapi-host", "microsoft-text-analytics1.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "9fdfaee7a2msh7bd51a897d5c357p1c3080jsn994b10d6ce88");


            request.AddJsonBody(dataAnalysisAPI);

            response = restClient.Execute(request);

            jobj = JObject.Parse(response.Content);

            var words = jobj["documents"][0]["keyPhrases"].ToArray();
            Console.WriteLine("Text key words is");
            for (int y = 0; y < words.Length; y++)
                Console.WriteLine(words[y]);
    }

        #endregion
    }
}

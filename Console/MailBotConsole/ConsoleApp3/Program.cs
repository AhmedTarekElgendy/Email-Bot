
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenPop.Mime;
using OpenPop.Pop3;
using RestSharp;


namespace ConsoleApp3
{

    class Program
    {
        static void Main(string[] args)
        {
            EmailBot emailBot = new EmailBot();

            emailBot.ReceiveMsg();
        }

    }


}

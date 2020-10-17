using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MailBotAPI.Models
{
    public class Message
    {
        [Key]
        public int ID { get; set; }
        public string Msg { get; set; }
        public string Email { get; set; }
        public string Host { get; set; }
        public int Last { get; set; }

    }
}

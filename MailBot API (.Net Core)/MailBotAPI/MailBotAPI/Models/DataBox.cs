using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MailBotAPI.Models
{
    public class DataBox
    {
        [Key]
        public int ID { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }
        public string Gender { get; set; }
    }
}

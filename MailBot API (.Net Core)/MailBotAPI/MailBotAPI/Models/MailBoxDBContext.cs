using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailBotAPI.Models
{
    public class MailBoxDBContext :DbContext
    {
        public MailBoxDBContext(DbContextOptions<MailBoxDBContext> options)
            : base(options)
        {

        }
        public DbSet<Message> Messages { get; set; }
        public DbSet<DataBox> DataBoxes { get; set; }
    }
}

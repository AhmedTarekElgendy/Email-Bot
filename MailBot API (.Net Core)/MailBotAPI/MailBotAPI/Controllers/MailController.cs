using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailBotAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MailBotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        MailBoxDBContext _context;
        public MailController(MailBoxDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetMessage()
        {
            return Ok(_context.Messages.Where(c=>c.ID ==(_context.Messages.Max(v=>v.ID))).ToList());
        }

        [HttpPost]
        public IActionResult CreateMessage(Message message)
        {
            if((!ModelState.IsValid) || message.Email == "" || message.Host == "" || message.Last == 0 || message.Msg == "")
                return BadRequest("Please check your data");

            _context.Messages.Add(message);
            _context.SaveChanges();
            return Ok(message);
        }

        [HttpPut]
        public IActionResult EditLast(Message message)
        {
            if ((!ModelState.IsValid) || message.Email == "" || message.Host == "" || message.Last == 0 || message.Msg == "")
                return BadRequest("Please check your data");

            var item = _context.Messages.SingleOrDefault(c => c.ID == message.ID);
            if (item == null)
                return BadRequest("Your message does not exist");

            item.Last = message.Last;
            _context.SaveChanges();
            return Ok(message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exch.Models;
using Exch.Services;
using Microsoft.AspNetCore.Mvc;

namespace Exch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrokerController : Controller
    {
        private MessageService _messageService;

        //1. Endpoint----------------------------------------------------------
        public BrokerController(MessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        //2. Senders messages
        [HttpGet("SendGet/{channel}/{text}")]
        public ActionResult<string> SendGet( int channel, string text)
        {
            InputMessage message = new InputMessage(channel, text);

            return _messageService.CreateRequest(Request, message);
        }

        [HttpPost("SendPost")]
        public ActionResult<string> SendPost([FromBody] InputMessage message)
        {
            return _messageService.CreateRequest(Request, message);
        }

        //3. Request response 
        [HttpGet("Answer/{requestKey}")]
        public ActionResult<ResponseResult> Answer(string requestKey)
        {
            if (string.IsNullOrWhiteSpace(requestKey))
            {
                return BadRequest("error -- requestKey is empty!");
            }
            var result = _messageService.GetResponse(requestKey);
            return result;
        }
        // GET api/Broker
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "I'm broker. I'm ready";
        }

        // POST api/Broker
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}
    }
}
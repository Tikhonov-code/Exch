using Exch.Controllers;
using Exch.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exch.Services
{
    public class MessageService
    {
        private static readonly Lazy<MessageService> _instance = new Lazy<MessageService>(() => new MessageService());
        private static IMessageHandler _messageHandler;

        private MessageService() { }
        public static MessageService GetInstance(IMessageHandler messageHandler)
        {
            if (!_instance.IsValueCreated)
            {
                _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            }

            return _instance.Value;
        }

        public string CreateRequest(HttpRequest request, InputMessage message)
        {
            return _messageHandler.CreateRequest(request,message);
        }
        public ActionResult<ResponseResult> GetResponse(string requestKey)
        {
            ResponseResult result = new ResponseResult();
            result = _messageHandler.GetResponse(requestKey);
            if (!string.IsNullOrWhiteSpace(result.answer))
            {
                return result;
            }
            else
            {
                result.answer = "message is empty";
                result.status = StatusCodes.Status204NoContent;
                return result;
            }
        }

    }
}

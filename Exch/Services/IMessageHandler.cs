using Exch.Controllers;
using Exch.Models;
using Microsoft.AspNetCore.Http;

namespace Exch.Services
{
    public interface IMessageHandler
    {
        string CreateRequest(HttpRequest request, InputMessage massage);
        ResponseResult GetResponse(string requestKey);
    }
}

using Socket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Core
{
    public class Router
    {
        private Dictionary<string, Func<HttpRequest, string>> routes;

        public Router(Dictionary<string, Func<HttpRequest, string>> routes)
        {
            this.routes = routes;
        }

        public string HandleRequest(HttpRequest request)
        {
            if (routes.ContainsKey(request.Path))
            {
                return routes[request.Path](request);
            }
            return GenerateResponse("404 Not Found", "text/html");
        }

        private string GenerateResponse(string content, string contentType, string status = "200 OK")
        {
            return $"HTTP/1.1 {status}\r\n" +
                   $"Content-Type: {contentType}\r\n" +
                   $"Content-Length: {System.Text.Encoding.UTF8.GetByteCount(content)}\r\n" +
                   "\r\n" +
                   content;
        }
    }
}

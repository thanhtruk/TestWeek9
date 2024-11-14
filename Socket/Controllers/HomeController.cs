using Socket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Controllers
{
    public class HomeController
    {
        public static string HandleHome(HttpRequest request)
        {
            string html = @"
                <html>
                    <body>
                        <h1>Student Information</h1>
                        <p>ID: 22123164</p>
                        <p>Full Name: Pham Thanh Truc</p>
                        <p>PC Number: PC009</p>
                        <a href='/login'>Login</a>
                    </body>
                </html>";
            return GenerateResponse(html, "text/html");
        }

        private static string GenerateResponse(string content, string contentType, string status = "200 OK")
        {
            return $"HTTP/1.1 {status}\r\n" +
                   $"Content-Type: {contentType}\r\n" +
                   $"Content-Length: {System.Text.Encoding.UTF8.GetByteCount(content)}\r\n" +
                   "\r\n" +
                   content;
        }
    }
}

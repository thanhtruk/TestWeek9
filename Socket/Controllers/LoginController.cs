using Socket.Models;
using Socket.Services.SingleTon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Socket.Controllers
{
    public class LoginController
    {
        private static Dictionary<string, string> activeSessions = new Dictionary<string, string>();

        public static string HandleLogin(HttpRequest request)
        {
            if (request.Method == "GET")
            {
                return ShowLoginForm();
            }
            else if (request.Method == "POST")
            {
                return ProcessLogin(request);
            }
            return GenerateResponse("Method not allowed", "text/html", "405 Method Not Allowed");
        }

        private static string ShowLoginForm()
        {
            string html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Login</title>
                    <style>
                        .login-form {
                            max-width: 300px;
                            margin: 50px auto;
                            padding: 20px;
                            border: 1px solid #ccc;
                            border-radius: 5px;
                        }
                        .form-group {
                            margin-bottom: 15px;
                        }
                        input[type='text'], input[type='password'] {
                            width: 100%;
                            padding: 8px;
                            margin-top: 5px;
                        }
                        input[type='submit'] {
                            width: 100%;
                            padding: 10px;
                            background-color: #4CAF50;
                            color: white;
                            border: none;
                            cursor: pointer;
                        }
                    </style>
                </head>
                <body>
                    <div class='login-form'>
                        <h2>Login</h2>
                        <form method='POST' action='/login'>
                            <div class='form-group'>
                                <label>Username:</label>
                                <input type='text' name='username' required>
                            </div>
                            <div class='form-group'>
                                <label>Password:</label>
                                <input type='password' name='password' required>
                            </div>
                            <input type='submit' value='Login'>
                        </form>
                    </div>
                </body>
                </html>";
            return GenerateResponse(html, "text/html");
        }

        private static string ProcessLogin(HttpRequest request)
        {
            var formData = ParseFormData(request.Body);
            string username = formData.GetValueOrDefault("username", "");
            string password = formData.GetValueOrDefault("password", "");
            Console.WriteLine(username + " " + password);
            if (UserDataManager.Instance.ValidateUser(username, password))
            {
                string sessionId = GenerateSessionId();
                activeSessions[sessionId] = username;

                return GenerateResponse(
                    "Login successful, redirecting...",
                    "text/html",
                    "302 Found",
                    new Dictionary<string, string> {
                        { "Set-Cookie", $"sessionId={sessionId}; Path=/" },
                        { "Location", "/chat" }
                    }
                );
            }

            return GenerateResponse(
                "<h2>Login failed. Invalid username or password.</h2><a href='/login'>Try again</a>",
                "text/html"
            );
        }

        private static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetUserFromSession(string sessionId)
        {
            return activeSessions.GetValueOrDefault(sessionId, null);
        }

        private static Dictionary<string, string> ParseFormData(string body)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(body)) return result;

            foreach (var pair in body.Split('&'))
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    result[parts[0]] = HttpUtility.UrlDecode(parts[1]);
                }
            }
            return result;
        }

        private static string GenerateResponse(string content, string contentType, string status = "200 OK", Dictionary<string, string> additionalHeaders = null)
        {
            StringBuilder response = new StringBuilder();
            response.AppendLine($"HTTP/1.1 {status}");
            response.AppendLine($"Content-Type: {contentType}");
            response.AppendLine($"Content-Length: {Encoding.UTF8.GetByteCount(content)}");

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    response.AppendLine($"{header.Key}: {header.Value}");
                }
            }

            response.AppendLine();
            response.Append(content);
            return response.ToString();
        }
    }
}

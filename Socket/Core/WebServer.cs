using Socket.Controllers;
using Socket.Models;
using Socket.Services.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Core
{
    public class WebServer
    {
        private TcpListener listener;
        private bool isRunning;
        private readonly string ipAddress;
        private readonly int port;

        public WebServer(string ipAddress = "127.0.0.1", int port = 8080)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;
                Console.WriteLine($"Server started at http://localhost:{port}");

                while (isRunning)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    // Đọc request
                    byte[] buffer = new byte[1024 * 4]; // Tăng buffer size
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Parse request
                    HttpRequest request = ParseRequest(requestData);
                    string response;

                    // Xử lý route
                    if (request.Path.StartsWith("/login"))
                    {
                        response = LoginController.HandleLogin(request);
                    }
                    else if (request.Path.StartsWith("/chat"))
                    {
                        response = ChatController.HandleChat(request);
                    }
                    else if (request.Path.StartsWith("/"))
                    {
                        response = HomeController.HandleHome(request);
                    }
                    else
                    {
                        response = "HTTP/1.1 302 Found\r\nLocation: /\r\n\r\n";
                    }

                    // Gửi response
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client handler error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private HttpRequest ParseRequest(string requestString)
        {
            try
            {
                var request = new HttpRequest
                {
                    Headers = new Dictionary<string, string>(),
                    Body = ""
                };

                string[] requestLines = requestString.Split(new[] { "\r\n" }, StringSplitOptions.None);
                if (requestLines.Length == 0) return request;

                // Parse first line
                string[] firstLineParts = requestLines[0].Split(' ');
                if (firstLineParts.Length >= 2)
                {
                    request.Method = firstLineParts[0];
                    request.Path = firstLineParts[1];
                }

                bool isBody = false;
                StringBuilder bodyBuilder = new StringBuilder();

                // Parse headers and body
                for (int i = 1; i < requestLines.Length; i++)
                {
                    if (string.IsNullOrEmpty(requestLines[i]))
                    {
                        isBody = true;
                        continue;
                    }

                    if (!isBody)
                    {
                        var headerParts = requestLines[i].Split(new[] { ": " }, 2, StringSplitOptions.None);
                        if (headerParts.Length == 2)
                        {
                            request.Headers[headerParts[0]] = headerParts[1];
                        }
                    }
                    else
                    {
                        bodyBuilder.AppendLine(requestLines[i]);
                    }
                }

                request.Body = bodyBuilder.ToString().Trim();
                return request;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing request: {ex.Message}");
                return new HttpRequest
                {
                    Method = "GET",
                    Path = "/",
                    Headers = new Dictionary<string, string>(),
                    Body = ""
                };
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
        }
    }
}

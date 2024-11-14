using Socket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Controllers
{
    public class ChatController
    {
        private static Dictionary<int, ChatRoom> chatRooms = new Dictionary<int, ChatRoom>();
        private static readonly object chatLock = new object();

        static ChatController()
        {
            // Initialize default chat rooms
            for (int i = 1; i <= 3; i++)
            {
                chatRooms[i] = new ChatRoom(i);
            }
        }

        public static string HandleChat(HttpRequest request)
        {
            string sessionId = GetSessionIdFromCookie(request);
            string username = LoginController.GetUserFromSession(sessionId);

            if (string.IsNullOrEmpty(username))
            {
                return GenerateResponse(
                    "Please login first",
                    "text/html",
                    "302 Found",
                    new Dictionary<string, string> { { "Location", "/login" } }
                );
            }

            if (request.Path == "/chat")
            {
                return ShowChatRooms();
            }
            else if (request.Path.StartsWith("/chat/"))
            {
                int roomId = int.Parse(request.Path.Split('/')[2]);

                if (request.Method == "GET")
                {
                    return ShowChatRoom(roomId, username);
                }
                else if (request.Method == "POST")
                {
                    return ProcessMessage(roomId, username, request);
                }
            }

            return GenerateResponse("Invalid request", "text/html", "400 Bad Request");
        }

        private static string ShowChatRooms()
        {
            StringBuilder html = new StringBuilder();
            html.Append(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Chat Rooms</title>
                    <style>
                        .chat-rooms {
                            max-width: 600px;
                            margin: 20px auto;
                        }
                        .room {
                            padding: 15px;
                            margin: 10px 0;
                            border: 1px solid #ccc;
                            border-radius: 5px;
                        }
                        .online { background-color: #e8f5e9; }
                        .offline { background-color: #ffebee; }
                        a { text-decoration: none; }
                    </style>
                </head>
                <body>
                    <div class='chat-rooms'>
                        <h1>Chat Rooms</h1>");

            foreach (var room in chatRooms.Values)
            {
                string status = room.IsOnline() ? "online" : "offline";
                html.Append($@"
                    <div class='room {status}'>
                        <h3>Room {room.Id}</h3>
                        <p>Status: {status}</p>
                        <a href='/chat/{room.Id}'>Join Room</a>
                    </div>");
            }

            html.Append(@"
                    </div>
                </body>
                </html>");

            return GenerateResponse(html.ToString(), "text/html");
        }

        private static string ShowChatRoom(int roomId, string username)
        {
            if (!chatRooms.ContainsKey(roomId))
            {
                return GenerateResponse("Room not found", "text/html", "404 Not Found");
            }

            var room = chatRooms[roomId];

            StringBuilder html = new StringBuilder();
            html.Append($@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Chat Room {roomId}</title>
                    <style>
                        .chat-container {{
                            max-width: 800px;
                            margin: 20px auto;
                        }}
                        .messages {{
                            height: 400px;
                            overflow-y: auto;
                            border: 1px solid #ccc;
                            padding: 10px;
                            margin-bottom: 20px;
                        }}
                        .message {{
                            margin: 10px 0;
                            padding: 5px;
                        }}
                        .message-form {{
                            display: flex;
                            gap: 10px;
                        }}
                        #messageInput {{
                            flex: 1;
                            padding: 5px;
                        }}
                    </style>
                    <script>
                        function refreshChat() {{
                            location.reload();
                        }}
                        setInterval(refreshChat, 5000);
                    </script>
                </head>
                <body>
                    <div class='chat-container'>
                        <h2>Chat Room {roomId}</h2>
                        <p>Logged in as: {username}</p>
                        <div class='messages'>");

            foreach (var message in room.Messages)
            {
                html.Append($@"
                    <div class='message'>
                        <strong>{message.Username}</strong> ({message.Time:HH:mm:ss}):
                        {message.Message}
                    </div>");
            }

            html.Append($@"
                        </div>
                        <form method='POST' action='/chat/{roomId}' class='message-form'>
                            <input type='text' id='messageInput' name='message' placeholder='Type your message...' required>
                            <input type='submit' value='Send'>
                        </form>
                        <p><a href='/chat'>Back to Rooms</a></p>
                    </div>
                </body>
                </html>");

            return GenerateResponse(html.ToString(), "text/html");
        }

        private static string ProcessMessage(int roomId, string username, HttpRequest request)
        {
            if (!chatRooms.ContainsKey(roomId))
            {
                return GenerateResponse("Room not found", "text/html", "404 Not Found");
            }

            var formData = ParseFormData(request.Body);
            string message = formData.GetValueOrDefault("message", "");

            if (!string.IsNullOrEmpty(message))
            {
                lock (chatLock)
                {
                    chatRooms[roomId].Messages.Add(new ChatMessage
                    {
                        Time = DateTime.Now,
                        Username = username,
                        Message = message
                    });
                    chatRooms[roomId].LastActivity = DateTime.Now;
                }
            }

            return GenerateResponse(
                "Message sent, redirecting...",
                "text/html",
                "302 Found",
                new Dictionary<string, string> { { "Location", $"/chat/{roomId}" } }
            );
        }

        private static string GetSessionIdFromCookie(HttpRequest request)
        {
            if (request.Headers.ContainsKey("Cookie"))
            {
                var cookies = request.Headers["Cookie"].Split(';')
                    .Select(c => c.Trim().Split('='))
                    .ToDictionary(c => c[0], c => c[1]);

                if (cookies.ContainsKey("sessionId"))
                {
                    return cookies["sessionId"];
                }
            }
            return null;
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
                    result[parts[0]] = System.Web.HttpUtility.UrlDecode(parts[1]);
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

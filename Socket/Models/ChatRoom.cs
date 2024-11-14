using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public List<ChatMessage> Messages { get; set; }
        public DateTime LastActivity { get; set; }

        public ChatRoom(int id)
        {
            Id = id;
            Messages = new List<ChatMessage>();
            LastActivity = DateTime.Today;
        }

        public bool IsOnline()
        {
            return (DateTime.Now - LastActivity).TotalMinutes <= 3;
        }
    }
}

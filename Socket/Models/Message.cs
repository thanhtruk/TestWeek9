﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Models
{
    public class ChatMessage
    {
        public DateTime Time { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
    }
}

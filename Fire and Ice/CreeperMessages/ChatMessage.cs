using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public class ChatMessage
    {
        public String Message { get; private set; }
        public ChatMessage(string message)
        {
            Message = message;
        }
    }
}

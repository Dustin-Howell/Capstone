using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum ChatMessageType { Send, Receive }
    public class ChatMessage
    {
        public String Message { get; private set; }
        public ChatMessageType Type { get; private set; }

        public ChatMessage(string message, ChatMessageType type)
        {
            Message = message;
            Type = type;
        }
    }
}

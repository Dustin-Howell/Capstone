using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperNetwork
{
   
    public class ChatEventArgs : EventArgs
    {
        public string Message = "";

        public ChatEventArgs(string messageIn)
        {
           Message = messageIn;
        }
    }
}

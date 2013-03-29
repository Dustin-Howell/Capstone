using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public enum NetworkErrorType { Forfeit, Disconnect, IllegalMove }
    public class NetworkErrorMessage
    {
        public NetworkErrorType Type { get; set; }

        public NetworkErrorMessage(NetworkErrorType type)
        {
            Type = type;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public enum NetworkErrorType { OpponentForfeit, Forfeit, Disconnect, IllegalMove, ForfeitMessage, OpponentForfeitMessage, IllegalMoveMessage, DisconnectMessage, OpponentDisconnect, OpponentDisconnectMessage, AckDisconnectMessage, AckDisconnect }
    public class NetworkErrorMessage
    {
        public NetworkErrorType Type { get; set; }

        public NetworkErrorMessage(NetworkErrorType type)
        {
            Type = type;
        }
    }
}

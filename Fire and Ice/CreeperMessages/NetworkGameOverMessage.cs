using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperMessages
{
    public enum END_GAME_TYPE
    {
        FORFEIT,
        ILLEGAL_MOVE,
        DISCONNECT
    }

    public class NetworkGameOverMessage
    {
        public END_GAME_TYPE EndGameType { get; private set; }
        public NetworkGameOverMessage(END_GAME_TYPE endType)
        {
            EndGameType = endType;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreeperNetwork
{
    public enum END_GAME_TYPE
    {
        FORFEIT,
        ILLEGAL_MOVE,
        DISCONNECT
    }

    public class EndGameEventArgs : EventArgs
    {
        public END_GAME_TYPE END_GAME_TYPE { get; set; }

        public EndGameEventArgs(END_GAME_TYPE endGameTypeIn)
        {
            END_GAME_TYPE = endGameTypeIn;
        }
    }
}
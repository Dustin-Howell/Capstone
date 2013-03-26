using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public enum MoveMessageType { Request, Response }
    public class MoveMessage
    {
        public MoveMessageType Type { get; set; }
        public Move Move { get; set; }
        public PlayerType PlayerType { get; set; }

        public MoveMessage(PlayerType playerType, MoveMessageType type, Move move = null)
        {
            PlayerType = playerType;
            Type = type;
            Move = move;
        }
    }
}

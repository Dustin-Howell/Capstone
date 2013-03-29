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
        public CreeperBoard Board { get; set; }
        public CreeperColor TurnColor { get; set; }
        public MoveMessageType Type { get; set; }
        public Move Move { get; set; }
        public PlayerType PlayerType { get; set; }
    }
}

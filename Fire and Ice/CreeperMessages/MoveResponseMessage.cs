using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public class MoveResponseMessage
    {
        public Move Move { get; set; }

        public MoveResponseMessage(Move move)
        {
            Move = move;
        }
    }
}

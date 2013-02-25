using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public class MoveEventArgs : EventArgs
    {
        public Move Move { get; set; }

        public MoveEventArgs(Move move)
        {
            Move = move;
        }
    }
}

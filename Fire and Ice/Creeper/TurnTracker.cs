using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creeper
{
    public static class TurnTracker
    {
        public static Player Player1 { get; set; }
        public static Player Player2 { get; set; }
        public static Player CurrentPlayer { get; set; }
        public static Player OpponentPlayer
        {
            get
            {
                return CurrentPlayer == Player1 ? Player2 : Player1;
            }
        }
    }
}

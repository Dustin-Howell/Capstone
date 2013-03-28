using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using CreeperNetwork;
using CreeperAI;
using Caliburn.Micro;

namespace CreeperCore
{
    public class GameSettings
    {
        public PlayerType Player1Type { get; set; }
        public PlayerType Player2Type { get; set; }
        public CreeperBoard Board { get; set; }
        public CreeperColor StartingColor { get; set; }

        public EventAggregator EventAggregator { get; set; }

        public Network Network { get; set; }
        public AI AI { get; set; }
    }
}

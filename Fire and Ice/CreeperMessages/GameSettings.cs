using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using Caliburn.Micro;

namespace CreeperMessages
{
    public class GameSettings
    {
        public PlayerType Player1Type { get; set; }
        public PlayerType Player2Type { get; set; }
        public CreeperBoard Board { get; set; }
        public CreeperColor StartingColor { get; set; }

        public EventAggregator EventAggregator { get; set; }

        public IHandle Network { get; set; }
        public IHandle AI { get; set; }
    }
}

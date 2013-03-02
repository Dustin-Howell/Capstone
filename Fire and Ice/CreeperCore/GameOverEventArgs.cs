using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperCore
{
    public class GameOverEventArgs : EventArgs
    {
        public CreeperColor Winner { get; set; }
    }
}

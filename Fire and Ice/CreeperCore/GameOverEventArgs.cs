using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperCore
{
    class GameOverEventArgs : EventArgs
    {
        public CreeperColor Winner { get; set; }
    }
}

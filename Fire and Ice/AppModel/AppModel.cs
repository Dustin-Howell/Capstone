using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNAControlGame;

namespace CreeperCore
{
    public static class AppModel
    {
        public static AppModel()
        {
            Core = new CreeperGameCore();
        }

        public static CreeperGameCore Core { get; private set; }
    }
}

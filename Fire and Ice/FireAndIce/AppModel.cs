using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreeperCore;

namespace FireAndIce
{
    public static class AppModel
    {
        static AppModel()
        {
            Core = new CreeperGameCore();
        }

        public static CreeperGameCore Core { get; private set; }
    }
}

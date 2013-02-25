using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreeperCore;
using FireAndIce.ViewModels;

namespace FireAndIce
{
    public static class AppModel
    {
        static AppModel()
        {
            Core = new CreeperGameCore();
        }

        public static CreeperGameCore Core { get; private set; }
        public static AppViewModel AppViewModel { get; set; }
    }
}

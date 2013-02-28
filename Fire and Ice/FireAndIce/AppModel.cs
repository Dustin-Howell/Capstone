using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreeperCore;
using FireAndIce.ViewModels;
using System.Windows;

namespace FireAndIce
{
    public static class AppModel
    {
        static AppModel()
        {
        }

        public static CreeperGameCore Core { get; private set; }
        public static AppViewModel AppViewModel { get; set; }

        public static ResourceDictionary Resources { get { return new ResourceDictionary() { Source = new Uri(@"..\Resources.xaml", UriKind.Relative) }; } }

        internal static void ResetCreeperCore()
        {
            Core = new CreeperGameCore();
        }
    }
}

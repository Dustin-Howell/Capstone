using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreeperCore;
using FireAndIce.ViewModels;
using System.Windows;
using CreeperNetwork;
using Caliburn.Micro;
using XNAControlGame;
using Creeper;

namespace FireAndIce
{
    public static class AppModel
    {
        static AppModel()
        {
            _slimCore = SlimCore;
            _game = XNAGame;
        }

        private static EventAggregator _eventAggregator;
        public static EventAggregator EventAggregator { get { return _eventAggregator = _eventAggregator ?? new EventAggregator(); } }

        private static SlimCore _slimCore;
        public static SlimCore SlimCore
        {
            get { return _slimCore = _slimCore ?? new SlimCore(EventAggregator); }
        }        

        public static AppViewModel AppViewModel { get; set; }

        private static Network _network;
        public static Network Network { get { return _network = _network ?? new Network(EventAggregator); } }

        private static Game1 _game;
        public static Game1 XNAGame
        {
            get
            {
                return _game = _game ?? new Game1(EventAggregator, SlimCore);
            }
        }

        private static CreeperAI.AI _AI;
        public static CreeperAI.AI AI
        {
            get
            {
                return _AI = _AI ?? new CreeperAI.AI(EventAggregator)
                {
                    TerritorialWeight = 15d,
                    MaterialWeight = 84d,
                    PositionalWeight = 2d,
                    ShortestDistanceWeight = 43d,
                    VictoryValue = 100000,
                    Difficulty = AIDifficulty.Hard,
                };
            }
        }

        private static ResourceDictionary _resources;
        public static ResourceDictionary Resources { get { return _resources = _resources ?? new ResourceDictionary() { Source = new Uri(@"..\Resources.xaml", UriKind.Relative) }; } }

        public static void ResetEventAggregator()
        {
            _eventAggregator = null;
        }
    }
}

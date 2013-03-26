using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperMessages;

namespace Creeper
{
    public class GameTracker : IHandle<MoveMessage>
    {
        private EventAggregator _eventAggregator;

        public GameTracker(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }
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
        public static CreeperBoard Board { get; set; }

        public void Handle(MoveMessage message)
        {
            
        }
    }
}

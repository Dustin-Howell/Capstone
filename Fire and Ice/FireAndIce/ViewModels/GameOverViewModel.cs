using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class GameOverViewModel : PropertyChangedBase
    {
        private String _winner;
        private String Winner
        {
            get { return _winner ?? ""; }
            set
            {
                _winner = value;
                NotifyOfPropertyChange(() => GameOverMessage);
            }
        }

        public String GameOverMessage
        {
            get
            {
                return String.Format("{0} wins!", _winner);
            }
        }

        public GameOverViewModel(CreeperColor? winner)
        {
            _winner = winner.Value.ToString();
        }
    }
}

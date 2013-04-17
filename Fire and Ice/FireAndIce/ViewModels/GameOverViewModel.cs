using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using Caliburn.Micro;
using CreeperMessages;
using System.Windows.Media;

namespace FireAndIce.ViewModels
{
    public class GameOverViewModel : Screen
    {
        private CreeperColor? _winner;
        private CreeperColor? Winner
        {
            get { return _winner; }
            set
            {
                _winner = value;
                NotifyOfPropertyChange(() => GameOverMessage);
            }
        }

        public SolidColorBrush WinningColor
        {
            get
            {
                return (SolidColorBrush)(_winner == CreeperColor.Fire ?
                    AppModel.Resources["Primary1"]
                    : AppModel.Resources["Secondary1"]
                    );
            }
        }

        public String GameOverMessage
        {
            get
            {
                if (_winner.HasValue)
                {
                    return String.Format("{0} wins!", _winner.ToString());
                }
                else
                {
                    return "Draw!";
                }
            }
        }

        public GameOverViewModel(CreeperColor? winner)
        {
            _winner = winner;
        }

        public void ReturnToMenu()
        {
            AppModel.EventAggregator.Publish(new ReturnToMenuMessage());
            TryClose();
        }
    }
}

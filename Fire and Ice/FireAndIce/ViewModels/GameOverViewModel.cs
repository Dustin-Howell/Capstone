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
                if (_winner.HasValue)
                {
                    return (SolidColorBrush)(_winner == CreeperColor.Fire ?
                        AppModel.Resources["Primary1"]
                        : AppModel.Resources["Complementary1"]
                        );
                }
                else
                {
                    return new SolidColorBrush(Colors.Black);
                }
            }
        }

        public SolidColorBrush LosingColor
        {
            get
            {
                if (_winner.HasValue)
                {
                    return (SolidColorBrush)(_winner == CreeperColor.Fire ?
                        AppModel.Resources["Complementary1"]
                        : AppModel.Resources["Primary1"]
                        );
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
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

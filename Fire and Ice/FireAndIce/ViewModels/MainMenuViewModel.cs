using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class MainMenuViewModel : Conductor<Screen>.Collection.OneActive
    {
        private bool _newGameChecked;
        public bool NewGameChecked 
        {
            get
            {
                return _newGameChecked;
            }
            set
            {
                if (_newGameChecked != value)
                {
                    _newGameChecked = value;
                    NotifyOfPropertyChange(() => NewGameChecked);
                }
            }
        }

        public SlideOutPanelViewModel MainMenu { get; set; }

        public void NewGame()
        {
            ActivateItem(new SlideOutPanelViewModel(
                new List<OptionButtonViewModel> {
                    new OptionButtonViewModel { ClickAction = () => StartLocalGame(), Title = "Local" },
                    new OptionButtonViewModel { ClickAction = () => StartLocalGame(), Title = "Local" },
                    new OptionButtonViewModel { ClickAction = () => StartLocalGame(), Title = "Local" },
                    new OptionButtonViewModel { ClickAction = () => StartLocalGame(), Title = "Local" },
                    new OptionButtonViewModel { ClickAction = () => StartLocalGame(), Title = "Local" },
            }));
        }

        private void StartLocalGame()
        {
        }

        public void Help()
        {
        }

        public void HighScores()
        {
        }

        public void Settings()
        {
        }

        public void Credits()
        {
        }

        public MainMenuViewModel()
        {
            MainMenu = new SlideOutPanelViewModel(
                new List<OptionButtonViewModel> {
                    new OptionButtonViewModel { ClickAction = () => NewGame(), Title = "New Game" },
                    new OptionButtonViewModel { ClickAction = () => Help(), Title = "Help" },
                    new OptionButtonViewModel { ClickAction = () => HighScores(), Title = "High Scores" },
                    new OptionButtonViewModel { ClickAction = () => Settings(), Title = "Settings" },
                    new OptionButtonViewModel { ClickAction = () => Credits(), Title = "Credits" },
                });
        }
    }
}

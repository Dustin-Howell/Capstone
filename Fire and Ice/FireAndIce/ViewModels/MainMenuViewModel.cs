using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FireAndIce.ViewModels
{
    public class MainMenuViewModel : Screen
    {
        private BindableCollection<SlideOutPanelViewModel> _menus;
        public BindableCollection<SlideOutPanelViewModel> Menus
        {
            get
            {
                return _menus;
            }
            set
            {
                _menus = value;
                NotifyOfPropertyChange(() => Menus);
            }
        }

        //private BindableCollection<String> _menus;
        //public BindableCollection<String> Menus
        //{
        //    get
        //    {
        //        return _menus;
        //    }
        //    set
        //    {
        //        _menus = value;
        //        NotifyOfPropertyChange(() => Menus);
        //    }
        //}

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
            Menus.Clear();
            Menus.Add(new SlideOutPanelViewModel(
                new List<OptionButtonViewModel> {
                    new OptionButtonViewModel { ClickAction = () => StartLocalGame(), Title = "Local" },
                    new OptionButtonViewModel { ClickAction = () => StartNetworkGame(), Title = "Network" },
            }));

            NotifyOfPropertyChange(() => Menus);
        }


        private void StartLocalGame()
        {
            Menus.Add(new SlideOutPanelViewModel(
                new List<OptionButtonViewModel>{
                    new OptionButtonViewModel {ClickAction = () => StartLocalHumanGame(), Title = "Human"},
                    new OptionButtonViewModel {ClickAction = () => StartLocalAIGame(), Title = "AI"},
            }));
        }

        private void StartLocalAIGame()
        {
            throw new NotImplementedException();
        }

        private void StartLocalHumanGame()
        {
            throw new NotImplementedException();
        }

        private void StartNetworkGame()
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
            Menus = new BindableCollection<SlideOutPanelViewModel>()
                {
                };

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

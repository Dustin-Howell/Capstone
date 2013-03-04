using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows;
using System.Windows.Media;
using CreeperCore;
using CreeperNetwork;
using System.ComponentModel;
using Creeper;
using System.Windows.Controls;

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

        private PropertyChangedBase _popup;
        public PropertyChangedBase Popup
        {
            get
            {
                return _popup;
            }
            set
            {
                _popup = value;
                NotifyOfPropertyChange(() => Popup);
            }
        }

        public void AddMenu(SlideOutPanelViewModel panel)
        {
            if (!Menus.Contains(panel))
            {
                if (panel.MenuParent == MainMenu)
                {
                    Menus.Apply(x => x.ControlIsVisible = false);
                    Menus.Clear();
                    Menus.Add(panel);
                }
                else
                {
                    BindableCollection<SlideOutPanelViewModel> newMenus = new BindableCollection<SlideOutPanelViewModel>(Menus);
                    bool foundParent = false;
                    foreach (SlideOutPanelViewModel menu in newMenus)
                    {
                        if ((foundParent |= menu == panel.MenuParent) && menu != panel.MenuParent)
                        {
                            menu.ControlIsVisible = false;
                            Menus.Remove(menu);
                        }
                    }

                    Menus.Add(panel);
                }

                panel.ControlIsVisible = true;

                foreach (OptionButtonViewModel button in panel.Buttons)
                {
                    button.IsOptionChecked = false;
                }

                Popup = null;
            }
        }

        private SlideOutPanelViewModel _newGameMenu;
        private SlideOutPanelViewModel NewGameMenu
        {
            get
            {
                return _newGameMenu = _newGameMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel>
                    {
                        new OptionButtonViewModel { ClickAction = () => AddMenu(LocalGameMenu), Title = "Local" },
                        new OptionButtonViewModel { ClickAction = () => AddMenu(NetworkGameMenu), Title = "Network" },
                    },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "Where?",
                    MenuParent = MainMenu,
                };
            }
        }

        private SlideOutPanelViewModel _localGameMenu;
        private SlideOutPanelViewModel LocalGameMenu
        {
            get
            {
                return _localGameMenu = _localGameMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => StartLocalHumanGame(), Title = "Human"},
                    new OptionButtonViewModel {ClickAction = () => AddMenu(LocalAIGameMenu), Title = "AI"},
                },
                    Background = AppModel.Resources["Primary5"] as SolidColorBrush,
                    Title = "Against?",
                    MenuParent = NewGameMenu,
                };
            }
        }

        private SlideOutPanelViewModel _localAIGameMenu;
        private SlideOutPanelViewModel LocalAIGameMenu
        {
            get
            {
                return _localAIGameMenu = _localAIGameMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => AddMenu(LocalEasyAIGameMenu), Title = "Novice"},
                    new OptionButtonViewModel {ClickAction = () => AddMenu(LocalHardAIGameMenu), Title = "Expert"},
                },
                    Background = AppModel.Resources["Primary1"] as SolidColorBrush,
                    Title = "Difficulty?",
                    MenuParent = LocalGameMenu,
                };
            }
        }

        private SlideOutPanelViewModel _localEasyAIGameMenu;
        private SlideOutPanelViewModel LocalEasyAIGameMenu
        {
            get
            {
                return _localEasyAIGameMenu = _localEasyAIGameMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => StartLocalEasyAIGame(CreeperColor.Fire), Title = "Fire"},
                    new OptionButtonViewModel {ClickAction = () => StartLocalEasyAIGame(CreeperColor.Ice), Title = "Ice"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "Choose a Side",
                    MenuParent = LocalAIGameMenu,
                };
            }
        }

        private void StartLocalEasyAIGame(CreeperColor playerColor)
        {
            GameContainerViewModel gameContainer = (playerColor == CreeperColor.Fire) ? new GameContainerViewModel(PlayerType.Human, PlayerType.AI, AIDifficulty.Easy) : new GameContainerViewModel(PlayerType.AI, PlayerType.Human, AIDifficulty.Easy);

            AppModel.AppViewModel.ActivateItem(gameContainer);
        }

        private SlideOutPanelViewModel _localHardAIGameMenu;
        private SlideOutPanelViewModel LocalHardAIGameMenu
        {
            get
            {
                return _localHardAIGameMenu = _localHardAIGameMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => StartLocalHardAIGame(CreeperColor.Fire), Title = "Fire"},
                    new OptionButtonViewModel {ClickAction = () => StartLocalHardAIGame(CreeperColor.Ice), Title = "Ice"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "Choose a Side",
                    MenuParent = LocalAIGameMenu,
                };
            }
        }

        private void StartLocalHardAIGame(CreeperColor playerColor)
        {
            GameContainerViewModel gameContainer = (playerColor == CreeperColor.Fire) ? new GameContainerViewModel(PlayerType.Human, PlayerType.AI, AIDifficulty.Hard) : new GameContainerViewModel(PlayerType.AI, PlayerType.Human, AIDifficulty.Hard);

            AppModel.AppViewModel.ActivateItem(gameContainer);
        }

        private void StartLocalHumanGame()
        {
            AppModel.AppViewModel.ActivateItem(new GameContainerViewModel(PlayerType.Human, PlayerType.Human));
        }

        private SlideOutPanelViewModel _networkGameMenu;
        private SlideOutPanelViewModel NetworkGameMenu
        {
            get
            {
                return _networkGameMenu = _networkGameMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => { HostNetworkGame(); }, Title = "Create Game"},
                    new OptionButtonViewModel {ClickAction = () => { FindNetworkGame(); }, Title = "Find Game"},
                },
                    Background = AppModel.Resources["Primary5"] as SolidColorBrush,
                    Title = "Network",
                    MenuParent = NewGameMenu,
                };
            }
        }

        private void HostNetworkGame()
        {
             Popup = new HostGameViewModel() { Title = "What's your name?" };
        }

        private void FindNetworkGame()
        {
            Popup = new FindGameViewModel() { Title = "Find a game." };
        }

        private SlideOutPanelViewModel _helpMenu;
        private SlideOutPanelViewModel HelpMenu
        {
            get
            {
                return _helpMenu = _helpMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Instructions"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Practice"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Guided Tour"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "Help",
                    MenuParent = MainMenu,
                };
            }
        }

        private SlideOutPanelViewModel _highScoresMenu;
        private SlideOutPanelViewModel HighScoresMenu
        {
            get
            {
                return _highScoresMenu = _highScoresMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Super"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Good"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Meh"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "High Scores",
                    MenuParent = MainMenu,
                };
            }
        }

        private SlideOutPanelViewModel _settingsMenu;
        private SlideOutPanelViewModel SettingsMenu
        {
            get
            {
                return _settingsMenu = _settingsMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Toggle"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Sprocket"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Switch"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Pixel"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "Settings",
                    MenuParent = MainMenu,
                };
            }
        }

        private SlideOutPanelViewModel _creditsMenu;
        private SlideOutPanelViewModel CreditsMenu
        {
            get
            {
                return _creditsMenu = _creditsMenu ?? new SlideOutPanelViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => new object(), Title = "Joshua Griffith"},
                    new OptionButtonViewModel {ClickAction = () => new object(), Title = "Gage Gwaltney"},
                    new OptionButtonViewModel {ClickAction = () => new object(), Title = "Dustin Howell"},
                    new OptionButtonViewModel {ClickAction = () => new object(), Title = "Kaleb Lape"},
                    new OptionButtonViewModel {ClickAction = () => new object(), Title = "Jon Scott Smith"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "RRR Software",
                    MenuParent = MainMenu,
                };
            }
        }

        public MainMenuViewModel()
        {
            Menus = new BindableCollection<SlideOutPanelViewModel>();
            MainMenu = new SlideOutPanelViewModel() {
                Buttons  = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel { ClickAction = () => AddMenu(NewGameMenu), Title = "New Game" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(HelpMenu), Title = "Help" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(HighScoresMenu), Title = "High Scores" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(SettingsMenu), Title = "Settings" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(CreditsMenu), Title = "Credits" },
                    new OptionButtonViewModel { ClickAction = () => AppModel.AppViewModel.TryClose(), Title = "Exit" },
                },
                Background = AppModel.Resources["Primary1"] as SolidColorBrush,
                Title = "Main Menu",
            };

            MainMenu.ControlIsVisible = true;
        }
    }
}

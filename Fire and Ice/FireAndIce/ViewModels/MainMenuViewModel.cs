using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows;
using System.Windows.Media;
using CreeperCore;

namespace FireAndIce.ViewModels
{
    public class MainMenuViewModel : Screen
    {
        //TODO: Move this to App
        public ResourceDictionary Resources { get { return new ResourceDictionary() { Source = new Uri(@"..\Resources.xaml", UriKind.Relative) }; } }

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
                    Background = Resources["Primary4"] as SolidColorBrush,
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
                    new OptionButtonViewModel {ClickAction = () => StartLocalAIGame(), Title = "AI"},
                },
                    Background = Resources["Primary3"] as SolidColorBrush,
                    Title = "Against?",
                    MenuParent = NewGameMenu,
                };
            }
        }

        private void StartLocalAIGame()
        {
            AppModel.AppViewModel.ActivateItem(new GameContainerViewModel(PlayerType.Human, PlayerType.AI));
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
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "Networky Stuff"},
                    new OptionButtonViewModel {ClickAction = () => { throw new NotImplementedException(); }, Title = "More Networky Stuff"},
                },
                    Background = Resources["Primary3"] as SolidColorBrush,
                    Title = "Network",
                    MenuParent = NewGameMenu,
                };
            }
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
                    Background = Resources["Primary4"] as SolidColorBrush,
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
                    Background = Resources["Primary4"] as SolidColorBrush,
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
                    Background = Resources["Primary4"] as SolidColorBrush,
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
                    Background = Resources["Primary4"] as SolidColorBrush,
                    Title = "RRR Software",
                    MenuParent = MainMenu,
                };
            }
        }

        public MainMenuViewModel()
        {
            Menus = new BindableCollection<SlideOutPanelViewModel>()
                {
                };

            MainMenu = new SlideOutPanelViewModel() {
                Buttons  = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel { ClickAction = () => AddMenu(NewGameMenu), Title = "New Game" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(HelpMenu), Title = "Help" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(HighScoresMenu), Title = "High Scores" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(SettingsMenu), Title = "Settings" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(CreditsMenu), Title = "Credits" },
                },
                Background = Resources["Primary1"] as SolidColorBrush,
                Title = "Main Menu",
            };
            MainMenu.ControlIsVisible = true;
        }
    }
}

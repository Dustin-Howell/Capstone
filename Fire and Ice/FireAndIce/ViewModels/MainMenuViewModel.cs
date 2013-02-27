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
                    new OptionButtonViewModel {ClickAction = () => StartLocalAIGame(), Title = "AI"},
                },
                    Background = AppModel.Resources["Primary5"] as SolidColorBrush,
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
            BackgroundWorker hostGameWorker = new BackgroundWorker();
            BackgroundWorker connectServerWorker = new BackgroundWorker();

            Network network = new Network();

            hostGameWorker.DoWork += new DoWorkEventHandler((s, e) => network.server_hostGame("Game 1", "Player 1"));
            connectServerWorker.DoWork += new DoWorkEventHandler((s, e) => network.server_startGame());

            hostGameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => connectServerWorker.RunWorkerAsync());
            connectServerWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
                {
                    AppModel.AppViewModel.ActivateItem(new GameContainerViewModel(PlayerType.Human, PlayerType.Network, network));
                });

            hostGameWorker.RunWorkerAsync();
        }

        private void FindNetworkGame()
        {
            BackgroundWorker findGamesWorker = new BackgroundWorker();
            BackgroundWorker startGameWorker = new BackgroundWorker();
            
            Network network = new Network();

            string[,] gamesFound = new string[256,7];
            findGamesWorker.DoWork += new DoWorkEventHandler((s, e) => gamesFound = network.client_findGames());
            startGameWorker.DoWork += new DoWorkEventHandler((s, e) => network.client_ackStartGame());

            findGamesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                string[] gameToJoin = new string[7];
                //Choose first game -- will crash if run in wrong order.

                gameToJoin[0] = gamesFound[0, 0];
                gameToJoin[1] = gamesFound[0, 1];
                gameToJoin[2] = gamesFound[0, 2];
                gameToJoin[3] = gamesFound[0, 3];
                gameToJoin[4] = "7";
                gameToJoin[5] = "Player2";
                gameToJoin[6] = gamesFound[0, 6];

                network.client_joinGame(gameToJoin);
                startGameWorker.RunWorkerAsync();
            });

            startGameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                AppModel.AppViewModel.ActivateItem(new GameContainerViewModel(PlayerType.Network, PlayerType.Human, network));
            });

            findGamesWorker.RunWorkerAsync();
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
            AppModel.ResetCreeperCore();

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

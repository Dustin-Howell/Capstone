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
using CreeperAI;
using CreeperMessages;
using CreeperSound;
using System.Diagnostics;

namespace FireAndIce.ViewModels
{
    public class MainMenuViewModel : Screen
    {    
        private BindableCollection<ToggleButtonMenuViewModel> _menus;
        public BindableCollection<ToggleButtonMenuViewModel> Menus
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

        public ToggleButtonMenuViewModel MainMenu { get; set; }

        private PropertyChangedBase _popup;
        public PropertyChangedBase Popup
        {
            get
            {
                return _popup;
            }
            set
            {
                if (_popup as IDisposable != null)
                    ((IDisposable)_popup).Dispose();
                
                _popup = value;
                NotifyOfPropertyChange(() => Popup);
            }
        }

        public void AddMenu(ToggleButtonMenuViewModel panel)
        {
            //AppModel.EventAggregator.Publish(new SoundPlayMessage(SoundPlayType.MenuSlideOut));

            // Temporary workaround?
            if (AppModel.Network != null)
                AppModel.Network.quitHostGame();

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
                    BindableCollection<ToggleButtonMenuViewModel> newMenus = new BindableCollection<ToggleButtonMenuViewModel>(Menus);
                    bool foundParent = false;
                    foreach (ToggleButtonMenuViewModel menu in newMenus)
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

        private ToggleButtonMenuViewModel _newGameMenu;
        private ToggleButtonMenuViewModel NewGameMenu
        {
            get
            {
                return _newGameMenu = _newGameMenu ?? new ToggleButtonMenuViewModel()
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

        private ToggleButtonMenuViewModel _localGameMenu;
        private ToggleButtonMenuViewModel LocalGameMenu
        {
            get
            {
                return _localGameMenu = _localGameMenu ?? new ToggleButtonMenuViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => StartLocalHumanGame(), Title = "Human"},
                    new OptionButtonViewModel {ClickAction = () => AddMenu(LocalAIGameMenu), Title = "Computer"},
                },
                    Background = AppModel.Resources["Primary5"] as SolidColorBrush,
                    Title = "Against?",
                    MenuParent = NewGameMenu,
                };
            }
        }

        private ToggleButtonMenuViewModel _localAIGameMenu;
        private ToggleButtonMenuViewModel LocalAIGameMenu
        {
            get
            {
                return _localAIGameMenu = _localAIGameMenu ?? new ToggleButtonMenuViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => AddMenu(LocalEasyAIGameMenu), Title = "Novice"},
                    new OptionButtonViewModel {ClickAction = () => AddMenu(LocalHardAIGameMenu), Title = "Expert"},
                    new OptionButtonViewModel {ClickAction = () => StartAIvAIGame(), Title = "AI vs. AI (Debug)"},
                },
                    Background = AppModel.Resources["Primary1"] as SolidColorBrush,
                    Title = "Difficulty?",
                    MenuParent = LocalGameMenu,
                };
            }
        }

        private void StartAIvAIGame()
        {
            AppModel.EventAggregator.Publish(new StartGameMessage()
            {
                Settings = new GameSettings()
                {
                    AI = AppModel.AI,
                    Board = new CreeperBoard(),
                    Player1Type = PlayerType.AI,
                    Player2Type = PlayerType.AI,
                    StartingColor = CreeperColor.Fire,
                }
            });
        }

        private ToggleButtonMenuViewModel _localEasyAIGameMenu;
        private ToggleButtonMenuViewModel LocalEasyAIGameMenu
        {
            get
            {
                return _localEasyAIGameMenu = _localEasyAIGameMenu ?? new ToggleButtonMenuViewModel()
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
            AppModel.AI.Difficulty = AIDifficulty.Easy;
            AppModel.EventAggregator.Publish(new StartGameMessage()
            {
                Settings = new GameSettings()
                {
                    AI = AppModel.AI,
                    Board = new CreeperBoard(),
                    Player1Type = PlayerType.Human,
                    Player2Type = PlayerType.AI,
                    StartingColor = CreeperColor.Fire,
                }
            });
        }

        private ToggleButtonMenuViewModel _localHardAIGameMenu;
        private ToggleButtonMenuViewModel LocalHardAIGameMenu
        {
            get
            {
                return _localHardAIGameMenu = _localHardAIGameMenu ?? new ToggleButtonMenuViewModel()
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
            AppModel.AI.Difficulty = AIDifficulty.Hard;
            AppModel.EventAggregator.Publish(new StartGameMessage()
            {
                Settings = new GameSettings()
                {
                    AI = AppModel.AI,
                    Board = new CreeperBoard(),
                    Player1Type = PlayerType.Human,
                    Player2Type = PlayerType.AI,
                    StartingColor = CreeperColor.Fire,
                }
            });
        }

        private void StartLocalHumanGame()
        {
            AppModel.EventAggregator.Publish(new StartGameMessage()
            {
                Settings = new GameSettings()
                {
                    AI = AppModel.AI,
                    Board = new CreeperBoard(),
                    Player1Type = PlayerType.Human,
                    Player2Type = PlayerType.Human,
                    StartingColor = CreeperColor.Fire,
                }
            });
        }

        private ToggleButtonMenuViewModel _networkGameMenu;
        private ToggleButtonMenuViewModel NetworkGameMenu
        {
            get
            {
                return _networkGameMenu = _networkGameMenu ?? new ToggleButtonMenuViewModel()
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
            Popup = new FindGameViewModel(AppModel.EventAggregator) { Title = "Find a game." };
        }

        private ToggleButtonMenuViewModel _helpMenu;
        private ToggleButtonMenuViewModel HelpMenu
        {
            get
            {
                return _helpMenu = _helpMenu ?? new ToggleButtonMenuViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => { Popup = new InstructionsViewModel(); }, Title = "Instructions"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "Help",
                    MenuParent = MainMenu,
                };
            }
        }

        private ToggleButtonMenuViewModel _settingsMenu;
        private ToggleButtonMenuViewModel SettingsMenu
        {
            get
            {
                return _settingsMenu = _settingsMenu ?? new ToggleButtonMenuViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel {ClickAction = () => { AddMenu(SoundMenu); }, Title = "Sound"},
                },
                    Background = AppModel.Resources["Primary4"] as SolidColorBrush,
                    Title = "Option",
                    MenuParent = MainMenu,
                };
            }
        }

        private ToggleButtonMenuViewModel _soundMenu;
        private ToggleButtonMenuViewModel SoundMenu
        {
            get
            {
                return _soundMenu = _soundMenu ?? new ToggleButtonMenuViewModel
                {
                    Buttons = new BindableCollection<OptionButtonViewModel> {
                        new OptionButtonViewModel {ClickAction = () => SoundEngine.ToggleSound(false), Title = "On", IsOptionChecked = !SoundEngine.IsMuted,},
                        new OptionButtonViewModel {ClickAction = () => SoundEngine.ToggleSound(true), Title = "Off", IsOptionChecked = SoundEngine.IsMuted,},

                    },
                    Background = AppModel.Resources["Primary5"] as SolidColorBrush,
                };
            }
        }

        private ToggleButtonMenuViewModel _creditsMenu;
        private ToggleButtonMenuViewModel CreditsMenu
        {
            get
            {
                return _creditsMenu = _creditsMenu ?? new ToggleButtonMenuViewModel()
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
            AppModel.ResetAppModel();

            Menus = new BindableCollection<ToggleButtonMenuViewModel>();
            MainMenu = new ToggleButtonMenuViewModel() {
                Buttons  = new BindableCollection<OptionButtonViewModel> {
                    new OptionButtonViewModel { ClickAction = () => AddMenu(NewGameMenu), Title = "New Game" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(HelpMenu), Title = "Help" },
                    new OptionButtonViewModel { ClickAction = () => AddMenu(SettingsMenu), Title = "Option" },
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

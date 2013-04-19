using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.ComponentModel;
using CreeperNetwork;
using Creeper;
using CreeperMessages;
using System.Timers;

namespace FireAndIce.ViewModels
{
    internal class NetworkGameInfo
    {
        public string ServerIP { get; set; }
        public string ProtocolVersion { get; set; }
        public string GameName { get; set; }
        public string PlayerName { get; set; }
        public string FirstMove { get; set; }
        public string GameInstance { get; set; }

        public string[] ToArray()
        {
            string[] array = new string[9];
            array[0] = ServerIP;
            array[1] = ProtocolVersion;
            array[2] = GameName.Length.ToString();
            array[3] = GameName;
            array[4] = PlayerName.Length.ToString();
            array[5] = PlayerName;
            array[6] = FirstMove;
            array[8] = GameInstance;

            return array;
        }
    }

    public class FindGameViewModel : PropertyChangedBase, IDisposable, IHandle<StartGameMessage>, IHandle<ConnectionStatusMessage>, IHandle<NetworkErrorMessage>
    {
        private List<NetworkGameInfo> _gamesData;

        // Title of menu screen
        public string Title { get; set; }

        private Timer refreshTimer = new Timer();

        //Constructor
        public FindGameViewModel(IEventAggregator eventAggregator)
        {
            AppModel.Network.quitHostGame();

            refreshTimer.Elapsed += new ElapsedEventHandler((s, e) => RefreshFoundGames());
            // Set the Interval to 2000 milliseconds.
            refreshTimer.Interval = 2000;
            refreshTimer.Enabled = true;

            //ideally true...
            refreshTimer.AutoReset = true;

            eventAggregator.Subscribe(this);

            SearchForGames = "Searching for games...";

            AppModel.Network.quitHostGame();
        }

        // Dynamically bindable properties.

        private string _gameName;
        public string GameName
        {
            get
            {
                return _gameName ?? "";
            }
            set
            {
                if (_gameName != value)
                {
                    _gameName = value;
                    NotifyOfPropertyChange(() => GameName);
                }
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get
            {
                return _playerName ?? "";
            }
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    NotifyOfPropertyChange(() => PlayerName);
                    NotifyOfPropertyChange(() => CanFindGameClick);
                }
            }
        }

        private BindableCollection<string> _foundGames;
        public BindableCollection<string> FoundGames
        {
            get
            {
                return _foundGames;
            }
            set
            {
                _foundGames = value;
                NotifyOfPropertyChange(() => FoundGames);
                NotifyOfPropertyChange(() => HasFoundGames);
            }
        }

        private string _selectedFoundGame;
        public string SelectedFoundGame
        {
            get
            {
                return _selectedFoundGame;
            }
            set
            {
                _selectedFoundGame = value;
                NotifyOfPropertyChange(() => SelectedFoundGame);
                NotifyOfPropertyChange(() => CanFindGameClick);
            }
        }

        public bool HasFoundGames
        {
            get
            {
                return FoundGames != null && FoundGames.Any();
            }
        }

        public void RefreshFoundGames()
        {
            BackgroundWorker findGamesWorker = new BackgroundWorker();

            string[,] gamesFound = new string[256, 7];
            findGamesWorker.DoWork += new DoWorkEventHandler((s, e) => gamesFound = AppModel.Network.client_findGames(PlayerName));
            SearchForGames = "Searching for games...";

            findGamesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                BindableCollection<string> games = new BindableCollection<string>();
                List<NetworkGameInfo> gamesData = new List<NetworkGameInfo>();
                for (int i = 0; i < gamesFound.Length && gamesFound[i, 0] != null; i++)
                {
                    if(!games.Contains(gamesFound[i, 3]))
                    {
                        games.Add(gamesFound[i, 3]);
                        gamesData.Add(new NetworkGameInfo()
                            {
                                ServerIP = gamesFound[i, 0],
                                ProtocolVersion = gamesFound[i, 1],
                                GameName = gamesFound[i, 3],
                                PlayerName = gamesFound[i, 5],
                                FirstMove = gamesFound[i, 6],
                                GameInstance = gamesFound[i, 7]
                            });
                    }
                }

                SelectedFoundGame = games.FirstOrDefault((x) => x == SelectedFoundGame);
                FoundGames = games;
                _gamesData = gamesData;
            });

            findGamesWorker.RunWorkerAsync();
        }

        public bool CanFindGameClick
        {
            get
            {
                return SelectedFoundGame != null;
            }
        }

        public void FindGameClick()
        {
            AppModel.Network.client_joinGame(_gamesData.First(x => x.GameName == SelectedFoundGame).ToArray());
            BackgroundWorker startGameWorker = new BackgroundWorker();

            refreshTimer.Enabled = false;

            startGameWorker.DoWork += new DoWorkEventHandler((s, e) => e.Result = AppModel.Network.client_ackStartGame());
            startGameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) 
                => 
                {
                    if ((bool)e.Result)
                    {
                        AppModel.EventAggregator.Publish(
                            new StartGameMessage()
                            {
                                Settings = new GameSettings()
                                {
                                    Board = new CreeperBoard(),
                                    Player1Type = PlayerType.Network,
                                    Player2Type = PlayerType.Human,
                                    StartingColor = CreeperColor.Fire,
                                    Network = AppModel.Network,
                                }
                            });
                    }
                    else
                    {
                        refreshTimer.Enabled = true;
                        DisconnectedMessage = "Could not join server.\nGame is full or does\nnot exist.";
                    }
                    
                });
            startGameWorker.RunWorkerAsync();            
        }

        public void Dispose()
        {
            refreshTimer.Enabled = false;
            refreshTimer.Close();
        }

        public void Handle(StartGameMessage message)
        {
            Dispose();
        }

        public void Handle(NetworkErrorMessage message)
        {
            if (message.Type == NetworkErrorType.AckDisconnect)
            {
                DisconnectedMessage = "Cannot join that game.";
            }
        }

        private string _disconnectedMessage;
        public string DisconnectedMessage
        {
            get { return _disconnectedMessage; }
            set
            {
                _disconnectedMessage = value;
                NotifyOfPropertyChange(() => DisconnectedMessage);
            }
        }

        public void Handle(ConnectionStatusMessage message)
        {
            if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_UNPLUGGED)
            {
                SearchForGames = "No Connection";
                NetworkCableUnpluggedMessage = "No Connection!";
            }
            else if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_RECONNECTED)
            {
                SearchForGames = "Searching for games...";
                NetworkCableUnpluggedMessage = "Connection OK";
            }
        }

        private string _networkCableUnpluggedMessage;
        public string NetworkCableUnpluggedMessage
        {
            get { return _networkCableUnpluggedMessage; }
            set
            {
                _networkCableUnpluggedMessage = value;
                NotifyOfPropertyChange(() => NetworkCableUnpluggedMessage);
            }
        }

        private string _searchForGames;
        public string SearchForGames
        {
            get { return _searchForGames; }
            set
            {
                _searchForGames = value;
                NotifyOfPropertyChange(() => SearchForGames);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.ComponentModel;
using CreeperNetwork;
using Creeper;

namespace FireAndIce.ViewModels
{
    internal class NetworkGameInfo
    {
        public string ServerIP { get; set; }
        public string ProtocolVersion { get; set; }
        public string GameName { get; set; }
        public string PlayerName { get; set; }
        public string FirstMove { get; set; }

        public string[] ToArray()
        {
            string[] array = new string[7];
            array[0] = ServerIP;
            array[1] = ProtocolVersion;
            array[2] = GameName.Length.ToString();
            array[3] = GameName;
            array[4] = PlayerName.Length.ToString();
            array[5] = PlayerName;
            array[6] = FirstMove;

            return array;
        }
    }

    public class FindGameViewModel : PropertyChangedBase
    {
        private List<NetworkGameInfo> _gamesData;

        // Title of menu screen
        public string Title { get; set; }

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
            

            findGamesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                BindableCollection<string> games = new BindableCollection<string>();
                List<NetworkGameInfo> gamesData = new List<NetworkGameInfo>();
                for (int i = 0; i < gamesFound.Length && gamesFound[i, 0] != null; i++)
                {
                    games.Add(gamesFound[i, 3]);
                    gamesData.Add(new NetworkGameInfo()
                        {
                            ServerIP = gamesFound[i, 0],
                            ProtocolVersion = gamesFound[i, 1],
                            GameName = gamesFound[i, 3],
                            PlayerName = gamesFound[i, 5],
                            FirstMove = gamesFound[i, 6],
                        });
                }

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
            //TODO: Use with slim core
            throw new NotImplementedException("Not configured for slim core");

            AppModel.Network.client_joinGame(_gamesData.First(x => x.GameName == SelectedFoundGame).ToArray());
            BackgroundWorker startGameWorker = new BackgroundWorker();

            startGameWorker.DoWork += new DoWorkEventHandler((s, e) => AppModel.Network.client_ackStartGame());
            startGameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => AppModel.AppViewModel.ActivateItem(new GameContainerViewModel(PlayerType.Network, PlayerType.Human, AppModel.Network)));
            startGameWorker.RunWorkerAsync();
            
        }
    }
}

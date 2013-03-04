using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperNetwork;
using System.ComponentModel;
using Creeper;

namespace FireAndIce.ViewModels
{
    public class HostGameViewModel : PropertyChangedBase
    {
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

        private bool _canHostGame = true;
        public bool CanHostGame
        {
            get
            {
                return _canHostGame;
            }
            set
            {
                _canHostGame = value;
                NotifyOfPropertyChange(() => CanHostGame);
            }
        }

        public void HostGame()
        {
            BackgroundWorker hostGameWorker = new BackgroundWorker();
            BackgroundWorker connectServerWorker = new BackgroundWorker();

            Network network = new Network();

            hostGameWorker.DoWork += new DoWorkEventHandler((s, e) => network.server_hostGame(GameName, PlayerName));
            connectServerWorker.DoWork += new DoWorkEventHandler((s, e) => network.server_startGame());

            hostGameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => connectServerWorker.RunWorkerAsync());
            connectServerWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
                {
                    AppModel.AppViewModel.ActivateItem(new GameContainerViewModel(PlayerType.Human, PlayerType.Network, network));
                });

            CanHostGame = false;
            hostGameWorker.RunWorkerAsync();
        }
    }
}

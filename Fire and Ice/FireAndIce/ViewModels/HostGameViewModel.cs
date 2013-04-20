using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperNetwork;
using System.ComponentModel;
using Creeper;
using CreeperMessages;
using System.Windows;

namespace FireAndIce.ViewModels
{
    public enum HostGameStatus { NoName, Startable, Starting, }

    public class HostGameViewModel : PropertyChangedBase, IDisposable, IHandle<ConnectionStatusMessage>
    {
        private BackgroundWorker _hostGameWorker;
        BackgroundWorker _connectServerWorker;

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
                }
            }
        }

        public bool CanChangePlayerName
        {
            get
            {
                return HostGameStatus != HostGameStatus.Starting;
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
                    HostGameStatus = _playerName == null || _playerName == "" ? HostGameStatus.NoName : HostGameStatus.Startable;
                    NotifyOfPropertyChange(() => PlayerName);
                }
            }
        }

        private object _canHostGameLock = new object();
        private bool _canHostGame = true;
        public bool CanHostGame
        {
            get
            {
                return _canHostGame;
            }
            set
            {
                lock (_canHostGameLock)
                {
                    _canHostGame = value;
                    NotifyOfPropertyChange(() => CanHostGame);
                }
            }
        }

        public string HostGameStatusText
        {
            get
            {
                switch (HostGameStatus)
                {
                    case HostGameStatus.NoName:
                        return "Please Enter A Name";
                    case HostGameStatus.Startable:
                        return "Ready?";
                    case HostGameStatus.Starting:
                        return "Waiting for a player...";
                    default:
                        return "Ready?";
                }
            }
        }

        private HostGameStatus _hostGameStatus;
        private HostGameStatus HostGameStatus
        {
            get
            {
                return _hostGameStatus;
            }
            set
            {
                _hostGameStatus = value;
                CanHostGame = _hostGameStatus == HostGameStatus.Startable;
                NotifyOfPropertyChange(() => HostGameStatusText);
                NotifyOfPropertyChange(() => AbortVisibility);
                NotifyOfPropertyChange(() => CanChangePlayerName);
            }
        }

        public HostGameViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
            CanHostGame = true;
            HostGameStatus = HostGameStatus.NoName;
        }

        public void HostGame()
        {
            _hostGameWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _connectServerWorker = new BackgroundWorker();

            GameName = PlayerName + "'s Game";
            HostGameStatus = HostGameStatus.Starting;

            _hostGameWorker.DoWork += new DoWorkEventHandler((s, e) =>
                {
                    e.Cancel = !AppModel.Network.server_hostGame(GameName, PlayerName);
                });

            _hostGameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                if (e.Cancelled)
                {
                    CanHostGame = true;
                    HostGameStatus = HostGameStatus.Startable;
                }
                else
                {
                        _connectServerWorker.RunWorkerAsync();
                }
            });

            _connectServerWorker.DoWork += new DoWorkEventHandler(
                (s, e) => {
                    AppModel.Network.server_startGame();
                });

            _connectServerWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                    AppModel.EventAggregator.Publish(
                        new InitializeGameMessage()
                        {
                            Settings = new GameSettings()
                            {
                                Board = new CreeperBoard(),
                                Player1Type = PlayerType.Human,
                                Player2Type = PlayerType.Network,
                                StartingColor = CreeperColor.Fire,
                                Network = AppModel.Network,
                            }
                        });                    
            });


            CanHostGame = false;

            _hostGameWorker.RunWorkerAsync();
        }

        public void Abort()
        {
            if (AppModel.Network != null)
                AppModel.Network.quitHostGame();
        }

        public Visibility AbortVisibility
        {
            get
            {
                return HostGameStatus == HostGameStatus.Starting ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public void Handle(ConnectionStatusMessage message)
        {
            if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_UNPLUGGED)
            {
                NetworkCableUnpluggedMessage = "No Connection!";
            }
            else if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_RECONNECTED)
            {
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

        public void Dispose()
        {
            if (AppModel.Network != null)
                AppModel.Network.quitHostGame();
        }
    }
}

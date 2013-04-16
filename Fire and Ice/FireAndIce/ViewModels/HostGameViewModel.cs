using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperNetwork;
using System.ComponentModel;
using Creeper;
using CreeperMessages;

namespace FireAndIce.ViewModels
{
    public class HostGameViewModel : PropertyChangedBase, IHandle<ConnectionStatusMessage>
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

        public HostGameViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void HostGame()
        {
            _hostGameWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _connectServerWorker = new BackgroundWorker();

            GameName = PlayerName + "'s Game";

            _hostGameWorker.DoWork += new DoWorkEventHandler((s, e) =>
                {
                    AppModel.Network.server_hostGame(GameName, PlayerName);
                });

            _hostGameWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) =>
            {
                if (e.Cancelled)
                {
                    CanHostGame = true;
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
                if(AppModel.Network.isServer)
                {
                    AppModel.EventAggregator.Publish(
                        new StartGameMessage()
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
                }
            });

            if(AppModel.Network.isServer)
            {
                CanHostGame = false;

                _hostGameWorker.RunWorkerAsync();
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
    }
}

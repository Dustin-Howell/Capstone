using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperCore;
using FireAndIce.Views;
using CreeperNetwork;
using System.Windows.Media;
using Creeper;
using CreeperMessages;
using XNAControlGame;
using CreeperSound;

namespace FireAndIce.ViewModels
{
    class GameContainerViewModel : Screen, IHandle<NetworkErrorMessage>, IHandle<MoveMessage>, IHandle<ChatMessage>, IHandle<ConnectionStatusMessage>
    {
        private GameSettings _settings;

        //private SlideOutPanelViewModel _gameMenu;
        public ToggleButtonMenuViewModel GameMenu
        {
            get
            {
                return new ToggleButtonMenuViewModel()
                {
                    Buttons = new BindableCollection<OptionButtonViewModel>()
                        {
                            new OptionButtonViewModel() { ClickAction = ReturnToMainMenu, Title = "Quit" },
                        },
                    Background = AppModel.Resources["Primary1"] as SolidColorBrush,
                    Title = "Game Menu",
                };
            }
        }

        private bool _isNetworkGame;
        public bool IsNetworkGame
        {
            get { return _isNetworkGame; }
            set
            {
                _isNetworkGame = value;
                NotifyOfPropertyChange(() => IsNetworkGame);
                NotifyOfPropertyChange(() => CanSendMessage);
            }
        }

        private String _gameOverText;
        public String GameOverText
        {
            get { return _gameOverText ?? ""; }
            set
            {
                _gameOverText = value;
                NotifyOfPropertyChange(() => GameOverText);
            }
        }

        private string _currentTurn = AppModel.SlimCore.GetCurrentPlayer().Color.ToString();
        public String CurrentTurn
        {
            get { return _currentTurn; }
            set
            {
                _currentTurn = value;
                NotifyOfPropertyChange(() => CurrentTurn);
            }
        }

        private BindableCollection<String> _chatMessages;
        public BindableCollection<String> ChatMessages
        {
            get { return _chatMessages = _chatMessages ?? new BindableCollection<String>(); }
            set
            {
                _chatMessages = value;
                NotifyOfPropertyChange(() => ChatMessages);
            }
        }

        private String _message;
        public String Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        public Game1 Game { get { return AppModel.XNAGame; } }

        public GameContainerViewModel(GameSettings settings)
        {
            _settings = settings;
            AppModel.EventAggregator.Subscribe(this);

            if (_settings.Player1Type == PlayerType.Network
                || _settings.Player2Type == PlayerType.Network)
            {
                IsNetworkGame = true;
                UndoVisible = System.Windows.Visibility.Collapsed;
                QuitVisible = System.Windows.Visibility.Collapsed;
            }
            else
                ForfeitVisible = System.Windows.Visibility.Collapsed;
        }

        public void ReturnToMainMenu()
        {
            AppModel.EventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.Disconnect));
            AppModel.EventAggregator.Publish(new ReturnToMenuMessage());
        }

        public void Forfeit()
        {
            AppModel.EventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.Forfeit));
            AppModel.EventAggregator.Publish(new ReturnToMenuMessage());
        }

        public void Handle(NetworkErrorMessage message)
        {
            GameOverText = String.Format("{0} Error!", "Network");
        }

        public void Handle(MoveMessage message)
        {
            CurrentTurn = message.TurnColor.ToString();
        }

        public void SendMessage()
        {
            AppModel.EventAggregator.Publish(new ChatMessage(AppModel.Network.getSelfName() + ": " + Message, ChatMessageType.Send));
            Message = "";
        }

        public bool CanSendMessage
        {
            get { return IsNetworkGame; }
        }

        public void Undo()
        {
            AppModel.EventAggregator.Publish(new MoveMessage() { Type = MoveMessageType.Undo, });
        }

        public void ToggleSound()
        {
            SoundEngine.ToggleSound();
        }

        public void Handle(ChatMessage message)
        {
            if (message.Type == ChatMessageType.Send)
                ChatMessages.Add(message.Message);
            else
                ChatMessages.Add(AppModel.Network.getOpponentName() + message.Message);
        }

        private System.Windows.Visibility _undoVisible;
        public System.Windows.Visibility UndoVisible
        {
            get { return _undoVisible; }
            set
            {
                _undoVisible = value;
                NotifyOfPropertyChange(() => UndoVisible);
            }
        }

        private System.Windows.Visibility _quitVisible;
        public System.Windows.Visibility QuitVisible
        {
            get { return _quitVisible; }
            set
            {
                _quitVisible = value;
                NotifyOfPropertyChange(() => QuitVisible);
            }
        }

        private System.Windows.Visibility _forfeitVisible;
        public System.Windows.Visibility ForfeitVisible
        {
            get { return _forfeitVisible; }
            set
            {
                _forfeitVisible = value;
                NotifyOfPropertyChange(() => ForfeitVisible);
            }
        }

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

        int stillNotConnected = 0;

        public void Handle(ConnectionStatusMessage message)
        {
            if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_UNPLUGGED)
            {
                if (stillNotConnected < 1)
                {
                    Popup = new InGameConnectionViewModel(message);
                    stillNotConnected++;
                }
            }
            else if (message.ErrorType == CONNECTION_ERROR_TYPE.CONNECTION_LOST)
            {
                if (stillNotConnected < 1)
                {
                    Popup = new InGameConnectionViewModel(message);
                    stillNotConnected++;
                }
            }
            else if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_RECONNECTED)
            {
                if (stillNotConnected != 0)
                {
                    Popup = new InGameConnectionViewModel(message);
                    stillNotConnected = 0;
                }
            }
            else if (message.ErrorType == CONNECTION_ERROR_TYPE.RECONNECTED)
            {
                if (stillNotConnected != 0)
                {
                    Popup = new InGameConnectionViewModel(message);
                    stillNotConnected = 0;
                }
            }
        }
    }
}

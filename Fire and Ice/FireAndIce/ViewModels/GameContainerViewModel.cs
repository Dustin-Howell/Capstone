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
    class GameContainerViewModel : Screen, IHandle<NetworkErrorMessage>, IHandle<MoveMessage>, IHandle<ChatMessage>
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
            //get { return _isNetworkGame; }
            get { return true; }
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
            }
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
            ChatMessages.Add(AppModel.Network.getOpponentName() + ": " + message.Message);
        }
    }
}

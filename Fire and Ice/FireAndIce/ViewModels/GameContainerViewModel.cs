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

namespace FireAndIce.ViewModels
{
    class GameContainerViewModel : Screen, IHandle<NetworkErrorMessage>, IHandle<MoveMessage>, IHandle<ChatMessage>
    {
        private PlayerType _player1Type;
        private PlayerType _player2Type;
        private Network _network = null;
        private AIDifficulty _aiDifficulty = AIDifficulty.Hard;
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

        public bool IsNetworkGame
        {
            get { return _network != null; }
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

        public String CurrentTurn
        {
            get
            {
                return "Fix this.";
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
        }

        public void ReturnToMainMenu()
        {
            AppModel.AppViewModel.ActivateItem(new MainMenuViewModel());
        }

        public void Forfeit()
        {
            AppModel.EventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.Forfeit));
            ReturnToMainMenu();
        }

        public void Handle(NetworkErrorMessage message)
        {
            GameOverText = String.Format("{0} Error!", "Network");
        }

        public void Handle(MoveMessage message)
        {
            NotifyOfPropertyChange(() => CurrentTurn);
        }

        public void SendMessage()
        {
            AppModel.EventAggregator.Publish(new ChatMessage(Message, ChatMessageType.Send));
            Message = "";
        }

        public void Undo()
        {
            AppModel.EventAggregator.Publish(new MoveMessage() { Type = MoveMessageType.Undo, });
        }

        public void Handle(ChatMessage message)
        {
            ChatMessages.Add(message.Message);
        }
    }
}

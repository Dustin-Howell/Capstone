using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Caliburn.Micro;
using CreeperCore;
using FireAndIce.Views;
using CreeperNetwork;
using System.Windows.Media;
using Creeper;
using CreeperMessages;
using XNAControlGame;
using CreeperSound;
using System.Windows.Controls;
using System.Windows;

namespace FireAndIce.ViewModels
{
    class GameContainerViewModel : Screen, IHandle<NetworkErrorMessage>, IHandle<MoveMessage>, IHandle<ChatMessage>, IHandle<ConnectionStatusMessage>, IHandle<GameOverMessage>
    {
        private GameSettings _settings;

        private bool isNetworkProblem = false;

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

        private string _currentTurn;
        public String CurrentTurn
        {
            get { return _currentTurn; }
            set
            {
                _currentTurn = value;
                NotifyOfPropertyChange(() => CurrentTurn);
            }
        }

        private string _initialTurnText;
        public string InitialTurnText
        {
            get { return _initialTurnText; }
            set
            {
                _initialTurnText = value;
                NotifyOfPropertyChange(() => InitialTurnText);
            }
        }

        private Uri _playMusic;
        public Uri PlayMusic
        {
            get { return _playMusic; }
            set
            {
                _playMusic = value;
              //  NotifyOfPropertyChange(() => PlayMusic);
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

        public GameContainerViewModel(GameSettings settings, IProvideBoardState boardProvider)
        {
            _settings = settings;
            _boardProvider = boardProvider;
            CurrentTurn = settings.StartingColor.ToString();
            AppModel.EventAggregator.Subscribe(this);

            if (_settings.Player1Type == PlayerType.Network
                || _settings.Player2Type == PlayerType.Network)
            {
                IsNetworkGame = true;
                InitialTurnText = _settings.Player1Type == PlayerType.Human ? "Your Turn" : "Opponent's Turn";
                UndoVisible = System.Windows.Visibility.Collapsed;
                QuitVisible = System.Windows.Visibility.Collapsed;
            }
            else if ((_settings.Player1Type == PlayerType.AI || _settings.Player2Type == PlayerType.AI) 
                && AppModel.AI.Difficulty == AIDifficulty.Hard)
            {
                UndoVisible = System.Windows.Visibility.Collapsed;
                ForfeitVisible = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ForfeitVisible = System.Windows.Visibility.Collapsed;
            }

            //Yeah...not what best way to do this. Temporary, or permanent, depending on time. 
            String path = Path.GetFullPath("Music");
            String soundFile = "\\";
            String actualFile = path + soundFile + "InGameMusic.mp3";
            PlayMusic = new Uri(actualFile, UriKind.Relative);
        }

        private MediaElement _musicPlayer;

        protected override void OnViewLoaded(object view)
        {
            //***********************************************************
            //MUSIC CODE
            //***********************************************************
            GameContainerView gameContainerView = view as GameContainerView;

            gameContainerView.MusicPlayer.Source = PlayMusic;
            gameContainerView.MusicPlayer.LoadedBehavior = MediaState.Manual;
            gameContainerView.MusicPlayer.Loaded += new RoutedEventHandler((s, e) =>
                {
                    gameContainerView.MusicPlayer.Play();
                });
            gameContainerView.MusicPlayer.MediaEnded += new RoutedEventHandler((s, e) =>
                {
                    gameContainerView.MusicPlayer.Position = TimeSpan.FromSeconds(0);
                });

            _musicPlayer = gameContainerView.MusicPlayer;

            base.OnViewLoaded(view);
            //***********************************************************
            //END MUSIC CODE
            //***********************************************************
        }

        public void ReturnToMainMenu()
        {
            AppModel.EventAggregator.Publish(new ReturnToMenuMessage());
        }

        public void Forfeit()
        {
            AppModel.EventAggregator.Publish(new NetworkErrorMessage(NetworkErrorType.ForfeitMessage));
        }

        public void Handle(NetworkErrorMessage message)
        {
            //GameOverText = String.Format("{0} Error!", "Network");
        }

        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Request)
            {
                if (_settings.Player2Type == PlayerType.AI)
                {
                    CurrentTurn = message.TurnColor.ToString();
                    CanUndo = _boardProvider.BoardHistory.Count > 2;
                }
                else if (_settings.Player1Type == PlayerType.AI)
                {
                    CurrentTurn = message.TurnColor.ToString();
                    CanUndo = _boardProvider.BoardHistory.Count > 3;
                }
                else
                {
                    CurrentTurn = message.TurnColor.ToString();
                    CanUndo = _boardProvider.BoardHistory.Count > 1;
                }
                if (message.PlayerType == PlayerType.AI)
                {
                    CanUndo = false;
                }
            }
            else if (message.Type == MoveMessageType.Response)
            {
                CanUndo = false;
            }
        }

        public void SendMessage()
        {
            if(!isNetworkProblem)
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

        private bool _canUndo;
        public bool CanUndo { get { return _canUndo; } set { _canUndo = value; NotifyOfPropertyChange(() => CanUndo); } }

        public void ToggleSound()
        {
            SoundEngine.ToggleSound();
            _musicPlayer.IsMuted = SoundEngine.IsMuted;
        }

        public void Handle(ChatMessage message)
        {
            if (message.Type == ChatMessageType.Send)
                ChatMessages.Add(message.Message);
            else
                ChatMessages.Add(message.Message);
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
        private IProvideBoardState _boardProvider;

        public void Handle(ConnectionStatusMessage message)
        {
            if (IsNetworkGame && AppModel.Network.isGameRunning())
            {
                if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_UNPLUGGED)
                {
                    if (stillNotConnected < 1)
                    {
                        Popup = new InGameConnectionViewModel(message);
                        stillNotConnected++;
                    }
                    isNetworkProblem = true;
                }
                else if (message.ErrorType == CONNECTION_ERROR_TYPE.CONNECTION_LOST)
                {
                    if (stillNotConnected < 1)
                    {
                        Popup = new InGameConnectionViewModel(message);
                        stillNotConnected++;
                    }
                    isNetworkProblem = true;
                }
                else if (message.ErrorType == CONNECTION_ERROR_TYPE.CABLE_RECONNECTED)
                {
                    if (stillNotConnected != 0)
                    {
                        Popup = new InGameConnectionViewModel(message);
                        stillNotConnected = 0;
                    }
                    isNetworkProblem = false;
                }
                else if (message.ErrorType == CONNECTION_ERROR_TYPE.RECONNECTED)
                {
                    if (stillNotConnected != 0)
                    {
                        Popup = new InGameConnectionViewModel(message);
                        stillNotConnected = 0;
                    }
                    isNetworkProblem = false;
                }
            }
        }

        private Visibility _gameOverVisibility = Visibility.Collapsed;
        public Visibility GameOverVisibility
        {
            get { return _gameOverVisibility; }
            set
            {
                _gameOverVisibility = value;
                NotifyOfPropertyChange(() => GameOverVisibility);
            }
        }

        public void Handle(GameOverMessage message)
        {
            if (message.Winner.HasValue)
            {
                GameOverText = String.Format("{0} wins!",message.Winner.ToString());
            }
            else
            {
                GameOverText = "Draw!";
            }

            GameOverVisibility = Visibility.Visible;
        }

        public void ReturnToMenu()
        {
            AppModel.EventAggregator.Publish(new ReturnToMenuMessage());
        }
    }
}

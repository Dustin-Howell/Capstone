using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperMessages;
using Creeper;

namespace CreeperCore
{
    public class SlimCore : IProvideBoardState, IHandle<MoveMessage>, IHandle<NetworkErrorMessage>, IHandle<InitializeGameMessage>, IHandle<ComponentInitializedMessage>
    {
        private IEventAggregator _eventAggregator;
        private Player _player1;
        private Player _player2;

        //State Variables
        private CreeperBoard _board
        {
            get
            {
                if (_boardHistory.Count > 0)
                {
                    return _boardHistory.Peek();
                }
                else
                {
                    throw new IndexOutOfRangeException("No boards left in board history.");
                }
            }
        }
        private Player _currentPlayer;

        private Stack<CreeperBoard> _boardHistory;

        public SlimCore(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _boardHistory = new Stack<CreeperBoard>();
            _boardHistory.Push(new CreeperBoard());
        }

        private void StartGame(GameSettings settings)
        {
            _player1 = new Player(settings.Player1Type, settings.StartingColor);
            _player2 = new Player(settings.Player2Type, settings.StartingColor.Opposite());
            _currentPlayer = _player1;
            _boardHistory.Clear();
            _boardHistory.Push(settings.Board);

            //if networked game
            if (_player1.Type == PlayerType.Network
                || _player2.Type == PlayerType.Network)
            {
                _eventAggregator.Subscribe(settings.Network);
            }

            //else if ai game
            if (_player1.Type == PlayerType.AI
                || _player2.Type == PlayerType.AI)
            {
                _eventAggregator.Subscribe(settings.AI);
            }
        }

        #region IProvideBoardState
        public CreeperBoard GetBoard()
        {
            return new CreeperBoard(_board);
        }

        public Player GetCurrentPlayer()
        {
            return new Player(_currentPlayer);
        }

        public Stack<CreeperBoard> BoardHistory { get { return new Stack<CreeperBoard>(_boardHistory); } }
        #endregion

        #region IHandle
        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.MoveMade)
            {
                if (message.PlayerType != _currentPlayer.Type)
                {
                    throw new InvalidOperationException(String.Format("Player type of {0} should not be making move now", message.PlayerType.ToString()));
                }

                if (message.TurnColor != _currentPlayer.Color)
                {
                    throw new InvalidOperationException(String.Format("Color: {0} should not be moving now", message.TurnColor.ToString()));
                }

                CreeperBoard board = new CreeperBoard(_board);
                board.Move(message.Move);
                _boardHistory.Push(board);

                switch (_board.GetGameState(_currentPlayer.Color))
                {
                    case CreeperGameState.Unfinished:
                        _currentPlayer = (_currentPlayer == _player1) ? _player2 : _player1;
                        RequestMove();
                        break;
                    case CreeperGameState.Complete:
                        _eventAggregator.Publish(new GameOverMessage() { Winner = _currentPlayer.Color, });
                        _currentPlayer = new Player(PlayerType.Invalid, CreeperColor.Invalid);
                        break;
                    case CreeperGameState.Draw:
                        _eventAggregator.Publish(new GameOverMessage() { Winner = null, });
                        _currentPlayer = new Player(PlayerType.Invalid, CreeperColor.Invalid);
                        break;
                }
            }
            else if (message.Type == MoveMessageType.Undo)
            {
                _boardHistory.Pop();
                _currentPlayer = (_currentPlayer == _player1) ? _player2 : _player1;
                _eventAggregator.Publish(new SychronizeBoardMessage() { Board = _board, });
                RequestMove();
            }
        }

        public void Handle(InitializeGameMessage message)
        {
            StartGame(message.Settings);
        }

        private List<String> _initializedComponents = new List<String>();
        public void Handle(ComponentInitializedMessage message)
        {
            _initializedComponents.Add(message.Component.ToString());

            if (Enum.GetNames(typeof(InitComponent)).All(x => _initializedComponents.Contains(x.ToString())))
            {
                RequestMove();
            }
        }

        public void Handle(NetworkErrorMessage message)
        {
            if (message.Type == NetworkErrorType.OpponentForfeit)
            {
                _currentPlayer = new Player(PlayerType.Invalid, CreeperColor.Invalid);

                Player forfeitWinner;

                if(_player1.Type == PlayerType.Network)
                    forfeitWinner = _player2;
                else
                    forfeitWinner = _player1;

                _eventAggregator.Publish(new GameOverMessage()
                {
                    Winner = forfeitWinner.Color,
                });
            }
            else if (message.Type == NetworkErrorType.Forfeit)
            {
                _currentPlayer = new Player(PlayerType.Invalid, CreeperColor.Invalid);

                Player forfeitWinner;

                if (_player1.Type == PlayerType.Network)
                    forfeitWinner = _player1;
                else
                    forfeitWinner = _player2;

                _eventAggregator.Publish(new GameOverMessage()
                {
                    Winner = forfeitWinner.Color,
                });
            }
            else if (message.Type == NetworkErrorType.Disconnect)
            {
                _currentPlayer = new Player(PlayerType.Invalid, CreeperColor.Invalid);

                Player disconnectWinner;
                
                if (_player1.Type == PlayerType.Network)
                    disconnectWinner = _player1;
                else
                    disconnectWinner = _player2;
            }
            else if (message.Type == NetworkErrorType.OpponentDisconnect)
            {
                _currentPlayer = new Player(PlayerType.Invalid, CreeperColor.Invalid);

                Player disconnectWinner;

                if (_player1.Type == PlayerType.Network)
                    disconnectWinner = _player2;
                else
                    disconnectWinner = _player1;
            }
            else if (message.Type == NetworkErrorType.IllegalMove)
            {
                _currentPlayer = new Player(PlayerType.Invalid, CreeperColor.Invalid);

                Player illegalMoveWinner;

                if (_player1.Type == PlayerType.Network)
                    illegalMoveWinner = _player2;
                else
                    illegalMoveWinner = _player1;

                Console.WriteLine("ILLEGAL MOVE");

                _eventAggregator.Publish(new GameOverMessage()
                {
                    Winner = illegalMoveWinner.Color,
                });
            }
        }
        #endregion

        private void RequestMove()
        {
            _eventAggregator.Publish(new MoveMessage()
            {
                PlayerType = _currentPlayer.Type,
                Type = MoveMessageType.Request,
                Board = new CreeperBoard(_board),
                TurnColor = _currentPlayer.Color,
            });
        }
    }
}

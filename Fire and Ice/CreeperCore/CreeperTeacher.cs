using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using Caliburn.Micro;
using CreeperMessages;
using System.Threading;

namespace CreeperCore
{
    public class CreeperTeacher : IProvideBoardState, IHandle<MoveMessage>, IHandle<StartGameMessage>
    {
        private IEventAggregator _eventAggregator;
        private Player _teacher;
        private Player _student;
        private Player _currentPlayer;

        private Queue<Move> _teacherMoves = new Queue<Move>();
        private Queue<Move> _studentMoves = new Queue<Move>();

        private CreeperBoard _board;

        public CreeperTeacher(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            //enqueue all the moves
            _studentMoves.Enqueue(new Move(new Position(1, 0), new Position(1, 1), CreeperColor.Fire));

            _teacherMoves.Enqueue(new Move(new Position(4, 0), new Position(4, 1), CreeperColor.Ice));
        }

        public CreeperBoard GetBoard()
        {
            return _board;
        }

        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Response)
            {
                _board.Move(message.Move);
                _currentPlayer = (_currentPlayer == _student)? _teacher : _student;

                if (_studentMoves.Count > 0 || _teacherMoves.Count > 0)
                {
                    _eventAggregator.Publish(new MoveMessage()
                    {
                        PlayerType = PlayerType.Tutorial,
                        Type = MoveMessageType.Request,
                    });
                }
            }

            else
            {
                Move move = (_currentPlayer == _student) ? _studentMoves.Dequeue() : _teacherMoves.Dequeue();
                _eventAggregator.Publish(new MoveMessage()
                {
                    Move = move,
                    PlayerType = PlayerType.Tutorial,
                    Type = MoveMessageType.Response,
                    Board = _board,
                    TurnColor = _currentPlayer.Color,
                });
            }
        }

        public Player GetCurrentPlayer()
        {
            throw new NotImplementedException();
        }

        public void Handle(StartGameMessage message)
        {
            if (message.Settings.Player1Type == PlayerType.Tutorial
                || message.Settings.Player2Type == PlayerType.Tutorial)
            {
                _board = message.Settings.Board;

                _student = new Player(PlayerType.Tutorial, CreeperColor.Fire);
                _teacher = new Player(PlayerType.Tutorial, CreeperColor.Ice);

                _currentPlayer = _student;

                _eventAggregator.Publish(new MoveMessage()
                {
                    PlayerType = PlayerType.Tutorial,
                    Type = MoveMessageType.Request,
                });
            }
            else
            {
                _eventAggregator.Unsubscribe(this);
            }
        }
    }
}

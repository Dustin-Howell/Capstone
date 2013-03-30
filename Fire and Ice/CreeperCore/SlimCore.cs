using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperMessages;
using Creeper;
using CreeperNetwork;
using CreeperAI;
using XNAControlGame;

namespace CreeperCore
{
    public class SlimCore : IProvideBoardState, IHandle<MoveMessage>, IHandle<NetworkErrorMessage>
    {
        private EventAggregator _eventAggregator;
        private Player _player1;
        private Player _player2;

        //Keeping a reference to the network so the GC doesn't eat it.
        private IHandle _networkReference;
        private IHandle _aiReference;

        //State Variables
        private CreeperBoard _board;
        private Player _currentPlayer;

        public SlimCore()
        {            
            _board = new CreeperBoard();
        }

        public void StartGame(GameSettings settings)
        {
            _eventAggregator = settings.EventAggregator;
            _eventAggregator.Subscribe(this);
            _player1 = new Player(settings.Player1Type, settings.StartingColor);
            _player2 = new Player(settings.Player2Type, settings.StartingColor.Opposite());
            _currentPlayer = _player1;

            //if networked game
            if (_player1.Type == PlayerType.Network
                || _player2.Type == PlayerType.Network)
            {
                _networkReference = settings.Network;
                _eventAggregator.Subscribe(_networkReference);
            }

            //else if ai game
            if (_player1.Type == PlayerType.AI
                || _player2.Type == PlayerType.AI)
            {
                _aiReference = settings.AI;
                _eventAggregator.Subscribe(_aiReference);
            }

            RequestMove();
        }

        public CreeperBoard GetBoard()
        {
            return new CreeperBoard(_board);
        }

        public CreeperColor GetCurrentTurn()
        {
            return _currentPlayer.Color;
        }

        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Response)
            {
                //TODO: throw some exceptions if something went wrong

                _board.Move(message.Move);

                if (!_board.IsFinished(_currentPlayer.Color))
                {
                    _currentPlayer = (_currentPlayer == _player1) ? _player2 : _player1;
                    RequestMove();
                }
                else
                {
                    _eventAggregator.Publish(new GameOverMessage() { Winner = _currentPlayer.Color, });
                }
            }
        }

        public void Handle(NetworkErrorMessage message)
        {
            throw new NotImplementedException("Core did not handle network error.");
        }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CreeperMessages;
using Creeper;
using CreeperNetwork;
using CreeperAI;

namespace CreeperCore
{
    public class SlimCore : IHandle<MoveMessage>, IHandle<GameOverMessage>
    {
        private EventAggregator _eventAggregator;
        private Player _player1;
        private Player _player2;

        //Keeping a reference to the network so the GC doesn't eat it.
        private Network _networkReference;
        private AI _aiReference;

        //State Variables
        private CreeperBoard _board;
        private Player _currentPlayer;

        public void StartGame(GameSettings settings)
        {
            _eventAggregator = settings.EventAggregator;
            _player1 = new Player(settings.Player1Type, settings.StartingColor);
            _player2 = new Player(settings.Player2Type, settings.StartingColor.Opposite());

            //if networked game
            if (_player1.PlayerType == PlayerType.Network
                || _player2.PlayerType == PlayerType.Network)
            {
                _networkReference = settings.Network;
                _eventAggregator.Subscribe(_networkReference);
            }

            //else if ai game
            if (_player1.PlayerType == PlayerType.AI
                || _player2.PlayerType == PlayerType.AI)
            {
                _aiReference = settings.AI;
                _eventAggregator.Subscribe(_aiReference);
            }
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
                    _eventAggregator.Publish(new MoveMessage(_currentPlayer.PlayerType, MoveMessageType.Request));
                }
                else
                {
                    _eventAggregator.Publish(new GameOverMessage(GameOverType.Win, _currentPlayer));
                }
            }
        }

        public void Handle(GameOverMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

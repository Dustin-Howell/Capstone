using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using CreeperNetwork;
using XNAControlGame;
using System.ComponentModel;
using System.IO;
using CreeperAI;
using Caliburn.Micro;
using CreeperMessages;

namespace CreeperCore
{
    public class CreeperGameCore : IHandle<MoveResponseMessage>
    {
        public Game1 XNAGame
        {
            get
            {
                return _xnaGame;
            }
            set
            {
                _xnaGame = value;
            }
        }

        private Game1 _xnaGame;
        private CreeperAI.CreeperAI _AI;
        private Network _network;
        private IEventAggregator _eventAggregator;
        private BackgroundWorker _getAIMoveWorker;
        private BackgroundWorker _networkPlayGame;
        private bool _IsNetworkGame { get { return GameTracker.Player1.PlayerType == PlayerType.Network || GameTracker.Player2.PlayerType == PlayerType.Network; } }
        

        public CreeperGameCore(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Publish(new GameOverMessage());

            GameTracker.Board = new CreeperBoard();
        }

        public void InitializeGameGUI(IntPtr handle, int width, int height)
        {
            //TODO: Figure this out
            //XNAGame = AppModel
        }

        public void StartLocalGame(PlayerType player1Type, PlayerType player2Type, AIDifficulty difficulty)
        {
            if (player1Type == PlayerType.Network || player2Type == PlayerType.Network)
            {
                throw new ArgumentException("Cannot start a local game with a player type of network.");
            }
            if (player1Type == PlayerType.AI || player2Type == PlayerType.AI)
            {
                //TODO: Think this through more thoroughly later
                _AI = new CreeperAI.CreeperAI(_eventAggregator)
                {
                    TerritorialWeight = 15d,
                    MaterialWeight = 84d,
                    PositionalWeight = 2d,
                    PathHueristicWeight = 43d,
                    VictoryWeight = 100000,
                    Difficulty = difficulty,
                };
            }

            GameTracker.Player1 = new Player(player1Type, CreeperColor.Fire);
            GameTracker.Player2 = new Player(player2Type, CreeperColor.Ice);
            GameTracker.CurrentPlayer = GameTracker.Player1;
            GetNextMove();
        }

        public void StartNetworkGame(PlayerType player1Type, PlayerType player2Type, Network network)
        {
            if (player1Type == PlayerType.Network && player2Type == PlayerType.Network)
            {
                throw new ArgumentException("Cannot start network game where both players are network players.");
            }

            _network = network;

            _networkPlayGame = new BackgroundWorker();
            _networkPlayGame.DoWork += new DoWorkEventHandler((s, e) => _network.playGame());

            GameTracker.Player1 = new Player(player1Type, CreeperColor.Fire);
            GameTracker.Player2 = new Player(player2Type, CreeperColor.Ice);
            GameTracker.CurrentPlayer = GameTracker.Player1;
            GetNextMove();

            _networkPlayGame.RunWorkerAsync();
        }

        private void GetNextMove()
        {
            _eventAggregator.Publish(
                new MoveRequestMessage() 
            { 
                Responder = GameTracker.CurrentPlayer.PlayerType, 
                Color = GameTracker.CurrentPlayer.Color, 
            });
        }

        public void Handle(MoveResponseMessage message)
        {
            GameTracker.Board.Move(message.Move);

            if (!GameTracker.Board.IsFinished(message.Move.PlayerColor))
            {
                GameTracker.CurrentPlayer = GameTracker.OpponentPlayer;

                GetNextMove();
            }
            else
            {
                _eventAggregator.Publish(new GameOverMessage() { Winner = GameTracker.CurrentPlayer.Color, });
            }
        }
    }
}

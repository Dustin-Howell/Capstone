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
        private CreeperBoard _board;
        private bool _IsNetworkGame { get { return GameTracker.Player1.PlayerType == PlayerType.Network || GameTracker.Player2.PlayerType == PlayerType.Network; } }
        //public event EventHandler<GameOverMessage> GameOver;

        public CreeperGameCore(IEventAggregator eventAggregator)
        {
            InitializeBackgroundWorkers();
            _eventAggregator = eventAggregator;
            _eventAggregator.Publish(new GameOverMessage());

            GameTracker.Board = new CreeperBoard();
        }

        private void _xnaGame_UserMadeMove(object sender, MoveResponseMessage e)
        {
            if (e.Move.PlayerColor == GameTracker.CurrentPlayer.Color
                && GameTracker.CurrentPlayer.PlayerType == PlayerType.Human)
            {
                MakeMove(e.Move);
            }
        }

        void _network_MoveMade(object sender, MoveResponseMessage e)
        {
            e.Move.PlayerColor = GameTracker.CurrentPlayer.Color;
            MakeMove(e.Move);
        }

        private void MakeMove(Move move)
        {
            GameTracker.Board.Move(move);

            if (GameTracker.OpponentPlayer.PlayerType == PlayerType.Network)
            {
                _network.move(move);
            }

            if (!GameTracker.Board.IsFinished(move.PlayerColor))
            {
                GameTracker.CurrentPlayer = GameTracker.OpponentPlayer;

                GetNextMove();
            }
            else
            {
                if (_IsNetworkGame)
                    _network.disconnect();
                
                //eventAggregator
                _eventAggregator.Publish(new GameOverMessage() { Winner = GameTracker.CurrentPlayer.Color, });                
            }
        }

        private void InitializeBackgroundWorkers()
        {
            _getAIMoveWorker = new BackgroundWorker();
            _getAIMoveWorker.DoWork += new DoWorkEventHandler(_getAIMoveWorker_DoWork);
            _getAIMoveWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getAIMoveWorker_RunWorkerCompleted);
        }

        void _getAIMoveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MakeMove(e.Result as Move);
        }

        void _getAIMoveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _AI.GetMove(GameTracker.Board, GameTracker.CurrentPlayer.Color);
        }

        public void InitializeGameGUI(IntPtr handle, int width, int height)
        {
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
                _AI = new CreeperAI.CreeperAI()
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
            //_network.MoveMade += new EventHandler<MoveResponseMessage>(_network_MoveMade);

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
            switch (GameTracker.CurrentPlayer.PlayerType)
            {
                case PlayerType.AI:
                    _getAIMoveWorker.RunWorkerAsync();
                    break;
                case PlayerType.Human:
                    //Wait for move from XNA
                    break;
                case PlayerType.Network:
                    //Wait for move from Network
                    break;
            }
        }

        public void Handle(MoveResponseMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

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

namespace CreeperCore
{
    public class CreeperGameCore
    {
        public Game1 XNAGame
        {
            get
            {
                return _xnaGame;
            }
            set
            {
                if (_xnaGame != null)
                {
                    _xnaGame.UserMadeMove -= new EventHandler<MoveEventArgs>(_xnaGame_UserMadeMove);
                }

                _xnaGame = value;

                if (_xnaGame != null)
                {
                    _xnaGame.UserMadeMove += new EventHandler<MoveEventArgs>(_xnaGame_UserMadeMove);
                }
            }
        }

        private Game1 _xnaGame;
        private CreeperAI.CreeperAI _AI;
        private Network _network;
        private BackgroundWorker _getAIMoveWorker;
        private BackgroundWorker _networkPlayGame;
        private CreeperBoard _board;
        private bool _IsNetworkGame { get { return GameTracker.Player1.PlayerType == PlayerType.Network || GameTracker.Player2.PlayerType == PlayerType.Network; } }

        public CreeperGameCore()
        {
            InitializeBackgroundWorkers();

            GameTracker.Board = new CreeperBoard();
        }

        private void _xnaGame_UserMadeMove(object sender, MoveEventArgs e)
        {
            if (e.Move.PlayerColor == GameTracker.CurrentPlayer.Color
                && GameTracker.CurrentPlayer.PlayerType == PlayerType.Human)
            {
                MakeMove(e.Move);
            }
        }

        void _network_MoveMade(object sender, MoveEventArgs e)
        {
            e.Move.PlayerColor = GameTracker.CurrentPlayer.Color;
            MakeMove(e.Move);
        }

        private void MakeMove(Move move)
        {
            GameTracker.Board.Move(move);
            XNAGame.OnMoveMade(move);

            if (GameTracker.OpponentPlayer.PlayerType == PlayerType.Network)
            {
                _network.move(move);
            }

            if (!GameTracker.Board.IsFinished(move.PlayerColor))
            {
                GameTracker.CurrentPlayer = GameTracker.OpponentPlayer;

                GetNextMove();
            }
            else if (_IsNetworkGame)
            {
                _network.disconnect();
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
            XNAGame = Game1.GetInstance(handle, width, height);
        }

        public void StartLocalGame(PlayerType player1Type, PlayerType player2Type, AIDifficulty difficulty)
        {
            if (player1Type == PlayerType.Network || player2Type == PlayerType.Network)
            {
                throw new ArgumentException("Cannot start a local game with a player type of network.");
            }
            if (player1Type == PlayerType.AI || player2Type == PlayerType.AI)
            {
                if (difficulty == AIDifficulty.Easy)
                {
                    _AI = new CreeperAI.CreeperAI(15, 84, 2, 43, 100, 2);
                }
                else
                {
                    _AI = new CreeperAI.CreeperAI(15, 84, 2, 43, 100, 5);
                }
            }

            GameTracker.Player1 = new Player(player1Type, CreeperColor.White);
            GameTracker.Player2 = new Player(player2Type, CreeperColor.Black);
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
            _network.MoveMade += new EventHandler<MoveEventArgs>(_network_MoveMade);

            _networkPlayGame = new BackgroundWorker();
            _networkPlayGame.DoWork += new DoWorkEventHandler((s, e) => _network.playGame());

            GameTracker.Player1 = new Player(player1Type, CreeperColor.White);
            GameTracker.Player2 = new Player(player2Type, CreeperColor.Black);
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
    }
}

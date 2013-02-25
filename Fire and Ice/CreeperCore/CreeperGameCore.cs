using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using CreeperNetwork;
using XNAControlGame;
using System.ComponentModel;
using System.IO;

namespace CreeperCore
{
    public class CreeperGameCore
    {
        public CreeperBoard Board
        {
            get
            {
                return _board;
            }

            private set
            {
                _board = value;
                if (XNAGame != null)
                {
                    XNAGame.Board = _board;
                }
            }
        }

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

                if (Board != null)
                {
                    _xnaGame.Board = Board;
                }
            }
        }

        #region members
        private Player Player1 { get; set; }
        private Player Player2 { get; set; }
        private Player CurrentPlayer { get; set; }
        private Game1 _xnaGame;
        private CreeperAI.CreeperAI _AI;
        private Network _network;
        private BackgroundWorker _getAIMoveWorker;
        private CreeperBoard _board;
        #endregion

        public CreeperGameCore()
        {
            InitializeBackgroundWorkers();

            Board = new CreeperBoard();
            _AI = new CreeperAI.CreeperAI(2, 10, .01, 11, 10000);
            _network = new Network();
        }

        private void _xnaGame_UserMadeMove(object sender, MoveEventArgs e)
        {
            if (!Board.IsFinished(e.Move.PlayerColor) && e.Move.PlayerColor == CurrentPlayer.Color)
            {
                MakeMove(e.Move);
            }
        }

        private void MakeMove(Move move)
        {
            Board.Move(move);
            XNAGame.OnMoveMade(move);

            CurrentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1;
            XNAGame.CurrentTurn = CurrentPlayer.Color;

            GetNextMove();
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
            e.Result = _AI.GetMove(Board, CurrentPlayer.Color);
        }

        public void InitializeGameGUI(IntPtr handle, int width, int height)
        {
            XNAGame = new Game1(handle, width, height);
        }

        public void StartLocalGame(PlayerType player1Type, PlayerType player2Type)
        {
            if (player1Type == PlayerType.Network || player2Type == PlayerType.Network)
            {
                throw new ArgumentException("Cannot start a local game with a player type of network.");
            }

            Player1 = new Player(player1Type, CreeperColor.White);
            Player2 = new Player(player2Type, CreeperColor.Black);
            CurrentPlayer = Player1;
            XNAGame.CurrentTurn = CurrentPlayer.Color;
            GetNextMove();
        }

        public void StartNetworkGame(PlayerType player1Type, PlayerType player2Type, Network network)
        {
            if (player1Type == PlayerType.Network && player2Type == PlayerType.Network)
            {
                throw new ArgumentException("Cannot start network game where both players are network players.");
            }

            _network = network;

            Player1 = new Player(player1Type, CreeperColor.White);
            Player2 = new Player(player2Type, CreeperColor.Black);
        }

        private void GetNextMove()
        {
            switch (CurrentPlayer.PlayerType)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using XNAControlGame;
using System.ComponentModel;

namespace CreeperCore
{
    public class EventCreeperCore
    {
        public CreeperBoard Board { get; private set; }
        private Player Player1 { get; set; }
        private Player Player2 { get; set; }
        private Player CurrentPlayer { get; set; }
        protected Game1 _xnaGame;
        protected CreeperAI.CreeperAI _AI;
        private CreeperColor _currentTurn;
        private BackgroundWorker _getAIMoveWorker;
        private BackgroundWorker _getXNAMoveWorker;
        private BackgroundWorker _getNetworkMoveWorker;

        public EventCreeperCore(Game1 xnaGame)
        {
            _xnaGame = xnaGame;
            InitializeBackgroundWorkers();
        }

        private void InitializeBackgroundWorkers()
        {
            _getAIMoveWorker = new BackgroundWorker();
            _getAIMoveWorker.DoWork += new DoWorkEventHandler(_getAIMoveWorker_DoWork);
            _getAIMoveWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getAIMoveWorker_RunWorkerCompleted);

            _getXNAMoveWorker = new BackgroundWorker();
            _getXNAMoveWorker.DoWork += new DoWorkEventHandler(_getXNAMoveWorker_DoWork);
            _getXNAMoveWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getXNAMoveWorker_RunWorkerCompleted);

            _getNetworkMoveWorker = new BackgroundWorker();
            _getNetworkMoveWorker.DoWork += new DoWorkEventHandler(_getNetworkMoveWorker_DoWork);
            _getNetworkMoveWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getNetworkMoveWorker_RunWorkerCompleted);
        }

        private void RunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            Board.Move((Move)e.Result);
            if (!Board.IsFinished(CurrentPlayer.Color))
            {
                CurrentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1;
                GetNextMove();
            }
        }

        void _getNetworkMoveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompleted(e);
        }

        void _getNetworkMoveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _getXNAMoveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompleted(e);
        }

        void _getXNAMoveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _getAIMoveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompleted(e);
        }

        void _getAIMoveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }


        public void StartGame(PlayerType player1Type, PlayerType player2Type)
        {
            _currentTurn = CreeperColor.White;
            Player1 = new Player(player1Type, CreeperColor.White);
            Player2 = new Player(player2Type, CreeperColor.Black);
            CurrentPlayer = Player1;
            GetNextMove();
        }

        private void GetNextMove()
        {
            switch (CurrentPlayer.PlayerType)
            {
                case PlayerType.AI:
                    _getAIMoveWorker.RunWorkerAsync();
                    break;
                case PlayerType.Human:
                    _getXNAMoveWorker.RunWorkerAsync();
                    break;
                case PlayerType.Network:
                    _getNetworkMoveWorker.RunWorkerAsync();
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNAControlGame;
using Creeper;
using System.Threading;
using System.Threading.Tasks;

namespace CreeperCore
{
    public enum PlayerType { Human, AI, Network }

    public class CreeperCore
    {
        //Only the core should be able to change these properties
        //  They should always stay as private set
        public CreeperBoard Board { get; private set; }

        public PlayerType OpponentType { get; private set; }

        protected Game1 _xnaGame;
        protected CreeperAI.CreeperAI _AI;

        public CreeperCore(Game1 xnaGame)
        {
            Board = new CreeperBoard();
            _AI = new CreeperAI.CreeperAI(2, 3, 1, 4, 1000);
            _xnaGame = xnaGame;
            _xnaGame.Board = Board;
        }

        public void StartGame(PlayerType playerType, PlayerType opponentType)
        {
            if (playerType == PlayerType.Network)
            {
                throw new InvalidOperationException("Can't start a game with the local player as the network player");
            }

            switch (playerType)
            {
                case PlayerType.AI:
                    AIGame(opponentType);
                    break;
                case PlayerType.Human:
                    new Thread((() => { HumanGame(opponentType); })).Start();
                    break;
            }
        }

        private void HumanGame(PlayerType opponentType)
        {
            if (opponentType == PlayerType.Human)
            {
                CreeperColor currentTurn = CreeperColor.White;
                while (!Board.IsFinished(currentTurn))
                {
                    Move move = new Move();
                    Thread thread = new Thread(delegate()
                    {
                        move = _xnaGame.GetMove(currentTurn);
                    });
                    thread.Start();
                    thread.Join();
                    Board.Move(move);
                    currentTurn = currentTurn.Opposite();
                }
            }
            else if (opponentType == PlayerType.AI)
            {
                CreeperColor currentTurn = CreeperColor.Black;
                while (!Board.IsFinished(currentTurn))
                {
                    currentTurn = currentTurn.Opposite();
                    Move move = new Move();
                    if (currentTurn == CreeperColor.White)
                    {
                        Thread thread = new Thread(delegate()
                        {
                            move = _xnaGame.GetMove(currentTurn);
                        });
                        thread.Start();
                        thread.Join();
                    }
                    else
                    {
                        Thread thread = new Thread(delegate()
                        {
                            move = _AI.GetMove(Board, currentTurn);
                        });
                        thread.Start();
                        thread.Join();
                    }
                    Board.Move(move);
                }
            }
        }

        private void AIGame(PlayerType opponentType)
        {
            CreeperColor currentTurn = CreeperColor.Black;
            while (!Board.IsFinished(currentTurn))
            {
                currentTurn = currentTurn.Opposite();
                Move move = new Move();
                if (currentTurn == CreeperColor.White)
                {
                    Thread thread = new Thread(delegate()
                    {
                        move = _AI.GetMove(Board, currentTurn);
                    });
                    thread.Start();
                    thread.Join();
                }
                else
                {
                    Thread thread = new Thread(delegate()
                    {
                        move = _AI.GetMove(Board, currentTurn);
                    });
                    thread.Start();
                    thread.Join();
                }
                Board.Move(move);
            }
        }
    }
}

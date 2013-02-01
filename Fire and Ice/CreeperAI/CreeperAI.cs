using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.Threading;

namespace CreeperAI
{
    public class CreeperAI
    {
        //Debug Variables
        private int _recursiveCalls = 0;

        private Random _random = new Random();
        private CreeperBoard _board;
        private CreeperColor _turnColor;

        private const int _White = 0;
        private const int _Black = 1;

        public Move GetMove(CreeperBoard board, CreeperColor AIColor)
        {
            //This must be a copy
            _board = new CreeperBoard(board);
            _turnColor = AIColor;

            //List<Piece> MyTeam = board.WhereTeam(_turnColor);
            //Piece pegToMove = MyTeam.OrderBy((x) => _random.Next()).First();
            //return CreeperUtility.PossibleMoves(pegToMove, board.Pegs).OrderBy((x) => _random.Next()).First();
            Move bestMove = new Move();

            //This business with the threading is a hack to increase the stack size
            //This is not an asynchronous implementaion:
            //Thread t = new Thread(delegate()
            //    {
            //        bestMove = GetMiniMaxMove(board);
            //    }, 100000000);
            //t.Start();

            //Join blocks the main thread until t returns
            //t.Join();

            //This hits stack overflow
            bestMove = GetMiniMaxMove(_board);

            return bestMove;
        }

        //This is where I functionalized the stuff we were doing to prepare to call minimax
        private Move GetMiniMaxMove(CreeperBoard board)
        {
            List<Piece> myTeam = board.WhereTeam(_turnColor);
            List<Move> possibleMoves = new List<Move>();
            foreach (Piece peg in myTeam)
            {
                possibleMoves.AddRange(peg.PossibleMoves(board));
            }
            int max = Int32.MinValue;
            Move bestMove = new Move();
            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                int moveScore = ScoreMiniMaxMove(newBoard, 1);
                if (moveScore > max)
                {
                    max = moveScore;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        //This i renamed to ScoreMiniMaxMove because it made more sense to me this way
        //And this is the actual recursive function
        private int ScoreMiniMaxMove(CreeperBoard board, int depth)
        {
            //Console.WriteLine("Calls: " + _recursiveCalls++);
            if (board.GameOver(_turnColor) || depth >= 0)
            {
                return ScoreBoard(board);
            }

            List<Piece> myTeam = board.WhereTeam(_turnColor);
            List<Move> possibleMoves = new List<Move>();
            foreach (Piece peg in myTeam)
            {
                possibleMoves.AddRange(peg.PossibleMoves(board));
            }
            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                ScoreMiniMaxMove(newBoard, 1);
            }

            int alpha = Int32.MinValue;
            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                alpha = Math.Max(alpha, -ScoreMiniMaxMove(newBoard, depth - 1));
            }
            return alpha;
        }

        private int ScoreBoard(CreeperBoard board)
        {
            return _random.Next() % 10;
        }

        public int TilesToVictory()
        {
            return 0;
        }
    }
}

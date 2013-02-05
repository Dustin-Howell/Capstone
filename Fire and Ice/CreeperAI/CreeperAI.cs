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
        private bool _DEBUG = false;
        private int _recursiveCalls = 0;

        private Random _random = new Random();
        private CreeperBoard _board;
        private CreeperColor _turnColor;

        private const int _White = 0;
        private const int _Black = 1;

        private const double _TerritorialWeight = 1.0;
        private const double _MaterialWeight = 1000000.0;

        private const int _MiniMaxDepth = 2;

        public Move GetMove(CreeperBoard board, CreeperColor AIColor)
        {
            //This must be a copy
            _board = new CreeperBoard(board);
            _turnColor = AIColor;

            Move bestMove = new Move();
            bestMove = GetMiniMaxMove(_board);

            return bestMove;
        }

        //This is where I functionalized the stuff we were doing to prepare to call minimax
        private Move GetMiniMaxMove(CreeperBoard board)
        {
            List<Move> possibleMoves = board.WhereTeam(_turnColor, PieceType.Peg).SelectMany(x => x.PossibleMoves(board)).ToList();

            double max = Double.MinValue;
            Move bestMove = new Move();
            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                double moveScore = -ScoreMiniMaxMove(newBoard, (_turnColor == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White, _MiniMaxDepth);
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
        private double ScoreMiniMaxMove(CreeperBoard board, CreeperColor turnColor, int depth)
        {
            if (_DEBUG)
            {
                Console.WriteLine("Calls: " + _recursiveCalls++);
            }

            if (board.IsFinished(turnColor) || depth <= 0)
            {
                //This is where we call our specific score board implementation
                return ScoreBoard(board, turnColor);
            }

            List<Move> possibleMoves = board.WhereTeam(turnColor, PieceType.Peg).SelectMany(x => x.PossibleMoves(board)).ToList();

            double alpha = Double.MinValue;
            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                alpha = Math.Max(alpha, -ScoreMiniMaxMove(newBoard, (turnColor == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White, depth - 1));
            }

            return alpha;
        }
        
        private double ScoreBoard(CreeperBoard board, CreeperColor turn)
        {
            double score = 0.0;

            score += (ScoreBoardTerritorial(board, turn) * _TerritorialWeight);
            score += (ScoreBoardMaterial(board, turn) * _MaterialWeight);

            return score;
        }

        private double ScoreBoardTerritorial(CreeperBoard board, CreeperColor turn)
        {
            CreeperColor opponentTurn = (turn == CreeperColor.White)? CreeperColor.Black : CreeperColor.White;
            double myTeamCount = board.WhereTeam(turn, PieceType.Tile).Count;
            double opponentTeamCount = board.WhereTeam(opponentTurn, PieceType.Tile).Count;

            //TODO: maybe fix this
            if (opponentTeamCount == 0)
            {
                opponentTeamCount = 1;
            }

            return myTeamCount / opponentTeamCount;
        }

        private double ScoreBoardMaterial(CreeperBoard board, CreeperColor turn)
        {
            CreeperColor opponentTurn = (turn == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White;
            double myTeamCount = board.WhereTeam(turn, PieceType.Peg).Count;
            double opponentTeamCount = board.WhereTeam(opponentTurn, PieceType.Peg).Count;

            //TODO: maybe fix this
            if (opponentTeamCount == 0)
            {
                opponentTeamCount = 1;
            }

            return myTeamCount / opponentTeamCount;
        }

        private double ScoreBoardRandom(CreeperBoard board)
        {
            return (((double)_random.Next()) % 100) / 100;
        }

        public int TilesToVictory()
        {
            return 0;
        }

        //**************************Debug Functions******************************\\

    }
}
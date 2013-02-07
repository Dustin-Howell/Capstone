using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace CreeperAI
{
    public class CreeperAI
    {
        //***************Debug Variables*************\\
        private const bool _DEBUG = true;
        private const bool _writeTimesToFile = true;
        private int _recursiveCalls = 0;
        //***************Debug Variables*************\\

        private Random _random = new Random();
        private CreeperBoard _board;
        private CreeperColor _turnColor;

        private const int _White = 0;
        private const int _Black = 1;

        private const double _TerritorialWeight = 1.0;
        private const double _MaterialWeight = 1000000.0;

        private const int _MiniMaxDepth = 3;

        public CreeperAI()
        {
        }

        public Move GetMove(CreeperBoard board, CreeperColor AIColor)
        {
            //This must be a copy
            _board = new CreeperBoard(board);
            _turnColor = AIColor;

            Stopwatch stopwatch = new Stopwatch();
            if (_DEBUG) stopwatch.Start();

            Move bestMove = new Move();
            bestMove = GetAlphaBetaMiniMaxMove(_board);

            if (_DEBUG)
            {
                stopwatch.Stop();
                System.Console.WriteLine("Seconds elapsed: {0}", ((double)stopwatch.ElapsedMilliseconds) / 1000);
            }

            return bestMove;
        }

        //This is where I functionalized the stuff we were doing to prepare to call minimax
        private Move GetNaiveMiniMaxMove(CreeperBoard board)
        {
            IEnumerable<Move> possibleMoves = board.WhereTeam(_turnColor, PieceType.Peg).SelectMany(x => x.PossibleMoves(board));

            double max = Double.MinValue;
            Move bestMove = new Move();
            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                double moveScore = -ScoreNaiveMiniMaxMove(newBoard, (_turnColor == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White, _MiniMaxDepth);
                if (moveScore > max)
                {
                    max = moveScore;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private Move GetAlphaBetaMiniMaxMove(CreeperBoard board)
        {
            IEnumerable<Move> possibleMoves = board.WhereTeam(_turnColor, PieceType.Peg).SelectMany(x => x.PossibleMoves(board))
                .OrderByDescending(x =>
                {
                    CreeperBoard newBoard = new CreeperBoard(board);
                    newBoard.Move(x);
                    return ScoreBoard(newBoard, _turnColor);
                });

            double max = Double.MinValue;
            Move bestMove = new Move();

            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                double moveScore = ScoreAlphaBetaMiniMaxMove(newBoard, (_turnColor == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White, Double.NegativeInfinity, Double.PositiveInfinity, _MiniMaxDepth);
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
        private double ScoreNaiveMiniMaxMove(CreeperBoard board, CreeperColor turnColor, int depth)
        {
            if (_DEBUG)
            {
                //Console.WriteLine("Calls: " + _recursiveCalls++);
            }

            if (board.IsFinished(turnColor) || depth <= 0)
            {
                //This is where we call our specific score board implementation
                return ScoreBoard(board, turnColor);
            }

            IEnumerable<Move> possibleMoves = board.WhereTeam(turnColor, PieceType.Peg).SelectMany(x => x.PossibleMoves(board));

            double alpha = Double.MinValue;
            foreach (Move move in possibleMoves)
            {
                CreeperBoard newBoard = new CreeperBoard(board);
                newBoard.Move(move);
                alpha = Math.Max(alpha, -ScoreNaiveMiniMaxMove(newBoard, (turnColor == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White, depth - 1));
            }

            return alpha;
        }

        private double ScoreAlphaBetaMiniMaxMove(CreeperBoard board, CreeperColor turnColor, double alpha, double beta, int depth)
        {
            // if  depth = 0 or node is a terminal node
            if ((depth <= 0) || board.IsFinished(turnColor))
            {
                // return the heuristic value of node
                return ScoreBoard(board, turnColor);
            }

            // children of current node
            IEnumerable<Move> possibleMoves = board.WhereTeam(turnColor, PieceType.Peg).SelectMany(x => x.PossibleMoves(board));

            // if  Player = MaximizedPlayer
            if (turnColor == _turnColor)
            {
                // prioitize favorable boards
                IEnumerable<CreeperBoard> boards = possibleMoves.Select(x =>
                {
                    CreeperBoard newBoard = new CreeperBoard(board);
                    newBoard.Move(x);
                    return newBoard;
                }).OrderByDescending(x => ScoreBoard(x, turnColor));

                // for each child of node
                foreach (CreeperBoard currentBoard in boards)
                {
                    // α := max(α, alphabeta(child, depth-1, α, β, not(Player) ))
                    alpha = Math.Max(alpha, ScoreAlphaBetaMiniMaxMove(currentBoard, (turnColor == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White, alpha, beta, depth - 1));

                    // if β ≤ α
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                // return α
                return alpha;
            }
            else
            {
                // prioitize favorable boards
                IEnumerable<CreeperBoard> boards = possibleMoves.Select(x =>
                {
                    CreeperBoard newBoard = new CreeperBoard(board);
                    newBoard.Move(x);
                    return newBoard;
                }).OrderBy(x => ScoreBoard(x, turnColor));

                // for each child of node
                foreach (CreeperBoard currentBoard in boards)
                {
                    // β := min(β, alphabeta(child, depth-1, α, β, not(Player) ))
                    beta = Math.Min(beta, ScoreAlphaBetaMiniMaxMove(currentBoard, (turnColor == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White, alpha, beta, depth - 1));

                    // if β ≤ α
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                // return β
                return beta;
            }
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
            double myTeamCount = board.WhereTeam(turn, PieceType.Tile).Count();
            double opponentTeamCount = board.WhereTeam(opponentTurn, PieceType.Tile).Count();

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
            double myTeamCount = board.WhereTeam(turn, PieceType.Peg).Count();
            double opponentTeamCount = board.WhereTeam(opponentTurn, PieceType.Peg).Count();

            //TODO: maybe fix this
            if (opponentTeamCount == 0)
            {
                opponentTeamCount = 1;
            }

            return myTeamCount / opponentTeamCount;
        }

        private double ScoreBoardVictoryProximity(CreeperBoard board, CreeperColor turn)
        {
            return 0.0;
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
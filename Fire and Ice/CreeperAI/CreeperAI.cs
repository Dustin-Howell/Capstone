using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.Diagnostics;
using System.IO;

namespace CreeperAI
{
    public class CreeperAI
    {
        //debug variables\\
        private bool _reportTime = true;

        private AICreeperBoard _board;
        private CreeperColor _turnColor;
        private int _MiniMaxDepth = 4;

        private const double _TerritorialWeight = 1.0;
        private const double _MaterialWeight = 100000000.0;

        public Move GetMove(CreeperBoard board, CreeperColor turnColor)
        {
            _board = new AICreeperBoard(board);
            _turnColor = turnColor;

            Stopwatch stopwatch = new Stopwatch();
            if (_reportTime) stopwatch.Start();

            Move bestMove = new Move();
            bestMove = GetAlphaBetaMiniMaxMove(_board);

            if (_reportTime)
            {
                stopwatch.Stop();
                System.Console.WriteLine("Seconds elapsed: {0}", ((double)stopwatch.ElapsedMilliseconds) / 1000);
                using (StreamWriter file = new StreamWriter("Times.log", true))
                {
                    file.WriteLine("Seconds elapsed: {0}", ((double)stopwatch.ElapsedMilliseconds) / 1000);
                }
            }

            return bestMove;
        }

        private Move GetAlphaBetaMiniMaxMove(AICreeperBoard board)
        {
            IEnumerable<Move> possibleMoves = board.AllPossibleMoves(_turnColor)
                .OrderByDescending(x =>
                {
                    board.PushMove(x);
                    double score = ScoreBoard(board, _turnColor);
                    board.PopMove();
                    return score;
                }).ToList();

            double max = Double.MinValue;
            Move bestMove = new Move();

            foreach (Move move in possibleMoves)
            {
                board.PushMove(move);
                double moveScore = ScoreAlphaBetaMiniMaxMove(board, _turnColor.Opposite(), Double.NegativeInfinity, Double.PositiveInfinity, _MiniMaxDepth - 1);
                board.PopMove();
                if (moveScore > max)
                {
                    max = moveScore;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private double ScoreAlphaBetaMiniMaxMove(AICreeperBoard board, CreeperColor turnColor, double alpha, double beta, int depth)
        {
            // if  depth = 0 or node is a terminal node
            if ((depth <= 0) || board.IsFinished(turnColor))
            {
                // return the heuristic value of node
                return ScoreBoard(board, turnColor.Opposite());
            }

            // if  Player = MaximizedPlayer
            if (turnColor == _turnColor)
            {
                // prioitize favorable boards
                IEnumerable<Move> moves = board.AllPossibleMoves(turnColor)
                 .OrderByDescending(x =>
                {
                    board.PushMove(x);
                    double score = ScoreBoard(board, turnColor);
                    board.PopMove();
                    return score;
                }).ToList();

                // for each child of node
                foreach (Move currentMove in moves)
                {
                    // α := max(α, alphabeta(child, depth-1, α, β, not(Player) ))
                    board.PushMove(currentMove);
                    alpha = Math.Max(alpha, ScoreAlphaBetaMiniMaxMove(board, turnColor.Opposite(), alpha, beta, depth - 1));
                    board.PopMove();

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
                IEnumerable<Move> moves = board.AllPossibleMoves(turnColor)
                .OrderByDescending(x =>
                {
                    board.PushMove(x);
                    double score = ScoreBoard(board, turnColor);
                    board.PopMove();
                    return score;
                }).ToList();

                // for each child of node
                foreach (Move currentMove in moves)
                {
                    // β := min(β, alphabeta(child, depth-1, α, β, not(Player) ))
                    board.PushMove(currentMove);
                    beta = Math.Min(beta, ScoreAlphaBetaMiniMaxMove(board, turnColor.Opposite(), alpha, beta, depth - 1));
                    board.PopMove();

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

        private double ScoreBoard(AICreeperBoard board, CreeperColor turnColor)
        {
            double score = 0.0;

            score += (ScoreBoardTerritorial(board, turnColor) * _TerritorialWeight);
            score += (ScoreBoardMaterial(board, turnColor) * _MaterialWeight);

            return score;
        }

        private double ScoreBoardTerritorial(AICreeperBoard board, CreeperColor turn)
        {
            double myTeamCount = board.TeamCount(turn, PieceType.Tile);
            double opponentTeamCount = board.TeamCount(turn.Opposite(), PieceType.Tile);

            //TODO: maybe fix this
            if (opponentTeamCount == 0)
            {
                opponentTeamCount = 1;
            }

            return myTeamCount / opponentTeamCount;
        }

        private double ScoreBoardMaterial(AICreeperBoard board, CreeperColor turn)
        {
            double myTeamCount = board.TeamCount(turn, PieceType.Peg);
            double opponentTeamCount = board.TeamCount(turn.Opposite(), PieceType.Peg);

            //TODO: maybe fix this
            if (opponentTeamCount == 0)
            {
                opponentTeamCount = 1;
            }

            return myTeamCount / opponentTeamCount;
        }
    }
}

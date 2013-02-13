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
        private bool _sort = true;

        private AICreeperBoard _board;
        private CreeperColor _turnColor;
        private int _MiniMaxDepth = 6;

        private const double _TerritorialWeight = 1.0;
        private const double _MaterialWeight = 2.0;
        private const double _pathToVictoryWeight = 1.0;

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
            if ((depth <= 0) 
                || (board.CouldBeFinished(turnColor) &&
                board.IsFinished(turnColor)))
            {
                // return the heuristic value of node
                return ScoreBoard(board, turnColor);
            }

            // if  Player = MaximizedPlayer
            if (turnColor == _turnColor)
            {
                // prioitize favorable boards
                Move[] moves = board.AllPossibleMoves(turnColor);              

                // for each child of node
                for (int i = 0; i < moves.Count(); i++)
                {
                    Move currentMove = moves[i];
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
                Move[] moves = board.AllPossibleMoves(turnColor);

                // for each child of node
                for (int i = 0; i < moves.Count(); i++)
                {
                    Move currentMove = moves[i];

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
            score += ScoreBoardVictory(board, turnColor);

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
        
        private double ScoreBoardPositional(AICreeperBoard board, CreeperColor turn)
        {
            return 0.0;
        }

        private double ScoreBoardVictory(AICreeperBoard board, CreeperColor turn)
        {
            double score = 0.0;

            //see if we are filling in a row with a null head
            //bonus points for doing that with an adjacency

            return score;
        }
        
        private double ScoreBoardVictory(AICreeperBoard board, CreeperColor turn, bool randomFlag)
        {
            double score = 0.0;

            Move currentMove = board.CurrentMove;
            if (board.IsFlipMove(currentMove))
            {
                AIBoardNode flippedTile = board.GetFlippedTileCopy(currentMove);
                if (turn == CreeperColor.White)
                {
                    int northRow = flippedTile.Row - 1;
                    int eastColumn = flippedTile.Column + 1;

                    if (northRow >= 0
                        && board.TileBoard[northRow, flippedTile.Column].Color == turn)
                    {
                        score += _pathToVictoryWeight * (CreeperBoard.TileRows - Math.Abs(AICreeperBoard._WhiteEnd.Row - flippedTile.Row));
                    }

                    if (eastColumn < CreeperBoard.TileRows
                        && board.TileBoard[flippedTile.Row, eastColumn].Color == turn)
                    {
                        score += _pathToVictoryWeight * (CreeperBoard.TileRows - Math.Abs(AICreeperBoard._WhiteEnd.Column - flippedTile.Column));
                    }
                }

                if (turn == CreeperColor.Black)
                {
                    int northRow = flippedTile.Row - 1;
                    int west = flippedTile.Column - 1;

                    if (northRow >= 0
                        && board.TileBoard[northRow, flippedTile.Column].Color == turn)
                    {
                        //Weights it more if the move is closer to the the end goal
                        score += _pathToVictoryWeight * (CreeperBoard.TileRows - Math.Abs(AICreeperBoard._BlackEnd.Row - flippedTile.Row));
                    }

                    if (west >= 0
                        && board.TileBoard[flippedTile.Row, west].Color == turn)
                    {
                        score += _pathToVictoryWeight * (CreeperBoard.TileRows - Math.Abs(AICreeperBoard._BlackEnd.Column - flippedTile.Column));
                    }
                }
            }

            return score;
        }
    }
}

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

        private const double _TerritorialWeight = 2.0;
        private const double _MaterialWeight = 5.0;
        private const double _pathToVictoryWeight = 6.0;

        public Move GetMove(CreeperBoard board, CreeperColor turnColor)
        {
            _board = new AICreeperBoard(board);
            _turnColor = turnColor;

            Stopwatch stopwatch = new Stopwatch();
            if (_reportTime) stopwatch.Start();

            Move bestMove = new Move();
            bestMove = GetAlphaBetaNegaMaxMove(_board);

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

        Move GetAlphaBetaNegaMaxMove(AICreeperBoard board)
        {
            Move bestMove = new Move();

            double best = Double.NegativeInfinity;
            double alpha = Double.NegativeInfinity;
            double beta = Double.PositiveInfinity;

            // Enumerate the children of the current node
            Move[] moves = board.AllPossibleMoves(_turnColor);
            for (int i = 0; i < moves.Length; i++)
            {
                // Evaluate child node
                board.PushMove(moves[i]);
                double score = -ScoreAlphaBetaNegaMaxMove(board, _turnColor.Opposite(), -beta, -Math.Max(alpha, best), _MiniMaxDepth - 1);
                board.PopMove();

                if (score > best)
                {
                    best = score;
                    bestMove = moves[i];
                }
            }

            return bestMove;
        }

        private double ScoreAlphaBetaNegaMaxMove(AICreeperBoard board, CreeperColor turnColor, double alpha, double beta, int depth)
        {
            // if  depth = 0 or node is a terminal node
            if ((depth <= 0) || board.IsFinished)
            {
                // return the heuristic value of node
                return ScoreBoard(board, turnColor);
            }

            // Initialize the best score
            double best = Double.NegativeInfinity;

            // Enumerate the children of the current node
            Move[] moves = board.AllPossibleMoves(turnColor);
            for (int i = 0; i < moves.Length; i++)
            {
                // Evaluate child node:
                board.PushMove(moves[i]);
                best = Math.Max(best, -ScoreAlphaBetaNegaMaxMove(board, turnColor.Opposite(), -beta, -Math.Max(alpha, best), depth - 1));
                board.PopMove();

                // Prune if the current best score crosses beta
                if (best >= beta)
                    return best;
            }

            return best;
        }

        private double ScoreAlphaBetaMiniMaxMove(AICreeperBoard board, CreeperColor turnColor, double alpha, double beta, int depth)
        {
            // if  depth = 0 or node is a terminal node
            if ((depth <= 0) || board.IsFinished)
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
                for (int i = 0; i < moves.Length; i++)
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
                for (int i = 0; i < moves.Length; i++)
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

            switch (board.GameState)
            {
                case CreeperGameState.Complete:
                    score = Double.PositiveInfinity;
                    break;

                default:
                    score += (ScoreBoardTerritorial(board, turnColor) * _TerritorialWeight);
                    score += (ScoreBoardMaterial(board, turnColor) * _MaterialWeight);
                    score += ScoreBoardVictory(board, turnColor);
                    break;
            }



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

            Move currentMove = board.CurrentMove;
            if (board.IsFlipMove(currentMove))
            {
                AIBoardNode flippedTile = board.GetFlippedTileCopy(currentMove);
                AIBoardNode[] columnHead = (turn == CreeperColor.White) ? board.ColumnHeadWhite : board.ColumnHeadWhite;
                AIBoardNode[] rowHead = (turn == CreeperColor.White) ? board.RowHeadWhite : board.RowHeadWhite;

                //If we are null in this row, we want to add a new tile here
                if (rowHead[flippedTile.Row] == null)
                {
                    //so we add to the score
                    score += _pathToVictoryWeight;

                    //if the tile above us is our color
                    if (flippedTile.Row - 1 >= 0
                        && board.TileBoard[flippedTile.Row - 1, flippedTile.Column].Color == turn)
                    {
                        //bonus points for an adjacency when moving to a null row
                        score += _pathToVictoryWeight;
                    }
                    //and if the row below us is our color
                    if (flippedTile.Row + 1 <= CreeperBoard.TileRows - 1
                        && board.TileBoard[flippedTile.Row + 1, flippedTile.Column].Color == turn)
                    {
                        //more bonus points
                        score += _pathToVictoryWeight;
                    }
                }

                //if this column is null
                if (columnHead[flippedTile.Column] == null)
                {
                    //that's good--we want to be filling in columns with no pieces
                    score += _pathToVictoryWeight;

                    //and if the column to our left has someone there
                    if (flippedTile.Column - 1 >= 0
                        && board.TileBoard[flippedTile.Row, flippedTile.Column - 1].Color == turn)
                    {
                        //great, another connection
                        score += _pathToVictoryWeight;
                    }
                    //and if the column to our right does as well
                    if (flippedTile.Column + 1 <= CreeperBoard.TileRows - 1
                        && board.TileBoard[flippedTile.Row, flippedTile.Column + 1].Color == turn)
                    {
                        //even better
                        score += _pathToVictoryWeight;
                    }
                }
            }

            return score;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace CreeperAI
{
    public class CreeperAI
    {
        //debug variables\\
        private bool _reportTime = true;
        private bool _sort = true;
        private bool _parallel = true;

        private AICreeperBoard _board;
        private CreeperColor _turnColor;
        private int _MiniMaxDepth = 5;

        private double _territorialWeight;
        private double _materialWeight;
        private double _pathToVictoryWeight;
        private double _victoryWeight;

        private static Random _Random = new Random();

        public CreeperAI(double territoryWeight, double materialWeight, double victoryPathWeight, double victoryWeight)
        {
            _territorialWeight = territoryWeight;
            _materialWeight = materialWeight;
            _pathToVictoryWeight = victoryPathWeight;
            _victoryWeight = victoryWeight;
        }

        public Move GetMove(CreeperBoard board, CreeperColor turnColor)
        {
            _board = new AICreeperBoard(board);
            _turnColor = turnColor;

            Stopwatch stopwatch = new Stopwatch();
            if (_reportTime) stopwatch.Start();

            Move bestMove = new Move();
            if (_parallel)
            {
                bestMove = GetAlphaBetaNegaMaxMove(board);
            }
            else
            {
                bestMove = GetAlphaBetaNegaMaxMoveSequential(_board);
            }

            if (_reportTime)
            {
                stopwatch.Stop();
                System.Console.WriteLine("Seconds elapsed: {0}", ((double)stopwatch.ElapsedMilliseconds) / 1000);
                using (StreamWriter file = new StreamWriter("Times.csv", true))
                {
                    file.WriteLine("{0}", ((double)stopwatch.ElapsedMilliseconds) / 1000);
                }
            }

            return bestMove;
        }

        Move GetAlphaBetaNegaMaxMoveSequential(AICreeperBoard board)
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

        Move GetAlphaBetaNegaMaxMove(CreeperBoard board)
        {
            Move bestMove = new Move();

            double best = Double.NegativeInfinity;
            double alpha = Double.NegativeInfinity;
            double beta = Double.PositiveInfinity;

            // Enumerate the children of the current node
            Move[] moves = new AICreeperBoard(board).AllPossibleMoves(_turnColor);

            Dictionary<Move, double> moveScores = new Dictionary<Move, double>();

            Parallel.ForEach(moves, move =>
                {
                    // If you have weird bugs, maybe use IDisposable?
                    AICreeperBoard currentBoard = new AICreeperBoard(board);
                    currentBoard.PushMove(move);
                    double score = -ScoreAlphaBetaNegaMaxMove(currentBoard, _turnColor.Opposite(), -beta, -Math.Max(alpha, best), _MiniMaxDepth - 1);
                    lock (this)
                    {
                        moveScores.Add(move, score);
                    }
                });

            bestMove = moveScores.First().Key;
            foreach (KeyValuePair<Move, double> move in moveScores)
            {
                if (moveScores[move.Key] > moveScores[bestMove])
                {
                    bestMove = move.Key;
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
                    score = 1000000.0;
                    break;

                default:
                    score += (ScoreBoardTerritorial(board, turnColor) * _territorialWeight);
                    score += (ScoreBoardMaterial(board, turnColor) * _materialWeight);
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

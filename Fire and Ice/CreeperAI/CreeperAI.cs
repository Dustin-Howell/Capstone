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
        private bool _reportTime = false;
        private bool _sort = true;
        private bool _parallel = false;

        private AICreeperBoard _board;
        private CreeperColor _turnColor;
        //TODO: Change this after the UIP
        public int _MiniMaxDepth;

        private double _territorialWeight;
        private double _materialWeight;
        private double _centralWeight;
        private double _centralRelativeWeight;
        private double _linearWeight;
        private double _pathToVictoryWeight;
        private double _powerWeight;
        private double _victoryWeight;

        private static Random _Random = new Random();


        public CreeperAI(double territoryWeight, double materialWeight, double centralWeight, double centralRelativeWeight, double linearWeight, double victoryPathWeight, double powerWeight, double victoryWeight, int depth = 3)
        {
            _territorialWeight = territoryWeight;
            _materialWeight = materialWeight;
            _centralWeight = centralWeight;
            _centralRelativeWeight = centralRelativeWeight;
            _linearWeight = linearWeight;
            _pathToVictoryWeight = victoryPathWeight;
            _powerWeight = powerWeight;
            _victoryWeight = victoryWeight;
            _MiniMaxDepth = depth;
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
                bestMove = GetParallelAlphaBetaNegaMaxMove(board);
            }
            else
            {
                bestMove = GetAlphaBetaNegaMaxMove(_board);
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
            Console.WriteLine("Best score found: {0}", alpha);
            return bestMove;
        }

        Move GetParallelAlphaBetaNegaMaxMove(CreeperBoard board)
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
                return ScoreBoard(board, turnColor, depth);
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
                return ScoreBoard(board, turnColor, depth);
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






        // Scoring stuff:

        private double ScoreBoard(AICreeperBoard board, CreeperColor turnColor, int depth)
        {
            double score = 0.0;

            switch (board.GameState)
            {
                case CreeperGameState.Complete:
                    score = _victoryWeight * depth * ((_MiniMaxDepth % 2 == 0)? 1 : -1);
                    break;

                default:
                    //score += (ScoreBoardTerritorial(board, turnColor) * _territorialWeight);
                    //score += (ScoreBoardMaterial(board, turnColor) * _materialWeight);
                    //score += (ScoreBoardCentralPegs(board, turnColor) * _centralWeight);
                    //score += (ScoreBoardCentralPegsRelative(board, turnColor) * _centralRelativeWeight);
                    //score += (ScoreBoardLinearPegs(board, turnColor) * _linearWeight);
                    double shortestDistance = ScoreBoardShortestDistance(board, turnColor);
                    //score +=  shortestDistance * _pathToVictoryWeight;
                    score += Math.Pow(shortestDistance, _powerWeight) * _pathToVictoryWeight;
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

            return myTeamCount / opponentTeamCount - 1;
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

            return myTeamCount / opponentTeamCount - 1;
        }
        
        // This scores based on how close the pegs are to the center of the board.
        private double ScoreBoardCentralPegs(AICreeperBoard board, CreeperColor turn)
        {
            double score = 0d;
            double pegCount = turn.IsBlack() ? board.BlackPegs.Count : board.WhitePegs.Count;

            if (turn == CreeperColor.Fire)
            {
                foreach (AIBoardNode peg in board.WhitePegs)
                {
                    score += Math.Sqrt(Math.Pow(peg.Row - 3, 2) + (Math.Pow(peg.Column - 3, 2)));
                }
            }
            else
            {
                foreach (AIBoardNode peg in board.BlackPegs)
                {
                    score += Math.Sqrt(Math.Pow(peg.Row - 3, 2) + (Math.Pow(peg.Column - 3, 2)));
                }
            }

            return (8.485 - (score / pegCount)) / 8.485;
        }

        // This scores our pegs based on how close our pegs are to the line between our goals.
        private double ScoreBoardLinearPegs(AICreeperBoard board, CreeperColor turn)
        {
            double score = 0.0;
            double pegCount = turn.IsBlack() ? board.BlackPegs.Count : board.WhitePegs.Count;

            foreach (AIBoardNode peg in turn.IsBlack() ? board.BlackPegs : board.WhitePegs)
            {
                score += (turn.IsBlack() ? Math.Abs( peg.Row + peg.Column - 6 ) : Math.Abs( peg.Row - peg.Column));
            }

            return (6 - (score / pegCount)) / 6;
        }

        private double ScoreBoardCentralPegsRelative(AICreeperBoard board, CreeperColor turn)
        {
            return ScoreBoardCentralPegs(board, turn) / ScoreBoardCentralPegs(board, turn.Opposite()) - 1;
        }

        private double ScoreBoardShortestDistance(AICreeperBoard board, CreeperColor turn)
        {
            Position start = turn.IsBlack() ? AICreeperBoard._BlackStart : AICreeperBoard._WhiteStart;
            Position end = turn.IsBlack() ? AICreeperBoard._BlackEnd : AICreeperBoard._WhiteEnd;
            HashSet<AIBoardNode> startTiles = new HashSet<AIBoardNode>();
            startTiles.Add(board.TileBoard[start.Row, start.Column]);
            HashSet<AIBoardNode> endTiles = new HashSet<AIBoardNode>();
            endTiles.Add(board.TileBoard[end.Row, end.Column]);
            Stack<AIBoardNode> stack = new Stack<AIBoardNode>();

            stack.Push(board.TileBoard[start.Row, start.Column]);
            do
            {
                AIBoardNode currentTile = stack.Pop();

                IEnumerable<AIBoardNode>  neighbors = board.GetNeighbors(currentTile, turn);
                foreach (AIBoardNode neighbor in neighbors)
                {
                    if (!startTiles.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }

                startTiles.UnionWith(neighbors);
            }
            while (stack.Any());

            double shortestDistance = DistanceBetweenTiles(board.TileBoard[start.Row, start.Column], board.TileBoard[end.Row, end.Column]);

            stack.Push(board.TileBoard[end.Row, end.Column]);
            do
            {
                AIBoardNode currentTile = stack.Pop();

                //Here is where we calculate the distances...
                foreach (AIBoardNode tile in startTiles)
                {
                    shortestDistance = Math.Min(shortestDistance, DistanceBetweenTiles(currentTile, tile));
                }

                IEnumerable<AIBoardNode> neighbors = board.GetNeighbors(currentTile, turn);
                foreach (AIBoardNode neighbor in neighbors)
                {
                    if (!endTiles.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }

                endTiles.UnionWith(neighbors);
            }
            while (stack.Any());

            return 1 - shortestDistance / 10;
        }

        private double DistanceBetweenTiles(AIBoardNode tile1, AIBoardNode tile2)
        {
            return Math.Abs(tile1.Row - tile2.Row) + Math.Abs(tile1.Column - tile2.Column);
        }

        private double ScoreBoardVictory(AICreeperBoard board, CreeperColor turn)
        {
            double score = 0.0;

            Move currentMove = board.CurrentMove;
            if (board.IsFlipMove(currentMove))
            {
                AIBoardNode flippedTile = board.GetFlippedTileCopy(currentMove);
                AIBoardNode[] columnHead = (turn == CreeperColor.Fire) ? board.ColumnHeadWhite : board.ColumnHeadWhite;
                AIBoardNode[] rowHead = (turn == CreeperColor.Fire) ? board.RowHeadWhite : board.RowHeadWhite;

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

        double ScoreBoardPathSearch(AICreeperBoard board, CreeperColor turn)
        {
            double score = 0.0;

            HashSet<Position> startTiles = new HashSet<Position>();
            HashSet<Position> endTiles = new HashSet<Position>();

            Position start = (turn == CreeperColor.Fire) ? AICreeperBoard._WhiteStart : AICreeperBoard._BlackStart;
            Position end = (turn == CreeperColor.Fire) ? AICreeperBoard._WhiteEnd : AICreeperBoard._BlackEnd;

            return score;
        }
    }
}

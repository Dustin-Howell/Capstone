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
using Caliburn.Micro;
using CreeperMessages;
using System.ComponentModel;
using System.Reflection;

namespace CreeperAI
{
    public class AI : IHandle<MoveMessage>
    {
        //debug variables\\
        private bool _reportTime = false;
        private bool _sort = true;
        private bool _parallel = false;

        //Math Constants
        private const double _MaxPegDistanceFromCenter = 3.60555127546; // Square Root of 13
        private const double _MaxPathLength = 10;
        private const double _TileCenterRow = (double)CreeperBoard.TileRows / 2d;
        private const double _TileCenterColumn = (double)CreeperBoard.TileRows / 2d;

        private AICreeperBoard _board;
        private CreeperColor _turnColor;
        public int _MiniMaxDepth = 5;
        //private Dictionary<AIHash, double> _scoredBoards;

        public double TerritorialWeight { get; set; }
        public double MaterialWeight { get; set; }
        public double PositionalWeight { get; set; }
        public double ShortestDistanceWeight { get; set; }
        public double PathPowerWeight { get; set; }
        public double VictoryWeight { get; set; }
        public AIDifficulty Difficulty { get; set; }

        private static Random _Random = new Random();

        private IEventAggregator _eventAggregator;
        private BackgroundWorker _getMoveWorker;

        public AI(IEventAggregator eventAggregator)
        {
            _MiniMaxDepth = (Difficulty == AIDifficulty.Hard) ? 5 : 3;
            _eventAggregator = eventAggregator;
            _getMoveWorker = new BackgroundWorker();
            _getMoveWorker.DoWork += new DoWorkEventHandler(_getMoveWorker_DoWork);
            _getMoveWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getMoveWorker_RunWorkerCompleted);
        }

        public AI(Dictionary<String, double> weights)
        {
            _MiniMaxDepth = 3;
            foreach (String key in weights.Keys)
            {
                PropertyInfo property = this.GetType().GetProperty(key);
                if (property != null)
                {
                    property.SetValue(this, weights[key], null);
                }
            }

            VictoryWeight = 10000;
        }

        public Move GetMove(CreeperBoard board, CreeperColor turnColor)
        {
            _board = new AICreeperBoard(board);
            _turnColor = turnColor;

            Stopwatch stopwatch = new Stopwatch();
            if (_reportTime) stopwatch.Start();

            Move bestMove = GetAlphaBetaNegaMaxMove(_board);

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

        #region AlphaBeta
        Move GetAlphaBetaNegaMaxMove(AICreeperBoard board)
        {
            Move bestMove = null;

            double alpha = Double.NegativeInfinity;
            double beta = Double.PositiveInfinity;

            // Enumerate the children of the current node
            Move[] moves = board.AllPossibleMoves(_turnColor);
            for (int i = 0; i < moves.Length; i++)
            {
                // Evaluate child node
                board.PushMove(moves[i]);
                double score =
                    //board.IsCaptureMove(moves[i]) ? -ScoreNegaMaterialExchange(board, _turnColor.Opposite()) : 
                    -ScoreAlphaBetaNegaMaxMove(board, _turnColor.Opposite(), -beta, -alpha, _MiniMaxDepth - 1);
                board.PopMove();

                if (score > alpha)
                {
                    alpha = score;
                    bestMove = moves[i];
                }
            }

            if (bestMove == null) throw new InvalidOperationException("Don't ask AI for a move on a moveless board.");

            return bestMove;
        }

        private double ScoreNegaMaterialExchange(AICreeperBoard board, CreeperColor turnColor)
        {
            double score = double.NegativeInfinity;
            Move[] captures = board.AllPossibleCaptures(turnColor);

            if (captures.Length > 0)
            {
                for (int i = 0; i < captures.Length; i++)
                {
                    board.PushMove(captures[i]);
                    score = -ScoreNegaMaterialExchange(board, turnColor.Opposite());
                    board.PopMove();
                }
            }
            else
            {
                score = ScoreBoard(board, _turnColor, -1) * ((turnColor == _turnColor) ? 1 : -1);
            }

            return score;
        }

        private double ScoreAlphaBetaNegaMaxMove(AICreeperBoard board, CreeperColor turnColor, double alpha, double beta, int depth)
        {
            // if  depth = 0 or node is a terminal node
            if ((depth <= 0) || board.IsFinished)
            {
                // return the heuristic value of node
                return ScoreBoard(board, _turnColor, depth)
                    * ((turnColor == _turnColor) ? 1 : -1);
            }

            // Enumerate the children of the current node
            Move[] moves = board.AllPossibleMoves(turnColor);
            for (int i = 0; i < moves.Length; i++)
            {
                // Evaluate child node:
                board.PushMove(moves[i]);
                //alpha = Math.Max(alpha, -ScoreAlphaBetaNegaMaxMove(board, turnColor.Opposite(), -beta, -alpha, depth - 1));
                double score = -ScoreAlphaBetaNegaMaxMove(board, turnColor.Opposite(), -beta, -alpha, depth - 1);
                board.PopMove();

                // Prune if the current best score crosses beta
                if (score >= beta)
                    return score;
                if (score >= alpha)
                    alpha = score;
            }

            return alpha;
        }
        #endregion

        #region BoardScoring
        private double ScoreBoard(AICreeperBoard board, CreeperColor turnColor, int depth)
        {
            double score = 0.0;

            //if (ScoredBoards.TryGetValue(board.Hash, out score));

            //else
            //{
            switch (board.GameState)
            {
                case CreeperGameState.Complete:
                    score = VictoryWeight * (depth + 1);
                    break;

                default:
                    score += (ScoreBoardTerritorial(board, turnColor) * TerritorialWeight);
                    score += (ScoreBoardMaterial(board, turnColor) * MaterialWeight);
                    score += (ScoreBoardPositional(board, turnColor) * PositionalWeight);
                    double path = ScoreBoardShortestDistance(board, turnColor);
                    score += Math.Pow(path, PathPowerWeight) * ShortestDistanceWeight;
                    break;
            }

            //ScoreBoards[board] = score;
            //}

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
        
        private double ScoreBoardPositional(AICreeperBoard board, CreeperColor turn)
        {
            double score = 0d;
            if (turn == CreeperColor.Fire)
            {
                foreach (AIBoardNode peg in board.WhitePegs)
                {
                    score += Math.Sqrt(Math.Pow(peg.Row - _TileCenterRow, 2) + (Math.Pow(peg.Column - _TileCenterColumn, 2)));
                }
                if (board.WhitePegs.Count > 0)
                {
                    score /= board.WhitePegs.Count;
                }
                else
                {
                    score = _MaxPegDistanceFromCenter;
                }
            }
            else
            {
                foreach (AIBoardNode peg in board.BlackPegs)
                {
                    score += EuclideanDistance(peg.Row, peg.Column, _TileCenterRow, _TileCenterColumn);
                }
                if (board.BlackPegs.Count > 0)
                {
                    score /= board.BlackPegs.Count;
                }
                else
                {
                    score = _MaxPegDistanceFromCenter;
                }
            }

            return 1 - score / _MaxPegDistanceFromCenter;
        }

        private double ScoreBoardShortestDistance(AICreeperBoard board, CreeperColor turn)
        {
            Position start = turn.IsIce() ? AICreeperBoard._BlackStart : AICreeperBoard._WhiteStart;
            Position end = turn.IsIce() ? AICreeperBoard._BlackEnd : AICreeperBoard._WhiteEnd;
            HashSet<AIBoardNode> startTiles = new HashSet<AIBoardNode>();
            startTiles.Add(board.TileBoard[start.Row, start.Column]);
            HashSet<AIBoardNode> endTiles = new HashSet<AIBoardNode>();
            endTiles.Add(board.TileBoard[end.Row, end.Column]);
            Stack<AIBoardNode> stack = new Stack<AIBoardNode>();

            stack.Push(board.TileBoard[start.Row, start.Column]);
            do
            {
                AIBoardNode currentTile = stack.Pop();

                IEnumerable<AIBoardNode> neighbors = board.GetNeighbors(currentTile, turn);
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

            double shortestDistance = TaxiCabDistance(start.Row, start.Column, end.Row, end.Column);

            stack.Push(board.TileBoard[end.Row, end.Column]);
            do
            {
                AIBoardNode currentTile = stack.Pop();

                //Here is where we calculate the distances...
                foreach (AIBoardNode tile in startTiles)
                {
                    shortestDistance = Math.Min(shortestDistance, TaxiCabDistance(currentTile.Row, currentTile.Column, tile.Row, tile.Column));
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

            return 1 - shortestDistance / _MaxPathLength;
        }

        // Scoring Utility Functions
        private double TaxiCabDistance(double row1,double column1, double row2, double column2)
        {
            return Math.Abs(row1 - row2) + Math.Abs(column1 - column2);
        }
        private double EuclideanDistance(double row1,double column1, double row2, double column2)
        {
            return Math.Sqrt(Math.Pow(row1 - row2, 2) + Math.Pow(column1 - column2, 2));
        }
        #endregion

        #region Event Stuff
        public void Handle(MoveMessage message)
        {
            if (message.Type == MoveMessageType.Request
                && message.PlayerType == PlayerType.AI)
            {
                _getMoveWorker.RunWorkerAsync(message);
            }
        }

        void _getMoveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MoveMessage response = new MoveMessage() { PlayerType = PlayerType.AI, Type = MoveMessageType.Response, Move = ((Move)e.Result), };
            _eventAggregator.Publish(response);
        }

        void _getMoveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MoveMessage message = (MoveMessage)e.Argument;
            e.Result = GetMove(message.Board, message.TurnColor);
        }
        #endregion
    }
}

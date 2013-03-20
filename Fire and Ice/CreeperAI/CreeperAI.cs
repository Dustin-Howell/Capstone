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
    public class CreeperAI : IHandle<MoveRequestMessage>
    {
        //debug variables\\
        private bool _reportTime = false;
        private bool _sort = true;
        private bool _parallel = false;

        private AICreeperBoard _board;
        private CreeperColor _turnColor;
        public int _MiniMaxDepth = 5;
        //private Dictionary<AIHash, double> _scoredBoards;

        public double TerritorialWeight { get; set; }
        public double MaterialWeight { get; set; }
        public double PositionalWeight { get; set; }
        public double ShortestDistanceWeight { get; set; }
        public double VictoryWeight { get; set; }
        public AIDifficulty Difficulty { get; set; }

        private static Random _Random = new Random();

        private IEventAggregator _eventAggregator;
        private BackgroundWorker _getMoveWorker;

        public CreeperAI(IEventAggregator eventAggregator)
        {
            _MiniMaxDepth = (Difficulty == AIDifficulty.Hard) ? 5 : 3;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _getMoveWorker = new BackgroundWorker();
            _getMoveWorker.DoWork += new DoWorkEventHandler(_getMoveWorker_DoWork);
            _getMoveWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getMoveWorker_RunWorkerCompleted);
        }

        public CreeperAI(Dictionary<String, double> weights)
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
                double score = -ScoreAlphaBetaNegaMaxMove(board, _turnColor.Opposite(), -beta, -alpha, _MiniMaxDepth - 1);
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
                    score += ScoreBoardShortestDistance(board, turnColor) * ShortestDistanceWeight;
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
            double score = 0d;

            if (turn == CreeperColor.Fire)
            {
                foreach (AIBoardNode peg in board.WhitePegs)
                {
                    score += 2 - Math.Sqrt(Math.Pow(peg.Row - 3, 2) + (Math.Pow(peg.Column - 3, 2)));
                }
            }
            else
            {
                foreach (AIBoardNode peg in board.BlackPegs)
                {
                    score += 2 - Math.Sqrt(Math.Pow(peg.Row - 3, 2) + (Math.Pow(peg.Column - 3, 2)));
                }
            }

            return score;
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
        #endregion

        #region Event Stuff
        public void Handle(MoveRequestMessage message)
        {
            if (message.Responder == PlayerType.AI)
                _getMoveWorker.RunWorkerAsync(message.Color);
        }

        void _getMoveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MoveResponseMessage response = new MoveResponseMessage(((Move)e.Result), PlayerType.AI);
            _eventAggregator.Publish(response);
        }

        void _getMoveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = GetMove(GameTracker.Board, (CreeperColor)e.Argument);
        }
        #endregion
    }
}

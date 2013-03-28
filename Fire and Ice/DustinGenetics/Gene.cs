using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.IO;
using System.Reflection;

namespace DustinGenetics
{
    public class Gene
    {
        public static Random _Random = new Random();

        private Dictionary<String, double> _weights;

        public int Wins { get; set; }
        public int Losses { get; set; }
        public int GamesPlayed { get { return Wins + Losses; } }
        public double WinPercentage { get { return (double)Wins / (double)GamesPlayed; } }

        public void ResetGames()
        {
            Wins = 0;
            Losses = 0;
        }

        public Gene()
        {
            _weights = new Dictionary<string, double>();
            Type T = typeof(CreeperAI.AI);
            foreach (String property in T.GetProperties().Where(x => x.Name.Contains("Weight")).Select(x => x.Name))
            {
                if (property.Contains("Power"))
                {
                    _weights[property] = _Random.NextDouble() * 2;
                }
                else
                {
                    _weights[property] = _Random.Next(-100, 100);
                }
            }
        }

        public Gene(Gene gene)
        {
            foreach (String key in _weights.Keys)
            {
                _weights[key] = gene._weights[key];
            }
        }

        public Gene(Dictionary<String, double> weights)
        {
            _weights = new Dictionary<string,double>(weights);
        }

        public Gene CrossWith(Gene gene)
        {
            foreach (String key in _weights.Keys)
            {
                _weights[key] = _Random.Next() % 2 == 0 ? gene._weights[key] : _weights[key];
            }

            return new Gene(_weights);
        }

        public Gene Mutate()
        {
            foreach (String key in _weights.Keys)
            {
                if (key.Contains("Power"))
                {
                    _weights[key] = _Random.Next() % 2 == 0 ? _weights[key] + _Random.NextDouble() / 5 : _weights[key] - _Random.NextDouble() / 5;
                }
                else
                {
                    _weights[key] = _Random.Next() % 2 == 0 ? _weights[key] + _Random.Next() % 5 : _weights[key] - _Random.Next() % 5;
                }
            }

            return new Gene(_weights);
        }

        public bool Defeats(Gene opponent)
        {
            try
            {
                int moveCount = 0;
                CreeperColor turn = CreeperColor.Ice;
                CreeperBoard board = new CreeperBoard();
                CreeperAI.AI thisAI = new CreeperAI.AI(_weights);
                CreeperAI.AI opponentAI = new CreeperAI.AI(opponent._weights);

                while (!board.IsFinished(turn))
                {
                    moveCount++;
                    if (moveCount > 70)
                    {
                        Console.WriteLine("Move Loop");
                        return false;
                    }
                    turn = turn.Opposite();

                    if (turn == CreeperColor.Fire)
                    {
                        board.Move(thisAI.GetMove(board, turn));
                    }
                    else
                    {
                        board.Move(opponentAI.GetMove(board, turn));
                    }
                }

                return turn == CreeperColor.Fire && board.GetGameState(turn) == CreeperGameState.Complete;
            }
            catch (Exception)
            {
                Console.WriteLine("Exception");
                return false;
            }
        }

        public void Print()
        {
            //Console.Write("Material: {0}\nTerritorial: {1}\nPath: {2}\nVictory: {3}\nPositional: {4}\n\n", MaterialWeight, TerritorialWeight, PathToVictoryWeight, VictoryWeight, PositionalWeight);
            foreach (String key in _weights.Keys)
            {
                Console.WriteLine("{0}: {1}", key, _weights[key]);
            }
        }

        public void WriteToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                foreach (String key in _weights.Keys)
                {
                    writer.WriteLine("{0}: {1}", key, _weights[key]);
                }
                writer.WriteLine();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;
using System.IO;

namespace DustinGenetics
{
    public class Gene
    {
        public static Random _Random = new Random();

        public double MaterialWeight { get; set; }
        public double TerritorialWeight { get; set; }
        public double PositionalWeight { get; set; }
        public double PathToVictoryWeight { get; set; }
        public double VictoryWeight { get; set; }

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
            MaterialWeight = _Random.Next(-100, 100);
            TerritorialWeight = _Random.Next(-100, 100);
            PositionalWeight = _Random.Next(-100, 100);
            PathToVictoryWeight = _Random.Next(-100, 100);
            VictoryWeight = _Random.Next(100, 1000);
        }

        public Gene(Gene gene)
        {
            MaterialWeight = gene.MaterialWeight;
            TerritorialWeight = gene.TerritorialWeight;
            PositionalWeight = gene.PositionalWeight;
            PathToVictoryWeight = gene.PathToVictoryWeight;
            VictoryWeight = gene.VictoryWeight;
        }

        public Gene(double materialWeight, double territorialWeight, double positionalWeight, double pathToVictoryWeight, double victoryWeight)
        {
            MaterialWeight = materialWeight;
            TerritorialWeight = territorialWeight;
            PositionalWeight = positionalWeight;
            PathToVictoryWeight = pathToVictoryWeight;
            VictoryWeight = victoryWeight;
        }

        public Gene CrossWith(Gene gene)
        {
            double material = _Random.Next() % 2 == 0 ? gene.MaterialWeight : MaterialWeight;
            double territory = _Random.Next() % 2 == 0 ? gene.TerritorialWeight : TerritorialWeight;
            double position = _Random.Next() % 2 == 0 ? gene.PositionalWeight : PositionalWeight;
            double path = _Random.Next() % 2 == 0 ? gene.PathToVictoryWeight : PathToVictoryWeight;
            double victory = _Random.Next() % 2 == 0 ? gene.VictoryWeight : VictoryWeight;

            return new Gene(material, territory, position, path, victory);
        }

        public Gene Mutate()
        {
            double material = _Random.Next() % 2 == 0 ? MaterialWeight + _Random.Next() % 5 : MaterialWeight - _Random.Next() % 5;
            double territory = _Random.Next() % 2 == 0 ? TerritorialWeight + _Random.Next() % 5 : TerritorialWeight - _Random.Next() % 5;
            double position = _Random.Next() % 2 == 0 ? PositionalWeight + _Random.Next() % 5 : PositionalWeight - _Random.Next() % 5;
            double path = _Random.Next() % 2 == 0 ? PathToVictoryWeight + _Random.Next() % 5 : PathToVictoryWeight - _Random.Next() % 5;
            double victory = _Random.Next() % 2 == 0 ? VictoryWeight + _Random.Next() % 5 : VictoryWeight - _Random.Next() % 5;

            return new Gene(material, territory, position, path, victory);
        }

        public bool Defeats(Gene opponent)
        {
            int moveCount = 0;
            CreeperColor turn = CreeperColor.Black;
            CreeperBoard board = new CreeperBoard();
            CreeperAI.CreeperAI thisAI = new CreeperAI.CreeperAI(TerritorialWeight, MaterialWeight, PositionalWeight, PathToVictoryWeight, VictoryWeight);
            CreeperAI.CreeperAI opponentAI = new CreeperAI.CreeperAI(opponent.TerritorialWeight, opponent.MaterialWeight, opponent.PositionalWeight, opponent.PathToVictoryWeight, opponent.VictoryWeight);

            while (!board.IsFinished(turn))
            {
                moveCount++;
                if (moveCount > 70)
                {
                    Console.WriteLine("Move Loop");
                    return false;
                }
                turn = turn.Opposite();

                if (turn == CreeperColor.White)
                {
                    board.Move(thisAI.GetMove(board, turn));
                }
                else
                {
                    board.Move(opponentAI.GetMove(board, turn));
                }
            }

            return turn == CreeperColor.White && board.GetGameState(turn) == CreeperGameState.Complete;
        }

        public void Print()
        {
            Console.WriteLine("Material: {0}\nTerritorial: {1}\nPath: {2}\nVictory: {3}\nPositional: {4}", MaterialWeight, TerritorialWeight, PathToVictoryWeight, VictoryWeight, PositionalWeight);
        }

        public void WriteToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine("Material: {0}\nTerritorial: {1}\nPath: {2}\nVictory: {3}\nPositional: {4}", MaterialWeight, TerritorialWeight, PathToVictoryWeight, VictoryWeight, PositionalWeight);
            }
        }
    }
}

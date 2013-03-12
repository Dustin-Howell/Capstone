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
        private double CentralRelativeWeight { get; set; }
        private double LinearWeight { get; set; }
        public double PathToVictoryWeight { get; set; }
        public double VictoryWeight { get; set; }
        public double PowerWeight { get; set; }

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
            MaterialWeight = _Random.Next(0, 100);
            TerritorialWeight = _Random.Next(0, 100);
            PositionalWeight = _Random.Next(-100, 100);
            CentralRelativeWeight = _Random.Next(-100, 100);
            LinearWeight = _Random.Next(-100, 100);
            PathToVictoryWeight = _Random.Next(50, 150);
            PowerWeight = _Random.NextDouble() * 2;
            VictoryWeight = _Random.Next(100, 1000);
        }

        public Gene(Gene gene)
        {
            MaterialWeight = gene.MaterialWeight;
            TerritorialWeight = gene.TerritorialWeight;
            PositionalWeight = gene.PositionalWeight;
            CentralRelativeWeight = gene.CentralRelativeWeight;
            LinearWeight = gene.LinearWeight;
            PathToVictoryWeight = gene.PathToVictoryWeight;
            PowerWeight = gene.PowerWeight;
            VictoryWeight = gene.VictoryWeight;
        }

        public Gene(double materialWeight, double territorialWeight, double positionalWeight, double centralRelativeWieght, double linearWeight, double pathToVictoryWeight, double powerWeight, double victoryWeight)
        {
            MaterialWeight = materialWeight;
            TerritorialWeight = territorialWeight;
            PositionalWeight = positionalWeight;
            CentralRelativeWeight = centralRelativeWieght;
            LinearWeight = linearWeight;
            PathToVictoryWeight = pathToVictoryWeight;
            PowerWeight = powerWeight;
            VictoryWeight = victoryWeight;
        }

        public Gene CrossWith(Gene gene)
        {
            double material = _Random.Next() % 2 == 0 ? gene.MaterialWeight : MaterialWeight;
            double territory = _Random.Next() % 2 == 0 ? gene.TerritorialWeight : TerritorialWeight;
            double position = _Random.Next() % 2 == 0 ? gene.PositionalWeight : PositionalWeight;
            double central = _Random.Next() % 2 == 0 ? gene.CentralRelativeWeight : CentralRelativeWeight;
            double linear = _Random.Next() % 2 == 0 ? gene.LinearWeight : LinearWeight;
            double path = _Random.Next() % 2 == 0 ? gene.PathToVictoryWeight : PathToVictoryWeight;
            double powerWeight = _Random.Next() % 2 == 0 ? gene.PowerWeight : PowerWeight;
            double victory = _Random.Next() % 2 == 0 ? gene.VictoryWeight : VictoryWeight;

            return new Gene(material, territory, position, central, linear, path, powerWeight, victory);
        }

        public Gene Mutate()
        {
            double material = _Random.Next() % 2 == 0 ? MaterialWeight + _Random.Next() % 15 : MaterialWeight - _Random.Next() % 15;
            double territory = _Random.Next() % 2 == 0 ? TerritorialWeight + _Random.Next() % 15 : TerritorialWeight - _Random.Next() % 15;
            double position = _Random.Next() % 2 == 0 ? PositionalWeight + _Random.Next() % 15 : PositionalWeight - _Random.Next() % 15;
            double central = _Random.Next() % 2 == 0 ? CentralRelativeWeight + _Random.Next() % 15 : CentralRelativeWeight - _Random.Next() % 15;
            double linear = _Random.Next() % 2 == 0 ? LinearWeight + _Random.Next() % 15 : LinearWeight - _Random.Next() % 15;
            double path = _Random.Next() % 2 == 0 ? PathToVictoryWeight + _Random.Next() % 15 : PathToVictoryWeight - _Random.Next() % 15;
            double powerWeight = ((PowerWeight * 50) + _Random.Next(-15, 15)) / 100;
            double victory = _Random.Next() % 2 == 0 ? VictoryWeight + _Random.Next() % 15 : VictoryWeight - _Random.Next() % 15;

            return new Gene(material, territory, position, central, linear, path, powerWeight, victory);
        }

        public bool Defeats(Gene opponent)
        {
            try
            {
                int moveCount = 0;
                CreeperColor turn = CreeperColor.Ice;
                CreeperBoard board = new CreeperBoard();
                CreeperAI.CreeperAI thisAI = new CreeperAI.CreeperAI(TerritorialWeight, MaterialWeight, PositionalWeight, CentralRelativeWeight, LinearWeight, PathToVictoryWeight, PowerWeight, VictoryWeight);
                CreeperAI.CreeperAI opponentAI = new CreeperAI.CreeperAI(opponent.TerritorialWeight, opponent.MaterialWeight, opponent.PositionalWeight, opponent.CentralRelativeWeight, opponent.LinearWeight, opponent.PathToVictoryWeight, opponent.PowerWeight, opponent.VictoryWeight);

                while (!board.IsFinished(turn))
                {
                    moveCount++;
                    if (moveCount > 140)
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
            Console.Write("Material: {0}\nTerritorial: {1}\nPath: {2}\nVictory: {3}\nPositional: {4}\nLinear: {5}\nCentralRelative: {6}\nPower: {7}\n\n", MaterialWeight, TerritorialWeight, PathToVictoryWeight, VictoryWeight, PositionalWeight, LinearWeight, CentralRelativeWeight, PowerWeight);
        }

        public void WriteToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine("Material   : {0}", MaterialWeight);
                writer.WriteLine("Territorial: {0}", TerritorialWeight);
                writer.WriteLine("Positional : {0}", PositionalWeight);
                writer.WriteLine("Central    : {0}", CentralRelativeWeight);
                writer.WriteLine("Linear     : {0}", LinearWeight);
                writer.WriteLine("Path       : {0}", PathToVictoryWeight);
                writer.WriteLine("Power      : {0}", PowerWeight);
                writer.WriteLine("Victory    : {0}", VictoryWeight);
                writer.WriteLine("Win        : {0}%", WinPercentage * 100);
                writer.WriteLine();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace DustinGenetics
{
    public class Gene
    {
        public static Random _Random = new Random();

        public double MaterialWeight { get; set; }
        public double TerritorialWeight { get; set; }
        public double PathToVictoryWeight { get; set; }
        public double VictoryWeight { get; set; }

        public Gene()
        {
            MaterialWeight = _Random.Next(1, 100);
            TerritorialWeight = _Random.Next(1, 100);
            PathToVictoryWeight = _Random.Next(1, 100);
            VictoryWeight = _Random.Next(1, 100);
        }

        public Gene(Gene gene)
        {
            MaterialWeight = gene.MaterialWeight;
            TerritorialWeight = gene.TerritorialWeight;
            PathToVictoryWeight = gene.PathToVictoryWeight;
            VictoryWeight = gene.VictoryWeight;
        }

        private Gene(double materialWeight, double territorialWeight, double pathToVictoryWeight, double victoryWeight)
        {
            MaterialWeight = materialWeight;
            TerritorialWeight = territorialWeight;
            PathToVictoryWeight = pathToVictoryWeight;
            VictoryWeight = victoryWeight;
        }

        public Gene CrossWith(Gene gene)
        {
            double material = _Random.Next() % 2 == 0 ? gene.MaterialWeight : MaterialWeight;
            double territory = _Random.Next() % 2 == 0 ? gene.TerritorialWeight : TerritorialWeight;
            double path = _Random.Next() % 2 == 0 ? gene.PathToVictoryWeight : PathToVictoryWeight;
            double victory = _Random.Next() % 2 == 0 ? gene.VictoryWeight : VictoryWeight;

            return new Gene(material, territory, path, victory);
        }

        public Gene Mutate()
        {
            return new Gene();
        }

        public bool Defeats(Gene opponent)
        {
            int moveCount = 0;
            CreeperColor turn = CreeperColor.Black;
            CreeperBoard board = new CreeperBoard();
            CreeperAI.CreeperAI thisAI = new CreeperAI.CreeperAI(TerritorialWeight, MaterialWeight, PathToVictoryWeight, VictoryWeight);
            CreeperAI.CreeperAI opponentAI = new CreeperAI.CreeperAI(opponent.TerritorialWeight, opponent.MaterialWeight, opponent.PathToVictoryWeight, opponent.VictoryWeight);

            while (!board.IsFinished(turn))
            {
                moveCount++;
                if (moveCount > 200)
                {
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
            Console.WriteLine("Material: {0}\nTerritorial: {1}\nPath: {2}\nVictory: {3}\n", MaterialWeight, TerritorialWeight, PathToVictoryWeight, VictoryWeight);
        }
    }
}

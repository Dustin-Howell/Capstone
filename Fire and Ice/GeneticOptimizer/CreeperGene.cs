using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnticipatingMinds.ArtificialMind.GeneticAlgorithms;
using Creeper;

namespace GeneticOptimizer
{
    public class CreeperGene : Gene
    {
        public static CreeperGene CurrentOptimalGene = new CreeperGene(1,1,1,1);
        private static Random _Random = new Random();

        public double _materialWeight;
        public double _territorialWeight;
        public double _pathWeight;
        public double _victoryWeight;

        public CreeperGene(double materialWeight, double territorialWeight, double pathWeight, double victoryWeight)
        {
            _materialWeight = materialWeight;
            _territorialWeight = territorialWeight;
            _pathWeight = pathWeight;
            _victoryWeight = victoryWeight;
        }

        public override CreeperGene CrossOver(CreeperGene crossOverWith)
        {
            return new CreeperGene((_materialWeight + crossOverWith._materialWeight) / 2,
                                    (_territorialWeight + crossOverWith._territorialWeight) / 2,
                                    (_pathWeight + crossOverWith._pathWeight) / 2,
                                    (_victoryWeight + crossOverWith._victoryWeight) / 2);
        }

        public override double Evaluate()
        {
            CreeperAI.CreeperAI newCreeperAI = new CreeperAI.CreeperAI(_territorialWeight, 
                                                                        _materialWeight, 
                                                                        _pathWeight, 
                                                                        _victoryWeight);
            CreeperAI.CreeperAI currentOptimalAI = new CreeperAI.CreeperAI(CurrentOptimalGene._territorialWeight, 
                                                                        CurrentOptimalGene._materialWeight, 
                                                                        CurrentOptimalGene._pathWeight,
                                                                        CurrentOptimalGene._victoryWeight);

            CreeperBoard board = new CreeperBoard();
            CreeperColor turn = CreeperColor.Black;

            int moveCount = 0;
            while (!board.IsFinished(turn)
                && board.Pegs.Any(x => x.Color == CreeperColor.White)
                && board.Pegs.Any(x => x.Color == CreeperColor.Black))
            {
                turn = (turn == CreeperColor.White) ? CreeperColor.Black : CreeperColor.White;

                if (turn == CreeperColor.White)
                {
                    board.Move(newCreeperAI.GetMove(board, turn));
                }
                else
                {
                    board.Move(currentOptimalAI.GetMove(board, turn));
                }

                moveCount++;
            }

            if (turn == CreeperColor.White)
            {
                return 100 / moveCount;
            }

            else
            {
                return 1;
            }
        }

        public override void Initialize()
        {
            _territorialWeight = _Random.Next(0, 100) + _Random.NextDouble();
            _materialWeight = _Random.Next(0, 100) + _Random.NextDouble();
            _pathWeight = _Random.Next(0, 100) + _Random.NextDouble();
            _victoryWeight = _Random.Next(0, 100) + _Random.NextDouble();
        }

        public override void Mutate()
        {
            _territorialWeight += _Random.Next(-10, 10) + _Random.NextDouble();
            _materialWeight += _Random.Next(-10, 10) + _Random.NextDouble();
            _pathWeight += _Random.Next(-10, 10) + _Random.NextDouble();
            _victoryWeight += _Random.Next(-10, 10) + _Random.NextDouble();
        }
    }
}

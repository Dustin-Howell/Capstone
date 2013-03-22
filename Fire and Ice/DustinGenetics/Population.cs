using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DustinGenetics
{
    public class Population
    {
        private List<Gene> GenePool;

        public Population(int size)
        {
            GenePool = new List<Gene>();

            for (int i = 0; i < size; i++)
            {
                GenePool.Add(new Gene());
            }
        }

        public Population(List<Gene> genePool)
        {
            GenePool = genePool;
        }

        public Population( int size, Gene seedGene)
        {
            GenePool = new List<Gene>();

            for (int i = 0; i < size; i++)
            {
                if (i % 2 == 0)
                {
                    GenePool.Add(new Gene().CrossWith(seedGene));
                }
                else
                {
                    GenePool.Add(seedGene.Mutate());
                }
            }
        }

        public List<Gene> GetTopHalf()
        {
            List<Gene> topHalf = new List<Gene>();
            for (int i = 0; i < GenePool.Count; i++)
            {
                Gene gene = GenePool[i];
                Console.WriteLine("Analyzing Gene {0}", GenePool.IndexOf(gene));
                gene.Print();

                for (int j = 0; j < GenePool.Count; j++)
                {
                    Gene opponentGene = GenePool[j];
                    if (GenePool[j] != gene)
                    {
                        if (gene.Defeats(GenePool[j]))
                        {
                            gene.Wins += 1;
                            opponentGene.Losses += 1;
                            Console.WriteLine("Win");
                        }
                        else
                        {
                            gene.Losses += 1;
                            opponentGene.Wins += 1;
                            Console.WriteLine("Loss");
                        }
                    }
                }

            }

            double averageWinPercentage = GenePool.Average((y) => y.WinPercentage);
            topHalf = GenePool.Where(x => x.WinPercentage > averageWinPercentage).ToList();

            return topHalf;
        }

        public Gene GetBestGene()
        {
            Gene bestGene = new Gene();
            List<Gene> VictoryGenes = new List<Gene>();

            int roundNumber = 0;
            while (GenePool.Count > 1)
            {
                Console.WriteLine("Round {0}", roundNumber++);
                Console.WriteLine("Gene Pool Size: {0}\n", GenePool.Count);

                VictoryGenes = GetTopHalf();

                if (VictoryGenes.Any())
                {
                    bestGene = VictoryGenes.First();
                }

                GenePool = VictoryGenes;
                VictoryGenes = new List<Gene>();
            }

            return bestGene;
        }

    }
}

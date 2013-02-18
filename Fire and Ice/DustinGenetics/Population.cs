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
        private List<Gene> VictoryGenes;

        public Population(int size)
        {
            GenePool = new List<Gene>();
            VictoryGenes = new List<Gene>();

            for (int i = 0; i < size; i++)
            {
                GenePool.Add(new Gene());
            }
        }

        public Population(List<Gene> genePool)
        {
            GenePool = genePool;
            VictoryGenes = new List<Gene>();
        }

        public Gene GetBestGene()
        {
            Gene bestGene = new Gene();

            int roundNumber = 0;
            while (GenePool.Count > 1)
            {
                Console.WriteLine("Round {0}", roundNumber++);
                Console.WriteLine("Gene Pool Size: {0}\n", GenePool.Count);
                for (int i = 0; i < GenePool.Count; i++)
                {
                    Gene gene = GenePool[i];
                    Console.WriteLine("Analyzing Gene {0}", GenePool.IndexOf(gene));
                    int victoryCount = 0;

                    for (int j = 0; j < GenePool.Count; j++)
                    {
                        Gene opponentGene = GenePool[j];
                        if (GenePool[j] != gene)
                        {
                            if (gene.Defeats(GenePool[j]))
                            {
                                victoryCount++;
                            }
                        }
                    }

                    if (victoryCount >= (GenePool.Count / 2))
                    {
                        Console.WriteLine("Pass", GenePool.IndexOf(gene));
                        VictoryGenes.Add(new Gene(gene));
                    }
                    else
                    {
                        Console.WriteLine("Fail", GenePool.IndexOf(gene));
                    }
                    Console.WriteLine();
                }

                if (VictoryGenes.Any())
                {
                    bestGene = VictoryGenes.First();
                }
                GenePool = VictoryGenes;
                VictoryGenes = new List<Gene>();
            }

            return bestGene;
        }

        public void WriteToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                foreach (Gene gene in GenePool)
                {
                    writer.WriteLine("{0} {1} {2} {3}", gene.MaterialWeight, gene.TerritorialWeight, gene.PathToVictoryWeight, gene.VictoryWeight);
                }
            }
        }
    }
}

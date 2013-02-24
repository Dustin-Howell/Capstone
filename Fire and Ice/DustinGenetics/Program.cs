using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustinGenetics
{
    class Program
    {
        public static string LogPath = @"C:\Users\dhowell2\Dropbox\Capstone\Genetics\bestGenes.log";
        public static Gene SeedGene = new Gene(-21, 75, -74, 89, 107);
        public static bool UseSeed = false;

        static void Main(string[] args)
        {
            Random random = new Random();
            int populationSize = 10;
            int rounds = 10;
            Population population = (UseSeed)? new Population(populationSize, SeedGene) : new Population(populationSize);
            List<Gene> genePool;

            while (true)
            {
                genePool = new List<Gene>();

                for (int i = 0; i < rounds; i++)
                {
                    genePool = population.GetTopHalf();

                    while (genePool.Count > 0
                            && genePool.Count < populationSize)
                    {
                        Gene randomGene1 = genePool[random.Next() % genePool.Count];
                        Gene randomGene2 = genePool[random.Next() % genePool.Count];
                        genePool.Add(randomGene1.CrossWith(randomGene2));
                    }

                    if (genePool.Any())
                    {
                        population = new Population(genePool);
                    }
                    else
                    {
                        population = new Population(populationSize);
                    }
                }

                Gene bestGene = population.GetBestGene();
                bestGene.Print();
                bestGene.WriteToFile(LogPath);
                population = new Population(populationSize, bestGene);
            }
        }
    }
}

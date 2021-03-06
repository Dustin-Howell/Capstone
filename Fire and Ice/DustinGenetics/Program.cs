﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustinGenetics
{
    class Program
    {
        public static string LogPath = @"C:\Users\dhowell2\Dropbox\Capstone\Genetics\bestGenes.log";
        
        public static Gene SeedGene = new Gene(new Dictionary<String, double>()
        {
            {"TerritorialWeight", 84},
            {"MaterialWeight", 51},
            {"PositionalWeight", 12},
            {"ShortestDistanceWeight", 40},
            {"PathPowerWeight", 1.72285873988776},
            {"VictoryWeight", 104},
        });

        public static bool UseSeed = true;

        static void Main(string[] args)
        {
            Random random = new Random();
            int populationSize = 12;
            int rounds = 3;
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
                        Gene newGene;
                        if (random.Next() % 4 == 0)
                        {
                            newGene = new Gene();
                        }
                        else
                        {
                            Gene randomGene1 = genePool[random.Next() % genePool.Count];
                            Gene randomGene2 = genePool[random.Next() % genePool.Count];
                            newGene = randomGene1.CrossWith(randomGene2);
                        }
                        genePool.Add(newGene);
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

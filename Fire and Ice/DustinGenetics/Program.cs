using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustinGenetics
{
    class Program
    {
        static void Main(string[] args)
        {
            int populationSize = 20;
            Population population = new Population(populationSize);
            List<Gene> genePool = new List<Gene>();

            while (true)
            {
                Gene bestGene = population.GetBestGene();
                genePool.Add(bestGene);

                if (genePool.Count == populationSize)
                {
                    population = new Population(genePool);
                    population.WriteToFile("GenePool.log");
                }
                else
                {
                    population = new Population(populationSize);
                }
            }
        }
    }
}

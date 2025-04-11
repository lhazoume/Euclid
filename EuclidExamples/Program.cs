using System;

namespace EuclidExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lancement du projet EuclidExamples...");

            
            DistributionExample.Binomial();
            DistributionExample.Poisson();
            DistributionExample.Normal();
            DistributionExample.Exponential();
            DistributionExample.Uniform();
            DistributionExample.Cauchy();
        }
    }
}

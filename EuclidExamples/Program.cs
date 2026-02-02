using System;

namespace EuclidExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lancement du projet EuclidExamples...");

            #region Distributions
            //DistributionExample.Binomial();
            //DistributionExample.Poisson();
            //DistributionExample.Normal();
            //DistributionExample.Exponential();
            //DistributionExample.Uniform();
            //DistributionExample.Cauchy();
            #endregion
            // Rajout test interpolation
            #region Interpolation
            InterpolationExample.HymanExample();
            InterpolationExample.CubicSplineExample();
            #endregion
        }
    }
}

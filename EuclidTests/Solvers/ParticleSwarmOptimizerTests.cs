using Euclid.Distributions.Continuous;
using Euclid.Optimizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Euclid.Solvers.Tests
{
    [TestClass()]
    public class ParticleSwarmOptimizerTests
    {
        private static double Rosenbrock(Vector v)
        {
            return Math.Pow(1 - v[0], 2) + 100 * Math.Pow(v[1] - v[0] * v[0], 2);
        }

        private static double Rastrigin(Vector v)
        {
            return 10 * v.Size + v.Data.Sum(x => x * x - 10 * Math.Cos(2 * Math.PI * x));
        }

        [TestMethod()]
        public void ParticleSwarmOptimizerTest()
        {
            UniformDistribution uniform = new UniformDistribution(0, 1);
            int dimension = 10;
            ParticleSwarmOptimizer pso = new ParticleSwarmOptimizer(v=> 0.0,
                Enumerable.Range(0, 1000).Select(i => Vector.CreateRandom(dimension, uniform)),
                OptimizationType.Min,
                100, 10);

            //Assert.IsTrue(pso != null && pso.MaxIterations == 100 && pso.MaxStaticIterations == 10 && pso.SwarmSize == 1000);
        }

        [TestMethod()]
        public void SolveRosenbrockTest()
        {
            UniformDistribution uniform = new UniformDistribution(-2, 2);
            int dimension = 2;
            ParticleSwarmOptimizer pso = new ParticleSwarmOptimizer(Rosenbrock,
                Enumerable.Range(0, 10000).Select(i => Vector.CreateRandom(dimension, uniform)),
                OptimizationType.Min,
                100, 10);
            pso.Optimize(false);

            //Assert.IsTrue(Rosenbrock(pso.Result) < 1e-5);
        }

        [TestMethod()]
        public void SolveRastriginTest()
        {
            UniformDistribution uniform = new UniformDistribution(-5, 5);
            int dimension = 5;
            ParticleSwarmOptimizer pso = new ParticleSwarmOptimizer(Rastrigin,
                Enumerable.Range(0, 10000).Select(i => Vector.CreateRandom(dimension, uniform)),
                OptimizationType.Min,
                10000, 100);
            pso.Optimize(false);

            //Assert.IsTrue(Rastrigin(pso.Result) < 1e-5);
        }
    }
}
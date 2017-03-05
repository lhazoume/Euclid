using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Euclid.Distributions.Continuous;

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
            ParticleSwarmOptimizer pso = new ParticleSwarmOptimizer(1000,
                Vector.Create(dimension, 0.0), Vector.Create(dimension, 1.0),
                v => 0.0,
                s => Enumerable.Range(0, s).Select(i => Vector.CreateRandom(dimension, uniform)).ToArray(),
                100, 10);

            Assert.IsTrue(pso != null && pso.Dimension == dimension && pso.MaxIterations == 100 && pso.MaxStaticIterations == 10 && pso.SwarmSize == 1000);
        }

        [TestMethod()]
        public void SolveRosenbrockTest()
        {
            UniformDistribution uniform = new UniformDistribution(-2, 2);
            int dimension = 2;
            ParticleSwarmOptimizer pso = new ParticleSwarmOptimizer(10000,
                Vector.Create(dimension, -2.0), Vector.Create(dimension, 2.0),
                Rosenbrock,
                s => Enumerable.Range(0, s).Select(i => Vector.CreateRandom(dimension, uniform)).ToArray(),
                100, 10);
            pso.Solve();

            Assert.IsTrue(Rosenbrock(pso.Result)<1e-5);
        }

        [TestMethod()]
        public void SolveRastriginTest()
        {
            UniformDistribution uniform = new UniformDistribution(-5, 5);
            int dimension = 5;
            ParticleSwarmOptimizer pso = new ParticleSwarmOptimizer(10000,
                Vector.Create(dimension, -5.0), Vector.Create(dimension, 5.0),
                Rastrigin,
                s => Enumerable.Range(0, s).Select(i => Vector.CreateRandom(dimension, uniform)).ToArray(),
                10000, 100);
            pso.Solve();

            Assert.IsTrue(Rastrigin(pso.Result) < 1e-5);
        }
    }
}
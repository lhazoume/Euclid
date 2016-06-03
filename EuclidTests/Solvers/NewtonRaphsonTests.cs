using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Solvers.Tests
{
    [TestClass()]
    public class NewtonRaphsonTests
    {
        [TestMethod()]
        public void NewtonRaphsonTest()
        {
            NewtonRaphson nr = new NewtonRaphson(0, x => (x - 2) * (x - 2), 100);
            Assert.IsTrue(nr.Status == SolverStatus.NotRan && nr.MaxIterations == 100 && nr.InitialGuess == 0);
        }

        [TestMethod()]
        public void SolveTest()
        {
            NewtonRaphson nr = new NewtonRaphson(0, x => (x - 2) * (x - 2) - 1, 1000);
            nr.Solve();
            Assert.AreEqual(1, nr.Result, nr.AbsoluteTolerance);
        }

        [TestMethod()]
        public void SolveTest1()
        {
            NewtonRaphson nr = new NewtonRaphson(0, x => (x - 2) * (x - 2), 1000);
            nr.Solve(1);
            Assert.AreEqual(1, nr.Result, nr.AbsoluteTolerance);
        }
    }
}
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
    public class GradientDescentTests
    {
        [TestMethod()]
        public void SolveTest()
        {
            Vector initialGuess = Vector.Create(1.0, 1.0),
                target = Vector.Create(3.0, 3.0);
            GradientDescent gd = new GradientDescent(initialGuess, LineSearch.Armijo, v => (v - target).SumOfSquares, 100, 100);
            gd.Minimize();
            Assert.AreEqual(0, (gd.Result - target).SumOfSquares, 1e-10);
        }

        [TestMethod()]
        public void SolveBFGSTest()
        {
            Vector initialGuess = Vector.Create(1.0, 1.0),
                target = Vector.Create(3.0, 3.0);
            GradientDescent gd = new GradientDescent(initialGuess, LineSearch.Armijo, v => (v - target).SumOfSquares, 100, 100);
            gd.MinimizeBFGS();
            Assert.AreEqual(0, (gd.Result - target).SumOfSquares, 1e-10);
        }
    }
}
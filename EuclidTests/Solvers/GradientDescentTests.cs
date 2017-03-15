using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euclid.Solvers.Tests
{
    [TestClass()]
    public class GradientDescentTests
    {
        [TestMethod()]
        public void GradientDescentTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GradientDescentTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void MinimizeTest()
        {
            Vector initialGuess = Vector.Create(1.0, 1.0),
                target = Vector.Create(3.0, 3.0);
            GradientDescent gd = new GradientDescent(initialGuess, LineSearch.Armijo, v => (v - target).SumOfSquares, OptimizationType.Min, 100, 100);
            gd.Optimize();
            Assert.AreEqual(0, (gd.Result - target).SumOfSquares, 1e-10);
        }

        [TestMethod()]
        public void MinimizeBFGSTest()
        {
            Vector initialGuess = Vector.Create(1.0, 1.0),
                target = Vector.Create(3.0, 3.0);
            GradientDescent gd = new GradientDescent(initialGuess, LineSearch.Armijo, v => (v - target).SumOfSquares, OptimizationType.Min, 100, 100);
            gd.OptimizeBFGS();
            Assert.AreEqual(0, (gd.Result - target).SumOfSquares, 1e-10);
        }
    }
}
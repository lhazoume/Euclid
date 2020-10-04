using Euclid.Optimizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euclid.Solvers.Tests
{
    [TestClass()]
    public class GradientDescentTests
    {
        [TestMethod()]
        public void GradientDescentWithGradientProvidedTest()
        {
            Vector initialGuess = Vector.Create(1.0, 1.0),
                target = Vector.Create(3.0, 3.0);
            GradientDescent gd = new GradientDescent(initialGuess, LineSearch.Armijo, v => (v - target).SumOfSquares, v => v - target, OptimizationType.Min, 100, 100);
            Assert.IsTrue(gd != null && gd.LineSearch == LineSearch.Armijo && gd.OptimizationType == OptimizationType.Min);
        }

        [TestMethod()]
        public void GradientDescentWithoutGradientTest()
        {
            Vector initialGuess = Vector.Create(1.0, 1.0),
                target = Vector.Create(3.0, 3.0);
            GradientDescent gd = new GradientDescent(initialGuess, LineSearch.Armijo, v => (v - target).SumOfSquares, v => v, OptimizationType.Min, 100, 100);
            Assert.IsNotNull(gd);
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
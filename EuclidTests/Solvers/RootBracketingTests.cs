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
    public class RootBracketingTests
    {
        [TestMethod()]
        public void RootBracketingTest()
        {
            RootBracketing rb = new RootBracketing(0, 3, x => (x - 2) * (x - 2), RootBracketingMethod.Dichotomy, 100);
            Assert.IsTrue(rb.Status == SolverStatus.NotRan && rb.MaxIterations == 100 && rb.LowerBound == 0 && rb.UpperBound == 3);
        }

        [TestMethod()]
        public void SolveDichotomyTest()
        {
            RootBracketing rb = new RootBracketing(0, 3, x => (x - 2) * (x - 2) - 1, RootBracketingMethod.Dichotomy, 1000);
            rb.Solve();
            Assert.AreEqual(1, rb.Result, rb.Tolerance);
        }

        [TestMethod()]
        public void SolveFalsePositionTest()
        {
            RootBracketing rb = new RootBracketing(0, 3, x => (x - 2) * (x - 2) - 1, RootBracketingMethod.FalsePosition, 1000);
            rb.Solve();
            Assert.AreEqual(1, rb.Result, rb.Tolerance);
        }

        [TestMethod()]
        public void SolveTest1()
        {
            RootBracketing rb = new RootBracketing(0, 3, x => (x - 2) * (x - 2), RootBracketingMethod.Dichotomy, 1000);
            rb.Solve(1);
            Assert.AreEqual(1, rb.Result, rb.Tolerance);
        }
    }
}
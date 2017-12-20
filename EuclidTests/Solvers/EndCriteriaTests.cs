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
    public class EndCriteriaTests
    {
        [TestMethod()]
        public void EndCriteriaTest()
        {
            EndCriteria ec = new EndCriteria();
            Assert.IsTrue(ec.Status == SolverStatus.NotRan);
        }

        [TestMethod()]
        public void MaxIterationsTest()
        {
            EndCriteria ec = new EndCriteria(10, 1000, 0.01, 0.01);
            bool shouldStop = false;
            while(!shouldStop)
                shouldStop = ec.ShouldStop(0.011);
            Assert.IsTrue(shouldStop && ec.Status == SolverStatus.IterationExceeded);
        }

        [TestMethod()]
        public void MaxStaticIterationsTest()
        {
            EndCriteria ec = new EndCriteria(1000, 10, 0.01, 0.01);
            bool shouldStop = false;
            while(!shouldStop)
                shouldStop = ec.ShouldStop(0.011);
            Assert.IsTrue(shouldStop && ec.Status == SolverStatus.StationaryFunction);
        }

        [TestMethod()]
        public void ValueConvergenceTest()
        {
            EndCriteria ec = new EndCriteria(100, 1000, 0.01, 0.01);
            bool shouldStop = false;
            double err = 1.0;

            while (!shouldStop)
            {
                shouldStop = ec.ShouldStop(err);
                err /= 2;
            }
            Assert.IsTrue(shouldStop && ec.Status == SolverStatus.FunctionConvergence);
        }

        [TestMethod()]
        public void GradientConvergenceTest()
        {
            EndCriteria ec = new EndCriteria(100, 1000, 0.01, 0.01);
            bool shouldStop = false;
            double err = 1.0;

            while (!shouldStop)
            {
                shouldStop = ec.ShouldStop(0.011, err);
                err /= 2;
            }
            Assert.IsTrue(shouldStop && ec.Status == SolverStatus.GradientConvergence);
        }
    }
}
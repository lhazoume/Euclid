using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Histograms.Tests
{
    [TestClass()]
    public class BoundTests
    {
        [TestMethod()]
        public void BoundTest()
        {
            Bound bound = new Bound(Math.PI, false);
            Assert.IsTrue((bound.Value == Math.PI) && bound.IsIncluded == false);
        }

        [TestMethod()]
        public void EqualsTest()
        {
            Bound b1 = new Bound(Math.PI, false),
                b2 = new Bound(Math.PI, false);
            Assert.IsTrue(b1.Equals(b2));
        }

        [TestMethod()]
        public void EqualsTest1()
        {
            Bound b1 = new Bound(Math.PI, false), b2 = new Bound(Math.PI, false);
            Assert.IsTrue(b1.Equals((object)b2));
        }

        [TestMethod()]
        public void CompareToFiniteFiniteTest()
        {
            Bound zeroInc = new Bound(0),
                oneInc = new Bound(1);
            Assert.AreEqual(zeroInc.CompareTo(oneInc), -1);
        }

        [TestMethod()]
        public void CompareToAlmostEqualTest()
        {
            Bound zeroInc = new Bound(0),
                zeroExc = new Bound(0, false);
            Assert.AreEqual(zeroInc.CompareTo(zeroExc), 0);
        }

        [TestMethod()]
        public void CompareToFiniteInfinityTest()
        {
            Bound infty = new Bound(double.PositiveInfinity),
                zeroInc = new Bound(0);
            Assert.AreEqual(zeroInc.CompareTo(infty), -1);
        }
    }
}
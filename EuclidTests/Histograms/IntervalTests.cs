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
    public class IntervalTests
    {
        [TestMethod()]
        public void IntervalTest()
        {
            Interval i = new Interval(0, 1, true, false);
            Assert.IsTrue(i.LowerBound.Value == 0 && i.UpperBound.Value == 1 && i.LowerBound.IsIncluded && !i.UpperBound.IsIncluded);
        }

        [TestMethod()]
        public void IntervalTest1()
        {
            Bound b1 = new Bound(0, true),
                b2 = new Bound(1, false);
            Interval i = new Interval(b1, b2);
            Assert.IsTrue(i.LowerBound.Value == 0 && i.UpperBound.Value == 1 && i.LowerBound.IsIncluded && !i.UpperBound.IsIncluded);
        }

        [TestMethod()]
        public void ContainsTest()
        {
            Interval i = new Interval(0, 1);
            double dIn = 0.5,
                dOut = 1.5;
            Assert.IsTrue(i.Contains(dIn) && !i.Contains(dOut));
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Interval i = new Interval(0, 1, true, false);
            Assert.AreEqual("[0, 1[", i.ToString());
        }

        [TestMethod()]
        public void IntersectionDisjointTest()
        {
            Interval i1 = new Interval(0, 1),
                i2 = new Interval(2, 3);
            Assert.IsTrue(Interval.Intersection(i1, i2) == null);
        }

        [TestMethod()]
        public void IntersectionJointTest()
        {
            Interval i1 = new Interval(0, 1),
                i2 = new Interval(0.5, 1.5);
            Interval cap = Interval.Intersection(i1, i2);
            Assert.AreEqual(0, Math.Abs(cap.LowerBound.Value - 0.5) + Math.Abs(cap.UpperBound.Value - 1));
        }
    }
}
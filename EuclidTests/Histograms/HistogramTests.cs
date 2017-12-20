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
    public class HistogramTests
    {
        [TestMethod()]
        public void HistogramTest()
        {
            Interval[] intervals = new Interval[] { new Interval(-1, 0, true, false), new Interval(0, 1, true, true) };
            Histogram histogram = new Histogram(intervals);
            Assert.IsTrue(histogram != null && histogram.Count == 2);
        }

        [TestMethod()]
        public void TabulateTest()
        {
            Interval[] intervals = new Interval[] { new Interval(-1, 0, true, false), new Interval(0, 1, true, true) };
            Histogram histogram = new Histogram(intervals);

            double[] values = new double[] { -2, -1.5, -1, -.5, 0, .5, 1, 1.5, 2 };
            foreach (double value in values)
                histogram.Tabulate(value);
            Assert.IsTrue(histogram.TotalItems == 5 && histogram[0] == 2 && histogram[1] == 3);
        }

        [TestMethod()]
        public void TabulateTest1()
        {
            Interval[] intervals = new Interval[] { new Interval(-1, 0, true, false), new Interval(0, 1, true, true) };
            Histogram histogram = new Histogram(intervals);

            double[] values = new double[] { -2, -1.5, -1, -.5, 0, .5, 1, 1.5, 2 };
            histogram.Tabulate(values);
            Assert.IsTrue(histogram.TotalItems == 5 && histogram[0] == 2 && histogram[1] == 3);
        }

        [TestMethod()]
        public void CreateHistogramTest()
        {
            Histogram histogram = Histogram.Create(-1, 1, 2);
            Assert.IsTrue(histogram != null && histogram.Count == 2 &&
                histogram.Intervals[0].Equals(new Interval(-1, 0, true, true)) &&
                histogram.Intervals[1].Equals(new Interval(0, 1, false, true)));
        }
    }
}
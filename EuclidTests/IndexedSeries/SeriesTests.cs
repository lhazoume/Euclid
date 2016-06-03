using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.IndexedSeries.Tests
{
    [TestClass()]
    public class SeriesTests
    {
        [TestMethod()]
        public void SeriesTest()
        {
            int size = 15;
            Series<DateTime, double, string> series = new Series<DateTime, double, string>(size);
            Assert.AreEqual(size, series.Rows);
        }

        [TestMethod()]
        public void SeriesTest1()
        {
            Series<DateTime, double, string> series = new Series<DateTime, double, string>("Titre",
                new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) },
                new double[] { 5, 6, 7, 8 });
            Assert.IsTrue(series.Label == "Titre" &&
                series.Legends.Length == 4 &&
                series.Data.Length == 4 &&
                series[0] == 5 && series[3] == 8);
        }

        [TestMethod()]
        public void CloneTest()
        {
            Series<DateTime, double, string> series = new Series<DateTime, double, string>("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 }),
                series2 = series.Clone();

            double diff = 0;
            for (int i = 0; i < series.Rows; i++)
                diff += Math.Abs(series[i] - series2[i]) + Math.Abs((series.GetLegend(i) - series2.GetLegend(i)).TotalMilliseconds);
            Assert.IsTrue(series.Label == series2.Label &&
                series.Legends.Length == series2.Legends.Length &&
                diff == 0);
        }

        [TestMethod()]
        public void RemoveRowAtTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RemoveTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ApplyOnDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ApplyOnLegendsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SumTest()
        {
            Series<DateTime, double, string> series = new Series<DateTime, double, string>("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 });
            Assert.AreEqual(26, series.Sum(x => x));
        }

        [TestMethod()]
        public void GetLegendTest()
        {
            Series<DateTime, double, string> series = new Series<DateTime, double, string>("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 });
            Assert.AreEqual(2007, series.GetLegend(2).Year);
        }

        [TestMethod()]
        public void SetLegendTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToXmlTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FromXmlTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToCSVTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FromCSVTest()
        {
            Assert.Fail();
        }
    }
}
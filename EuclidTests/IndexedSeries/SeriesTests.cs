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
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create(size);
            Assert.AreEqual(size, series.Rows);
        }

        [TestMethod()]
        public void SeriesTest1()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre",
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
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 }),
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
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre",
                new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) },
                new double[] { 5, 6, 7, 8 });
            DateTime d = new DateTime(2006, 6, 1);
            bool containsBefore = series.Legends.Contains(d);
            series.RemoveRowAt(d);
            bool containsAfter = series.Legends.Contains(d);
            Assert.IsTrue(containsBefore && !containsAfter && series[2] == 8);
        }

        [TestMethod()]
        public void AddTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre",
                new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) },
                new double[] { 5, 6, 7, 8 });
            DateTime d = new DateTime(2009, 6, 1);
            bool containsBefore = series.Legends.Contains(d);
            series.Add(d, 9);
            bool containsAfter = series.Legends.Contains(d);
            Assert.IsTrue(!containsBefore && containsAfter && series[4] == 9);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre",
                new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) },
                new double[] { 5, 6, 7, 8 });
            DateTime d = new DateTime(2006, 6, 1);
            bool containsBefore = series.Legends.Contains(d);
            series.Remove((x, y) => x == d);
            bool containsAfter = series.Legends.Contains(d);
            Assert.IsTrue(containsBefore && !containsAfter && series[2] == 8);
        }

        [TestMethod()]
        public void ApplyOnDataTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 });
            series.ApplyOnData(d => d - 1);
            Assert.AreEqual(4, series[0]);
        }

        [TestMethod()]
        public void ApplyOnLegendsTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 });
            series.ApplyOnLegends(d => d.AddMonths(1));
            Assert.AreEqual(7, series.GetLegend(0).Month);
        }

        [TestMethod()]
        public void SumTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 });
            Assert.AreEqual(26, series.Sum(x => x));
        }

        [TestMethod()]
        public void GetLegendTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 });
            Assert.AreEqual(2007, series.GetLegend(2).Year);
        }

        [TestMethod()]
        public void SetLegendTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create("Titre", new DateTime[] { new DateTime(2005, 6, 1), new DateTime(2006, 6, 1), new DateTime(2007, 6, 1), new DateTime(2008, 6, 1) }, new double[] { 5, 6, 7, 8 });
            series.SetLegend(2, new DateTime(2012, 6, 1));
            Assert.AreEqual(2012, series.GetLegend(2).Year);
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
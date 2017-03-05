using Euclid.IndexedSeries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Euclid.IndexedSeries.Tests
{
    [TestClass()]
    public class SeriesTests
    {
        private static Series<DateTime, double, string> BaseSeries(int n)
        {
            Header<DateTime> hdr = new Header<DateTime>();
            for (int i = 0; i < n; i++)
                hdr.Add(new DateTime(2000 + i, 6, 1));

            return Series<DateTime, double, string>.Create("Titre", hdr, Enumerable.Range(0, n).Select(i => Convert.ToDouble(i)));
        }

        [TestMethod()]
        public void CloneTest()
        {
            Assert.Fail();
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
            Assert.Fail();
        }

        [TestMethod()]
        public void GetLegendTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RenameTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToXmlTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToCSVTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateTest2()
        {
            Assert.Fail();
        }
    }
}
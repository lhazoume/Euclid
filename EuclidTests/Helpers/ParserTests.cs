using Euclid.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Euclid.Helpers.Tests
{
    [TestClass()]
    public class ParserTests
    {
        [TestMethod()]
        public void ParseDecimalTest()
        {
            decimal d = 0.5m;
            Assert.AreEqual(d, d.ToString().Parse<decimal>());
        }

        [TestMethod()]
        public void ParseIntTest()
        {
            int d = 25;
            Assert.AreEqual(d, d.ToString().Parse<int>());
        }

        [TestMethod()]
        public void ParseDoubleTest()
        {
            double d = 25.02548;
            Assert.AreEqual(d, d.ToString().Parse<double>());
        }

        [TestMethod()]
        public void ParseDateTimeTest()
        {
            DateTime d = new DateTime(2015, 8,9);
            Assert.AreEqual<DateTime>(d, d.ToString().Parse<DateTime>());
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Euclid.IndexedSeries.Tests
{
    [TestClass()]
    public class DataFrameTests
    {
        #region vars
        DataFrame<DateTime, double, string> _df;
        #endregion
        [TestInitialize()]
        public void Initialize()
        {
            _df = new DataFrame<DateTime, double, string>(
                new string[] { "A", "B" }, new DateTime[] { new DateTime(2016, 1, 1), new DateTime(2016, 1, 2), new DateTime(2016, 1, 3) }, new double[3, 2]
                { { 100, 200 }, { 101, 201 }, { 100, 200 } });
        }

        [TestMethod()]
        public void DataFrameTest()
        {
            int nbRows = 3, nbColumns = 2;
            Assert.AreEqual(nbColumns, _df.Columns);
            Assert.AreEqual(nbRows, _df.Rows);
        }

        [TestMethod()]
        public void DataFrameTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CloneTest()
        {
            DataFrame<DateTime, double, string> clone = _df.Clone();
            Assert.AreEqual(clone.Rows, _df.Rows);
            Assert.AreEqual(clone.Columns, _df.Columns);
        }

        [TestMethod()]
        public void RemoveColumnAtTest()
        {
            string labelOfTheColumnToRemove = "A";
            _df.RemoveColumnAt(labelOfTheColumnToRemove);
            Assert.IsFalse(_df.Labels.Contains(labelOfTheColumnToRemove));
        }

        [TestMethod()]
        public void RemoveRowAtTest()
        {
            DateTime LegentOfTheRowToRemove = new DateTime(2016, 1, 1);
            _df.RemoveRowAt(LegentOfTheRowToRemove);
            Assert.IsFalse(_df.Legends.Contains(LegentOfTheRowToRemove));
        }

        [TestMethod()]
        public void RemoveTest()
        {
        }

        [TestMethod()]
        public void GetRowAtTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetColumnAtTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRowsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddTest()
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
        public void GetLegendTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SetLegendTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetLabelTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SetLabelTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSeriesTest()
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
            string content = _df.ToCSV();
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "dataframe.csc"), content);
        }

        [TestMethod()]
        public void FromCSVTest()
        {
            Assert.Fail();
        }
    }
}
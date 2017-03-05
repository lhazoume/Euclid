using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Euclid.IndexedSeries.Tests
{
    [TestClass()]
    public class DataFrameTests
    {
        private DataFrame<DateTime, double, string> SampleCreatedUsingFullCOnstructor()
        {
            DataFrame<DateTime, double, string> result = DataFrame<DateTime, double, string>.Create(
                new string[] { "Col1", "Col2", "Col3" },
                new DateTime[] { DateTime.Today, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), DateTime.Today.AddDays(3) },
                new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } }
                );
            return result;
        }

        [TestMethod()]
        public void DataFrameFullConstructorTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            Assert.IsNotNull(dataFrame);
        }

        [TestMethod()]
        public void DataFrameTextConstructorTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            DataFrame<DateTime, double, string> altDataFrame = DataFrame<DateTime, double, string>.Create(dataFrame.ToCSV());
            Assert.IsNotNull(altDataFrame);
        }

        [TestMethod()]
        public void DataFrameTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CloneTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            Assert.AreEqual(dataFrame.ToCSV(), dataFrame.Clone().ToCSV());
        }

        [TestMethod()]
        public void RemoveColumnAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            dataFrame.RemoveColumnAt("Col1");
            Assert.IsTrue(!dataFrame.Labels.Contains("Col1") && dataFrame.Columns == 2);
        }

        [TestMethod()]
        public void RemoveRowAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            DateTime today = DateTime.Today;
            dataFrame.RemoveRowAt(today);
            Assert.IsTrue(!dataFrame.Legends.Contains(today) && dataFrame.Rows == 3);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSliceAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            Slice<DateTime, double, string> slice = dataFrame.GetSliceAt(DateTime.Today);
            Assert.IsTrue(slice.Legend == DateTime.Today && slice[0] == 1 && slice[1] == 2 && slice[2] == 3);
        }

        [TestMethod()]
        public void GetSeriesAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            Series<DateTime, double, string> series = dataFrame.GetSeriesAt("Col1");
            Assert.IsTrue(series.Label == "Col1" && series[0] == 1 && series[1] == 4 && series[2] == 7 && series[3] == 10);
        }

        [TestMethod()]
        public void GetRowsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            Slice<DateTime, double, string>[] slices = dataFrame.GetSlices();
            Assert.IsTrue(slices.Length == dataFrame.Rows &&
                slices.All(s => s.Columns == dataFrame.Columns) &&
                slices.All(s => s.Data.Max() - s.Data.Min() == 2) &&
                slices.All(s=> s.Labels.SequenceEqual(dataFrame.Labels)) &&
                slices[0].Legend == DateTime.Today);
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
        public void ToCSVTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullCOnstructor();
            Assert.AreNotEqual(dataFrame.ToCSV().Trim(), "");
        }
    }
}
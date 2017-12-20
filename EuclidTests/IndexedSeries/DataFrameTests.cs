using Euclid.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Xml;

namespace Euclid.IndexedSeries.Tests
{
    [TestClass()]
    public class DataFrameTests
    {
        private static string[] _headers = new string[] { "Col1", "Col2", "Col3" };
        private static DateTime[] _legends = new DateTime[] { DateTime.Today, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2),
            DateTime.Today.AddDays(3), DateTime.Today.AddDays(4), DateTime.Today.AddDays(5),
            DateTime.Today.AddDays(6), DateTime.Today.AddDays(7), DateTime.Today.AddDays(8),
            DateTime.Today.AddDays(9), DateTime.Today.AddDays(10), DateTime.Today.AddDays(11) };

        private static double[,] _data = new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 },
            { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 },
            { 19, 20, 21 }, { 22, 23, 24 }, { 25, 26, 27 },
            { 28, 29, 30 }, { 31, 32, 33 }, { 34, 35, 36 } };

        private DataFrame<DateTime, double, string> SampleCreatedUsingFullConstructor()
        {
            DataFrame<DateTime, double, string> result = DataFrame<DateTime, double, string>.Create(_headers, _legends, _data);
            return result;
        }

        #region Constructor
        [TestMethod()]
        public void DataFrameFullConstructorTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.IsNotNull(dataFrame);
        }

        [TestMethod()]
        public void DataFrameXmlConstructorTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(dataFrame.GetXml().Trim());
            DataFrame<DateTime, double, string> newDataFrame = DataFrame<DateTime, double, string>.Create(doc);
            Assert.IsTrue(dataFrame.Equals(newDataFrame));
        }

        [TestMethod()]
        public void DataFrameSeriesConstructorTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor(),
                seriesDataFrame = DataFrame<DateTime, double, string>.Create(dataFrame.GetSeries());
            Assert.IsTrue(dataFrame.Equals(seriesDataFrame));
        }

        [TestMethod()]
        public void DataFrameSlicesConstructorTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor(),
                slicesDataFrame = DataFrame<DateTime, double, string>.Create(dataFrame.GetSlices());
            Assert.IsTrue(dataFrame.Equals(slicesDataFrame));
        }

        [TestMethod()]
        public void DataFrameTextConstructorTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            DataFrame<DateTime, double, string> altDataFrame = DataFrame<DateTime, double, string>.Create(dataFrame.ToCSV());
            Assert.IsNotNull(altDataFrame);
        }
        #endregion

        #region Accessors
        [TestMethod()]
        public void LegendsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.AreEqual(dataFrame.Legends.Except(_legends).Count(), 0);
        }

        [TestMethod()]
        public void LabelsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.AreEqual(dataFrame.Labels.Except(_headers).Count(), 0);
        }

        [TestMethod()]
        public void CountRowsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.AreEqual(dataFrame.Rows, _legends.Length);
        }

        [TestMethod()]
        public void CountColsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.AreEqual(dataFrame.Columns, _headers.Length);
        }

        [TestMethod()]
        public void DataTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            bool areEqual = true;
            double[,] data = dataFrame.Data;
            for (int i = 0; i < dataFrame.Rows; i++)
                for (int j = 0; j < dataFrame.Columns; j++)
                    if (data[i, j] != _data[i, j])
                    {
                        areEqual = false;
                        break;
                    }
            Assert.IsTrue(areEqual);
        }

        [TestMethod()]
        public void IntegerIndexedAccessTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            bool areEqual = true;
            for (int i = 0; i < dataFrame.Rows; i++)
                for (int j = 0; j < dataFrame.Columns; j++)
                    if (dataFrame[i, j] != _data[i, j])
                    {
                        areEqual = false;
                        break;
                    }
            Assert.IsTrue(areEqual);
        }

        [TestMethod()]
        public void TemplateIndexedAccessTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            bool areEqual = true;
            double[,] data = dataFrame.Data;
            for (int i = 0; i < dataFrame.Rows; i++)
                for (int j = 0; j < dataFrame.Columns; j++)
                    if (dataFrame[_legends[i], _headers[j]] != _data[i, j])
                    {
                        areEqual = false;
                        break;
                    }
            Assert.IsTrue(areEqual);
        }
        #endregion

        #region Methods

        #region Series

        #region Get

        [TestMethod()]
        public void GetSeriesAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Series<DateTime, double, string> series = dataFrame.GetSeriesAt(_headers[0]);
            Assert.IsTrue(series.Label == _headers[0] && Enumerable.Range(0, dataFrame.Rows).All(i => series[i] == _data[i, 0]));
        }

        [TestMethod()]
        public void GetSeriesTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Series<DateTime, double, string>[] series = dataFrame.GetSeries();
            int[] rows = Enumerable.Range(0, dataFrame.Rows).ToArray();
            Assert.IsTrue(Enumerable.Range(0, _headers.Length).All(j => series[j].Label == _headers[j] && rows.All(i => series[j][i] == _data[i, j])));
        }

        #endregion

        #region Add
        [TestMethod()]
        public void AddSeriesWithDataTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            string newSeriesName = "Col4";
            double[] newSeriesData = new double[dataFrame.Rows];
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < newSeriesData.Length; i++)
                newSeriesData[i] = rnd.NextDouble();
            dataFrame.AddSeries(newSeriesName, newSeriesData);

            Series<DateTime, double, string> newSeries = dataFrame.GetSeriesAt(newSeriesName);
            Assert.IsTrue(newSeries != null && Enumerable.Range(0, dataFrame.Rows).All(i => newSeries[i] == newSeriesData[i]));
        }

        [TestMethod()]
        public void AddEmptySeriesTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            string newSeriesName = "Col4";
            dataFrame.AddSeries(newSeriesName);
            Assert.IsTrue(dataFrame.GetSeriesAt(newSeriesName) != null && dataFrame.GetSeriesAt(newSeriesName).Data.All(d => d == 0));
        }
        #endregion

        #region Take

        [TestMethod()]
        public void TakeSeriesTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            string takenSeriesName = "Col2";
            Series<DateTime, double, string> takenSeries = dataFrame.TakeSeries(takenSeriesName);

            Assert.IsTrue(takenSeries != null &&
                dataFrame.Columns == _headers.Length - 1 &&
                Enumerable.Range(0, dataFrame.Rows).All(i => takenSeries[i] == _data[i, 1]) &&
                Enumerable.Range(0, dataFrame.Rows).All(i => dataFrame[i, 0] == _data[i, 0] && dataFrame[i, 1] == _data[i, 2]));
        }

        #endregion

        #region Remove
        [TestMethod()]
        public void RemoveSeriesAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            dataFrame.RemoveSeriesAt("Col1");
            Assert.IsTrue(!dataFrame.Labels.Contains("Col1") &&
                dataFrame.Columns == 2 &&
                Enumerable.Range(0, dataFrame.Rows).All(i => dataFrame[i, 0] == _data[i, 1] && dataFrame[i, 1] == _data[i, 2]));
        }
        #endregion

        #region Extract
        [TestMethod()]
        public void ExtractByLabelsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor(),
                extraction = dataFrame.ExtractByLabels(s => !s.Contains("2"));
            Assert.IsTrue(extraction.Columns == 2 &&
                _headers.Except(extraction.Labels).Count() == 1 &&
                _headers.Except(extraction.Labels).Count(s => !s.Contains("2")) == 0 &&
                extraction.Rows == dataFrame.Rows);
        }
        #endregion

        #endregion

        #region Slices

        #region Get
        [TestMethod()]
        public void GetSliceAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Slice<DateTime, double, string> slice = dataFrame.GetSliceAt(_legends[0]);
            Assert.IsTrue(slice.Legend == _legends[0] && Enumerable.Range(0, dataFrame.Columns).All(i => slice[i] == _data[0, i]));
        }

        [TestMethod()]
        public void GetSlicesTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Slice<DateTime, double, string>[] slices = dataFrame.GetSlices();
            int[] cols = Enumerable.Range(0, dataFrame.Columns).ToArray();
            Assert.IsTrue(Enumerable.Range(0, _legends.Length).All(i => slices[i].Legend == _legends[i] && cols.All(j => slices[i][j] == _data[i, j])));
        }

        #endregion

        #region Add

        [TestMethod()]
        public void AddSlicesWithDataTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            DateTime newSliceDate = new DateTime(1900, 7, 13);
            double[] newSliceData = new double[dataFrame.Columns];
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < newSliceData.Length; i++)
                newSliceData[i] = rnd.NextDouble();
            dataFrame.AddSlice(newSliceDate, newSliceData);

            Slice<DateTime, double, string> newSlice = dataFrame.GetSliceAt(newSliceDate);
            Assert.IsTrue(newSlice != null && Enumerable.Range(0, dataFrame.Columns).All(j => newSlice[j] == newSliceData[j]));
        }

        [TestMethod()]
        public void AddEmptySlicesTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            DateTime newSliceDate = new DateTime(1900, 7, 13); ;
            dataFrame.AddSlice(newSliceDate);
            Assert.IsTrue(dataFrame.GetSliceAt(newSliceDate) != null && dataFrame.GetSliceAt(newSliceDate).Data.All(d => d == 0));
        }
        #endregion

        #region Take
        [TestMethod()]
        public void TakeSliceTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            DateTime takenSliceDate = _legends[2];
            Slice<DateTime, double, string> takenSlice = dataFrame.TakeSlice(takenSliceDate);

            Assert.IsTrue(takenSlice != null &&
                dataFrame.Rows == _legends.Length - 1 &&
                Enumerable.Range(0, dataFrame.Columns).All(j => takenSlice[j] == _data[2, j]));
        }
        #endregion

        #region Remove
        [TestMethod()]
        public void RemoveSliceAtTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            DateTime removedSliceDate = _legends[2];
            bool sliceRemoved = dataFrame.RemoveSliceAt(removedSliceDate);

            Assert.IsTrue(sliceRemoved &&
                dataFrame.Rows == _legends.Length - 1 &&
                dataFrame.GetLegend(2) != _legends[2]);
        }
        #endregion

        #region Extract
        [TestMethod()]
        public void ExtractByLegendTest()
        {
            Func<DateTime, bool> predicate = d => d.DayOfWeek == DayOfWeek.Monday;

            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor(),
                extraction = dataFrame.ExtractByLegend(predicate);

            int mondays = _legends.Count(predicate);

            Assert.IsTrue(extraction.Rows == mondays &&
                _legends.Except(extraction.Legends).Count() == _legends.Length - mondays &&
                extraction.Columns == dataFrame.Columns);
        }
        #endregion

        #endregion

        #endregion

        [TestMethod()]
        public void CloneTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor(),
                clone = dataFrame.Clone();

            Assert.IsTrue(dataFrame.Equals(clone));
        }

        #region Apply
        [TestMethod()]
        public void ApplyOnDataTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            dataFrame.ApplyOnData(d => d * d);

            bool areEqual = true;
            for (int i = 0; i < dataFrame.Rows; i++)
                for (int j = 0; j < dataFrame.Columns; j++)
                    if (dataFrame[i, j] != _data[i, j] * _data[i, j])
                    {
                        areEqual = false;
                        break;
                    }
            Assert.IsTrue(areEqual);
        }

        [TestMethod()]
        public void ApplyOnLegendsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            dataFrame.ApplyOnLegends(d => d.AddMonths(1));
            Assert.IsTrue(Enumerable.Range(0, dataFrame.Rows).All(i => dataFrame.GetLegend(i) == _legends[i].AddMonths(1)));
        }

        [TestMethod()]
        public void ApplyOnLabelsTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            dataFrame.ApplyOnLabels(d => d + "_");
            Assert.IsTrue(Enumerable.Range(0, dataFrame.Columns).All(j => dataFrame.GetLabel(j) == _headers[j] + "_"));
        }
        #endregion

        #region Access the legends and labels
        [TestMethod()]
        public void GetLegendTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.IsTrue(Enumerable.Range(0, dataFrame.Rows).All(i => dataFrame.GetLegend(i) == _legends[i]));
        }

        [TestMethod()]
        public void RenameLegendTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            DateTime newLegend = new DateTime(1900, 7, 13);
            dataFrame.RenameLegend(_legends[0], newLegend);
            Assert.IsTrue(dataFrame.Legends.Contains(newLegend) && !dataFrame.Legends.Contains(_legends[0]));
        }

        [TestMethod()]
        public void GetLabelTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.IsTrue(Enumerable.Range(0, dataFrame.Columns).All(j => dataFrame.GetLabel(j) == _headers[j]));
        }

        [TestMethod()]
        public void RenameLabelTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            dataFrame.RenameLabel(_headers[0], "New");
            Assert.IsTrue(dataFrame.Labels.Contains("New") && !dataFrame.Labels.Contains(_headers[0]));
        }
        #endregion

        #region IXmlable
        [TestMethod()]
        public void ToXmlTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.AreNotEqual(dataFrame.GetXml().Trim(), "");
        }
        #endregion

        #region ICSVable
        [TestMethod()]
        public void ToCSVTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor();
            Assert.AreNotEqual(dataFrame.ToCSV().Trim(), "");
        }
        #endregion

        [TestMethod()]
        public void MatchesTest()
        {
            DataFrame<DateTime, double, string> dataFrame = SampleCreatedUsingFullConstructor(),
                trueClone = SampleCreatedUsingFullConstructor(),
                falseClone = SampleCreatedUsingFullConstructor();
            falseClone[1, 1] = 1000;

            Assert.IsTrue(dataFrame.Equals(trueClone) && !dataFrame.Equals(falseClone));
        }
    }
}
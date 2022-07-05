using Euclid.DataStructures.IndexedSeries;
using Euclid.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries.Tests
{
    [TestClass()]
    public class SeriesTests
    {
        private static readonly int _n = 100;
        private static readonly DateTime[] _dates = Enumerable.Range(0, _n).Select(i => new DateTime(2000 + i, 6, 1)).ToArray();
        private static readonly double[] _values = Enumerable.Range(0, _n).Select(i => Convert.ToDouble(i)).ToArray();

        private static Series<DateTime, double, string> BaseSeries(double value)
        {
            return Series<DateTime, double, string>.Create<Series<DateTime, double, string>>("Titre", new Header<DateTime>(_dates), Enumerable.Range(0, _dates.Length).Select(i => value));
        }

        private static Series<DateTime, double, string> ValuesSeries()
        {
            return Series<DateTime, double, string>.Create<Series<DateTime, double, string>>("Titre", new Header<DateTime>(_dates), _values);
        }

        #region Accessors
        [TestMethod()]
        public void LegendsTest()
        {
            Series<DateTime, double, string> series = BaseSeries(3);
            Assert.IsTrue(series.Rows == _n && series.Legends.Except(_dates).Count() == 0);
        }

        [TestMethod()]
        public void LabelsTest()
        {
            Series<DateTime, double, string> series = BaseSeries(3);
            Assert.IsTrue(series.Labels[0] == "Titre");
        }

        [TestMethod()]
        public void DataTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            double[] data = series.Data;
            Assert.IsTrue(data.Length == _values.Length && data.Except(_values).Count() == 0);
        }

        [TestMethod()]
        public void LabelTest()
        {
            Series<DateTime, double, string> series = BaseSeries(3);
            Assert.IsTrue(series.Label == "Titre");
        }

        [TestMethod()]
        public void ColumnsTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            Assert.IsTrue(series.Columns == 1);
        }

        [TestMethod()]
        public void RowsTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            Assert.IsTrue(series.Rows == _n);
        }
        #endregion

        #region Methods
        [TestMethod()]
        public void CloneTest()
        {
            Series<DateTime, double, string> series = ValuesSeries(),
                clone = series.Clone();
            Assert.IsTrue(series.Equals(clone));
        }

        [TestMethod()]
        public void AccessByIndexTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => series[i] == _values[i]));
        }

        [TestMethod()]
        public void AccessByLegendTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => series[series.Legends[i]] == _values[i]));
        }

        [TestMethod()]
        public void RemoveRowAtTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            int rowToRemove = rnd.Next(_n);
            DateTime dateToRemove = _dates[rowToRemove];
            series.RemoveRowAt(dateToRemove);
            Assert.IsTrue(series.Rows == _n - 1 && !series.Legends.Contains(dateToRemove) && !series.Data.Contains(_values[rowToRemove]));
        }

        [TestMethod()]
        public void AddTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            DateTime dateToAdd = new DateTime(1900, 12, 23);
            series.Add(dateToAdd, -100);
            Assert.IsTrue(series.Rows == _n + 1 && series.Legends.Contains(dateToAdd) && series.Data.Contains(-100));
        }

        [TestMethod()]
        public void ApplyOnDataTest()
        {
            Series<DateTime, double, string> series = BaseSeries(3);
            Func<double, double> transformer = d => d + 1;
            series.ApplyOnData(transformer);
            Assert.IsTrue(series.Rows == _n && Enumerable.Range(0, _n).All(i => series[i] == 4));
        }

        [TestMethod()]
        public void ApplyOnLegendsTest()
        {
            Series<DateTime, double, string> series = BaseSeries(3);
            Func<DateTime, DateTime> transformer = d => d.AddYears(1);
            series.ApplyOnLegends(transformer);
            Assert.IsTrue(series.Rows == _n && Enumerable.Range(0, _n).All(i => series.GetLegend(i) == _dates[i].AddYears(1)));
        }

        [TestMethod()]
        public void SumTest()
        {
            Series<DateTime, double, string> series = BaseSeries(3);
            Func<double, double> transformer = d => d + 1;
            double sumResult = series.Sum(transformer);
            Assert.IsTrue(sumResult == 4 * _n);
        }

        [TestMethod()]
        public void GetLegendTest()
        {
            Series<DateTime, double, string> series = BaseSeries(3);
            Assert.IsTrue(series.Rows == _n && Enumerable.Range(0, _n).All(i => series.GetLegend(i) == _dates[i]));
        }

        [TestMethod()]
        public void RenameTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            DateTime olDateTime = _dates[_n / 2],
                newDateTime = new DateTime(1900, 7, 15);
            series.Rename(olDateTime, newDateTime);
            Assert.IsTrue(series.Rows == _n && series.Legends.Contains(newDateTime) && !series.Legends.Contains(olDateTime));
        }

        [TestMethod()]
        public void MatchesTest()
        {
            Series<DateTime, double, string> series = ValuesSeries(),
                expectedMatch = ValuesSeries(),
                expectedUnMatch = ValuesSeries();
            expectedUnMatch[_n / 2] = 10000000;
            Assert.IsTrue(expectedMatch.Equals(series) && !expectedUnMatch.Equals(series));
        }

        [TestMethod()]
        public void RemoveTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            int rows = series.Remove((dt, d) => dt.DayOfWeek == DayOfWeek.Monday || d > 50);

            Assert.IsTrue(series.Data.Count(d => d > 50) == 0 &&
                series.Legends.Count(dt => dt.DayOfWeek == DayOfWeek.Monday) == 0);
        }
        #endregion

        #region IXmlable
        [TestMethod()]
        public void ToXmlTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            Assert.AreNotEqual(series.GetXml().Trim(), "");
        }
        #endregion

        #region ICSVable
        [TestMethod()]
        public void ToCSVTest()
        {
            Series<DateTime, double, string> series = ValuesSeries();
            Assert.AreNotEqual(series.ToCSV().Trim(), "");
        }

        #endregion

        #region Operators
        [TestMethod()]
        public void OperatorAddSeriesTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(1),
                ts2 = BaseSeries(2),
                tsSum = ts1 + ts2;
            Assert.IsTrue(tsSum.Rows == _n && tsSum.Data.All(d => d == 3));
        }

        [TestMethod()]
        public void OperatorSubstractSeriesTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(1),
                ts2 = BaseSeries(2),
                tsDiff = ts2 - ts1;
            Assert.IsTrue(tsDiff.Rows == _n && tsDiff.Data.All(d => d == 1));
        }

        [TestMethod()]
        public void OperatorMultiplySeriesByNumLeftTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(4),
                tsResult = ts1 * 3;
            Assert.IsTrue(tsResult.Rows == _n && tsResult.Data.All(d => d == 12));
        }

        [TestMethod()]
        public void OperatorMultiplySeriesByNumRightTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(4),
                tsResult = 3 * ts1;
            Assert.IsTrue(tsResult.Rows == _n && tsResult.Data.All(d => d == 12));
        }

        [TestMethod()]
        public void OperatorDivideSeriesByNumTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(4),
                tsResult = ts1 / 2;
            Assert.IsTrue(tsResult.Rows == _n && tsResult.Data.All(d => d == 2));

        }

        [TestMethod()]
        public void OperatorAddSeriesToNumRightTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(4),
                tsResult = ts1 + 3;
            Assert.IsTrue(tsResult.Rows == _n && tsResult.Data.All(d => d == 7));
        }

        [TestMethod()]
        public void OperatorAddSeriesToNumLeftTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(4),
                tsResult = 3 + ts1;
            Assert.IsTrue(tsResult.Rows == _n && tsResult.Data.All(d => d == 7));
        }

        [TestMethod()]
        public void OperatorSubstractSeriesToNumRightTest()
        {
            Series<DateTime, double, string> ts1 = BaseSeries(4),
                tsResult = ts1 - 3;
            Assert.IsTrue(tsResult.Rows == _n && tsResult.Data.All(d => d == 1));
        }
        #endregion

        #region Creators
        [TestMethod()]
        public void CreateFromRawDataTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create<Series<DateTime, double, string>>("Label", new Header<DateTime>(_dates), _values);
            Assert.IsTrue(series != null && series.Rows == _n && series.Label == "Label" &&
                series.Legends.Except(_dates).Count() == 0 &&
                series.Data.Except(_values).Count() == 0);
        }

        [TestMethod()]
        public void CreateFromXmlTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create<Series<DateTime, double, string>>("Label", new Header<DateTime>(_dates), _values);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(series.GetXml().Trim());
            Series<DateTime, double, string> newSeries = Series<DateTime, double, string>.Create<Series<DateTime, double, string>>(doc);
            Assert.IsTrue(series.Equals(newSeries));
        }

        [TestMethod()]
        public void CreateFromCSVTest()
        {
            Series<DateTime, double, string> series = Series<DateTime, double, string>.Create<Series<DateTime, double, string>>("Label", new Header<DateTime>(_dates), _values),
                pseudoClone = Series<DateTime, double, string>.Create<Series<DateTime, double, string>>(series.ToCSV());
            Assert.IsTrue(series.Equals(pseudoClone));
        }
        #endregion
    }
}
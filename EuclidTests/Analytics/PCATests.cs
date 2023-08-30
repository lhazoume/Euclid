using Euclid.Analytics.Clustering;
using Euclid.DataStructures.IndexedSeries;
using Euclid.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EuclidTests.Analystics
{
    [TestClass()]
    public class PCATests
    {
        #region vars
        public double[][] _data;
        public IList<int> _labels, _legends;
        #endregion

        [TestInitialize]
        public void Initialize()
        {
            string[] lines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Analytics", "data-pca.txt"));

            int N = lines.Length - 1, M = Regex.Split(lines[0], ",").Length;
            _data = Arrays.Build<double>(N, M);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = Regex.Split(lines[i], ",");
                for (int j = 0; j < M; j++) _data[i-1][j] = Convert.ToDouble(columns[j]);
            }

            _labels = Enumerable.Range(0, N).ToList();
            _legends = Enumerable.Range(0, M).ToList();
        }

        #region methods
        [TestMethod()]
        public void FitPCATest()
        {
            PCA<int, int> pca = PCA<int, int>.Create(DataFrame<int, double, int>.Create<DataFrame<int, double, int>>(_labels, _legends, _data));
            pca.Fit();
            Assert.AreEqual(pca.Status, Euclid.Analytics.Regressions.RegressionStatus.Normal);
        }
        #endregion
    }
}

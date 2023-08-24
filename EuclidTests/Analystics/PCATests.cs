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
                for (int j = 0; j < M; j++) _data[i][j] = Convert.ToDouble(columns[j]);
            }
        }

        #region methods
        [TestMethod()]
        public void FitPCATest()
        {


            //Assert.AreEqual(cumulatedNorm / (n * n), 0, 1e-9, "The EigenDecomposition does not behave as expected");
        }
        #endregion
    }
}

using Euclid.Analytics;
using Euclid.Analytics.Regressions;
using Euclid.DataStructures.IndexedSeries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Euclid.IndexedSeries.Analytics.Regressions.Tests
{
    [TestClass()]
    public class RIDGERegressionTests
    {
        [TestMethod()]
        public void RIDGERegressionTest()
        {
            int n = 100;
            int[] indexes = new int[n];
            for (int i = 0; i < n; i++) indexes[i] = i;

            double[,] data = Matrix.CreateRandom(n, 1).Array;

            DataFrame<int, double, string> x = DataFrame<int, double, string>.Create<DataFrame<int, double, string>>(new string[] { "V0" }, indexes, data);
            for (int v = 1; v <= 2; v++)
                x.AddSeries("V" + v, Matrix.CreateRandom(n, 1).Data);

            double[] factors = new double[] { Math.PI, Math.E, Math.Log(2) };

            Series<int, double, string> y = (factors[0] * x.GetSeriesAt("V0")) + (factors[1] * x.GetSeriesAt("V1")) + (factors[2] * x.GetSeriesAt("V2"));
            y.Label = "Y";

            RIDGERegression<int, string> ridge = new RIDGERegression<int, string>(x, y, 0.5);

            Assert.IsTrue(ridge.Regularization == 0.5 && ridge.ComputeError && ridge.WithConstant && !ridge.ReturnAverageIfFailed && ridge.Status == RegressionStatus.NotRan);
        }

        [TestMethod()]
        public void RegressTest()
        {
            int n = 100;
            int[] indexes = new int[n];
            for (int i = 0; i < n; i++) indexes[i] = i;

            double[,] data = Matrix.CreateRandom(n, 1).Array;

            DataFrame<int, double, string> x = DataFrame<int, double, string>.Create<DataFrame<int, double, string>>(new string[] { "V0" }, indexes, data);
            for (int v = 1; v <= 2; v++)
            {
                Thread.Sleep(37);
                x.AddSeries("V" + v, Matrix.CreateRandom(n, 1).Data);
            }

            double[] factors = new double[] { Math.PI, Math.E, Math.Log(2) };

            Series<int, double, string> y = (factors[0] * x.GetSeriesAt("V0")) + (factors[1] * x.GetSeriesAt("V1")) + (factors[2] * x.GetSeriesAt("V2"));
            y.Label = "Y";

            RIDGERegression<int, string> ridge = new RIDGERegression<int, string>(x, y, 100.0);
            ridge.Regress();
            LinearModel lm = ridge.LinearModel;

            Assert.IsTrue(ridge.Status == RegressionStatus.Normal && lm.Succeeded);
        }
    }
}
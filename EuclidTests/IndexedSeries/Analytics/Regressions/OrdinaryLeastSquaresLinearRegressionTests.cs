using Euclid.Analytics;
using Euclid.Analytics.Regressions;
using Euclid.DataStructures.IndexedSeries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Euclid.IndexedSeries.Analytics.Regressions.Tests
{
    [TestClass()]
    public class OrdinaryLeastSquaresLinearRegressionTests
    {
        [TestMethod()]
        public void OrdinaryLeastSquaresLinearRegressionTest()
        {
            int n = 100;
            int[] indexes = new int[n];
            for (int i = 0; i < n; i++) indexes[i] = i;

            double[][] data = Matrix.CreateRandom(n, 1).JaggedArray;

            DataFrame<int, double, string> x = DataFrame<int, double, string>.Create<DataFrame<int, double, string>>(new string[] { "V0" }, indexes, data);
            for (int v = 1; v <= 9; v++)
                x.AddSeries("V" + v, Matrix.CreateRandom(n, 1).Data);

            double[] factors = new double[] { Math.PI, Math.E, Math.Log(2) };

            Series<int, double, string> y = (factors[0] * x.GetSeriesAt("V0")) + (factors[1] * x.GetSeriesAt("V1")) + (factors[2] * x.GetSeriesAt("V2"));
            y.Label = "Y";

            OrdinaryLeastSquaresLinearRegression<int, string> ols = new OrdinaryLeastSquaresLinearRegression<int, string>(x.Data, y.Data);
            Assert.IsTrue(ols.ComputeError && ols.WithConstant && !ols.ReturnAverageIfFailed && ols.Status == RegressionStatus.NotRan);
        }

        [TestMethod()]
        public void RegressTest()
        {
            int n = 100;
            int[] indexes = new int[n];
            for (int i = 0; i < n; i++) indexes[i] = i;

            double[][] data = Matrix.CreateRandom(n, 1).JaggedArray;

            DataFrame<int, double, string> x = DataFrame<int, double, string>.Create<DataFrame<int, double, string>>(new string[] { "V0" }, indexes, data);
            for (int v = 1; v <= 2; v++)
            {
                Thread.Sleep(37);
                x.AddSeries("V" + v, Matrix.CreateRandom(n, 1).Data);
            }

            double[] factors = new double[] { Math.PI, Math.E, Math.Log(2) };

            Series<int, double, string> y = (factors[0] * x.GetSeriesAt("V0")) + (factors[1] * x.GetSeriesAt("V1")) + (factors[2] * x.GetSeriesAt("V2"));
            y.Label = "Y";

            OrdinaryLeastSquaresLinearRegression<int, string> ols = new OrdinaryLeastSquaresLinearRegression<int, string>(x.Data, y.Data);
            ols.Regress();
            LinearModel lm = ols.LinearModel;

            double sumSq = 0;
            for (int i = 0; i <= 2; i++)
                sumSq += Math.Abs(lm.Factors[i] - factors[i]);

            Assert.IsTrue(ols.Status == RegressionStatus.Normal && lm.Succeeded && sumSq < 1e-10);
        }

        [TestMethod()]
        public void RegressTestWithASupplementaryColumn()
        {
            int n = 100;
            int[] indexes = new int[n];
            for (int i = 0; i < n; i++) indexes[i] = i;

            double[][] data = Matrix.CreateRandom(n, 1).JaggedArray;

            DataFrame<int, double, string> x = DataFrame<int, double, string>.Create<DataFrame<int, double, string>>(new string[] { "V0" }, indexes, data);
            for (int v = 1; v <= 3; v++)
            {
                Thread.Sleep(37);
                x.AddSeries("V" + v, Matrix.CreateRandom(n, 1).Data);
            }

            double[] factors = new double[] { Math.PI, Math.E, Math.Log(2) };

            Series<int, double, string> y = (factors[0] * x.GetSeriesAt("V0")) + (factors[1] * x.GetSeriesAt("V1")) + (factors[2] * x.GetSeriesAt("V2"));
            y.Label = "Y";

            OrdinaryLeastSquaresLinearRegression<int, string> ols = new OrdinaryLeastSquaresLinearRegression<int, string>(x.Data, y.Data);
            ols.Regress();
            LinearModel lm = ols.LinearModel;

            double sumSq = 0;
            for (int i = 0; i <= 2; i++)
                sumSq += Math.Abs(lm.Factors[i] - factors[i]);

            Assert.IsTrue(ols.Status == RegressionStatus.Normal && lm.Succeeded && sumSq < 1e-10 && Math.Abs(lm.Factors[3]) < 1e-10);
        }
    }
}
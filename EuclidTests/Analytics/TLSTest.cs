using Euclid;
using Euclid.Analytics.Regressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuclidTests.Analytics
{
    [TestClass()]
    public class TLSTest
    {
        #region vars
        private Matrix X;
        private double Beta = 0.26260119949259825;
        #endregion

        #region methods
        [TestInitialize()]
        public void Intialize()
        {
            double[][] data = new double[2][];
            data[0] = new double[] { 0, 0.04, 0, -0.06, -0.02, 0, -0.02, 0.04, -0.06, 0.02, -0.04, 0, 0.04, 0, -0.04, 0, -0.04, -0.02, 0 };
            data[1] = new double[] { 0, 0, 0.01, -0.03, 0, 0, -0.01, 0, 0, -0.01, -0.01, 0.01, 0.03, 0, -0.01, -0.01, -0.01, -0.01, -0.01 };
            X = Matrix.Create(data).Transpose;
        }

        [TestMethod()]
        public void Regress()
        {
            TotalLeastSquaresRegression tls = TotalLeastSquaresRegression.Create(X, true, true);
            tls.Regress();
            Assert.AreEqual(tls.Status, RegressionStatus.Normal);
            Assert.AreEqual(Beta, tls.Beta[0]);
        }

        #endregion
    }
}

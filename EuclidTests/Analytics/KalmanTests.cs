using Euclid;
using Euclid.Analytics.Filtering;
using Euclid.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EuclidTests.Analytics
{
    [TestClass()]
    public class KalmanTests
    {
        #region vars
        public Vector Y;
        public Matrix H, F, X = Matrix.Create(new[,] { { 1.02282954 }, { 1.00166182 } });
        public double delta = 0.001;
        #endregion

        #region initialization
        [TestInitialize]
        public void Initialization()
        {
            string[] lines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Analytics", "data-kalman.txt"));

            int N = lines.Length - 1, M = Regex.Split(lines[0], ";").Length;
            Y = Vector.Create(N);
            H = Matrix.Create(N, M);
            
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = Regex.Split(lines[i], ";");
                H[i - 1, 0] = Convert.ToDouble(columns[0]);
                H[i - 1, 1] = 1;
                Y[i - 1] = Convert.ToDouble(columns[1]);
            }

            F = Matrix.CreateIdentityMatrix(M, M);
        }
        #endregion

        #region methods
        [TestMethod()]
        public void Filtering()
        {
            int m = F.Columns;
            Matrix Q = Matrix.CreateIdentityMatrix(m, m) * delta;
            Matrix R = Matrix.Create(1, 1, delta);
            Matrix x0 = Matrix.Create(m, 1, 1);
            Matrix P = Matrix.Create(m, m, 1);

            KalmanFilter kf = KalmanFilter.Create(Y, F, H, null, Q, R, P, x0);
            kf.Filter();

            Assert.IsTrue(Matrix.AllClose(kf.X, X));
        }
        #endregion
    }
}

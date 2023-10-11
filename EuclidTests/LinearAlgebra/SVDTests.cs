using Euclid;
using Euclid.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuclidTests.LinearAlgebra
{
    [TestClass()]
    public class SVDTests
    {
        #region vars
        private Matrix A_jac, U_jac, V_jac;
        private Matrix A_pi, U_pi;
        private Vector D_jac;
        private Vector D_pi;
        #endregion

        #region methods
        [TestInitialize()]
        public void Initialize()
        {
            double[][] data = new double[2][];
            data[0] = new double[] { 0, 0.04, 0, -0.06, -0.02, 0, -0.02, 0.04, -0.06, 0.02, -0.04, 0, 0.04, 0, -0.04, 0, -0.04, -0.02, 0 };
            data[1] = new double[] { 0, 0, 0.01, -0.03, 0, 0, -0.01, 0, 0, -0.01, -0.01, 0.01, 0.03, 0, -0.01, -0.01, -0.01, -0.01, -0.01 };
            A_pi = Matrix.Create(data);


            data = new double[3][];
            data[0] = new double[] { -8, 10, 14 };
            data[1] = new double[] { 2, 2, 1 };
            data[2] = new double[] { -6, -6, -3 };
            //data[3] = new double[] { -16, 2, 10 };

            A_jac = Matrix.Create(data);

            double[][] u = new double[3][];
            u[0] = new double[] { 0.98016393405667757, -0.19818845166794324, -2.8317279029799479E-17 };
            u[1] = new double[] { 0.062672691321289692, 0.30995505119701844, 3 };
            u[2] = new double[] { -0.18801807396386905, -0.92986515359105526, 1 };
            //u[3] = new double[] { -0.7071, -0.4714, 1, 1 };
            U_jac = Matrix.Create(u);

            u = new double[2][];
            u[0] = new double[] { 0.96720692364849892, -0.25398969830764179 };
            u[1] = new double[] { 0.2539896983076419, 0.96720692364849892 };
            U_pi = Matrix.Create(u);

            double[][] v = new double[3][];
            v[0] = new double[] { -0.34178996786477472, 0.87867428934496838, -0.33333333333333348 };
            v[1] = new double[] { 0.57355822504253218, 0.47601104823482326, 0.66666666666666652 };
            v[2] = new double[] { 0.74445320897492007, 0.03667390356233894, -0.66666666666666674 };
            //u[3] = new double[] { -0.7071, -0.4714, 1, 1 };
            V_jac = Matrix.Create(v);

            //double[] d = new double[] { 19.274578733785528, 8.8594929107202667, 4.182607133324845E-08 };
            //d[0] = new double[] { 19.274578733785528 };
            //d[1] = new double[] { 8.8594929107202667 };
            //d[2] = new double[] { 4.182607133324845E-08 };
            //u[3] = new double[] { -0.7071, -0.4714, 1, 1 };
            D_jac = Vector.Create(19.274578733785528, 8.8594929107202667, 4.182607133324845E-08);
            D_pi = Vector.Create(0.13984078543434761, 0.040553109980675413);
        }

        [TestMethod()]
        public void SVDByJacobiTest()
        {
            SingularValueDecomposition svd = SingularValueDecomposition.Run(A_jac, SVDType.JACOBI);
            Assert.IsTrue(svd.U.Equals(U_jac));
            Assert.IsTrue(svd.D.Equals(D_jac));
            Assert.IsTrue(svd.V.Equals(V_jac));
        }

        [TestMethod()]
        public void SVDByPowerIterationTest()
        {
            SingularValueDecomposition svd = SingularValueDecomposition.Run(A_pi, SVDType.POWER_ITERATION);
            Assert.IsTrue(svd.U.Equals(U_pi));
            Assert.IsTrue(svd.D.Equals(D_pi));
        }
        #endregion
    }
}

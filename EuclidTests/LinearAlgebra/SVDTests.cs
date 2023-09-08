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
        public Matrix A, U, V, D;
        #endregion

        #region methods
        [TestInitialize()]
        public void Initialize()
        {
            double[][] data = new double[3][];
            data[0] = new double[] { -8, 10, 14 };
            data[1] = new double[] { 2, 2, 1 };
            data[2] = new double[] { -6, -6, -3 };
            //data[3] = new double[] { -16, 2, 10 };

            A = Matrix.Create(data);

            double[][] u = new double[3][];
            u[0] = new double[] { 0.98016393405667757, -0.19818845166794324, -2.8317279029799479E-17 };
            u[1] = new double[] { 0.062672691321289692, 0.30995505119701844, 3 };
            u[2] = new double[] { -0.18801807396386905, -0.92986515359105526, 1 };
            //u[3] = new double[] { -0.7071, -0.4714, 1, 1 };
            U = Matrix.Create(u);

            double[][] v = new double[3][];
            v[0] = new double[] { -0.34178996786477472, 0.87867428934496838, -0.33333333333333348 };
            v[1] = new double[] { 0.57355822504253218, 0.47601104823482326, 0.66666666666666652 };
            v[2] = new double[] { 0.74445320897492007, 0.03667390356233894, -0.66666666666666674 };
            //u[3] = new double[] { -0.7071, -0.4714, 1, 1 };
            V = Matrix.Create(v);

            double[][] d = new double[3][];
            d[0] = new double[] { 19.274578733785528 };
            d[1] = new double[] { 8.8594929107202667 };
            d[2] = new double[] { 4.182607133324845E-08 };
            //u[3] = new double[] { -0.7071, -0.4714, 1, 1 };
            D = Matrix.Create(d);
        }

        [TestMethod()]
        public void SVDByJacobiTest()
        {
            SingularValueDecomposition svd = SingularValueDecomposition.Run(A, SVDType.JACOBI);
            Assert.IsTrue(svd.U.Equals(U));
            Assert.IsTrue(svd.D.Equals(D));
            Assert.IsTrue(svd.V.Equals(V));
        }
        #endregion


    }
}

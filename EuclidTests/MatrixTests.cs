using Euclid;
using Euclid.Distributions.Continuous;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Euclid.Tests
{
    [TestClass()]
    public class MatrixTests
    {
        #region Constructors

        [TestMethod()]
        public void CreateTest()
        {
            Matrix m = Matrix.Create();
            Assert.AreEqual(m.Size, 4, "Failed on the argument less matrix creation");
        }

        [TestMethod()]
        public void CreateSquareTest()
        {
            int n = 10;
            Matrix m = Matrix.CreateSquare(n);
            Assert.IsTrue(m.Rows == m.Columns && m.Rows == n, "Failed to build a square matrix");
        }

        [TestMethod()]
        public void CreateTest2()
        {
            int r = 5, c = 7;
            Matrix m = Matrix.Create(r, c);
            Assert.IsTrue(m.Rows == r && m.Columns == c, "Failed to build a rectangular matrix");
        }

        [TestMethod()]
        public void CreateTest3()
        {
            int r = 5, c = 7;
            double d = Math.PI;
            Matrix m = Matrix.Create(r, c, d);
            Assert.IsTrue(m.Rows == r && m.Columns == c && m.Data.All(v => v == d), "Failed to build a rectangular full matrix");
        }

        [TestMethod()]
        public void CreateTest4()
        {
            double[,] data = new double[2, 2];
            data[0, 0] = 1;
            data[1, 1] = 3;
            data[0, 1] = 2;
            data[1, 0] = 2;
            Matrix m = Matrix.Create(data);
            Assert.AreEqual(data[1, 1], m.Data[3]);
        }

        [TestMethod()]
        public void CreateZeroMatrixTest()
        {
            int rows = 5,
                cols = 8;
            Matrix z = Matrix.CreateZeroMatrix(rows, cols);

            bool succeeded = true;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (z[i, j] != 0)
                        succeeded = false;
            Assert.IsTrue(succeeded, "The zero matrix doesnot contain only zeros");
        }

        [TestMethod()]
        public void CreateIdentityMatrixTest()
        {
            int rows = 4,
                cols = rows + 1;
            Matrix m = Matrix.CreateIdentityMatrix(rows, cols),
                control = Matrix.Create(rows, cols);
            for (int i = 0; i < rows; i++)
                control[i, i] = 1;
            Assert.AreEqual((control - m).Norm1, 0, "The Identity Matrix does not match the expected");
        }

        [TestMethod()]
        public void CreateRandomTest()
        {
            Matrix testRnd = Matrix.CreateRandom(10, 5);
            Assert.IsTrue(testRnd.Data.All(d => d >= 0 && d <= 1));
        }

        [TestMethod()]
        public void CreateSquareRandomTest()
        {
            Matrix testRnd = Matrix.CreateSquareRandom(10);
            Assert.IsTrue(testRnd.Data.All(d => d >= 0 && d <= 1));
        }

        [TestMethod()]
        public void CreateBandMatrixTest()
        {
            int size = 10;
            Matrix m = Matrix.CreateBandMatrix(size, 1, 2, 3, 4),
                mRef = Matrix.CreateSquare(size);
            for (int i = 0; i < size; i++)
            {
                mRef[i, i] = 1;
                for (int j = i + 1; j < size; j++)
                {
                    mRef[i, j] = Math.Abs(i - j) <= 3 ? Math.Abs(i - j) + 1 : 0;
                    mRef[j, i] = mRef[i, j];
                }
            }
            Assert.AreEqual(0, (mRef - m).Norm2, 1e-10);
        }

        #endregion

        [TestMethod()]
        public void SolveWithTest()
        {
            int dimension = 5;
            Matrix a = Matrix.CreateSquare(dimension);
            for (int i = 0; i < dimension; i++)
                for (int j = 0; j <= i; j++)
                    a[i, j] = 1;
            Vector b = Vector.CreateRandom(dimension, new UniformDistribution(0, 1));
            Vector x = a.SolveWith(b);
            Assert.AreEqual((a * x - b).Norm1, 0, 1e-9, "The Solve with does not match the expected result");
        }

        #region Rows and columns

        [TestMethod()]
        public void SetColTest()
        {
            int targetColumn = 3,
                dimension = 5;
            Matrix m = Matrix.CreateSquare(dimension);
            Vector newCol = Vector.Create(dimension);

            for (int i = 0; i < dimension; i++)
                newCol[i] = i;
            m.SetCol(newCol, targetColumn);

            bool fit = true;
            for (int i = 0; i < dimension; i++)
                if (newCol[i] != m[i, targetColumn])
                    fit = false;
            Assert.IsTrue(fit, "The method 'SetCol' failed : the values do not match");
        }

        [TestMethod()]
        public void ColumnTest()
        {
            int size = 3;
            Matrix m = Matrix.Create(size, size, 0);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    m[i, j] = i * j;
            Vector v = m.Column(2),
                v2 = Vector.Create(new double[] { 0, 2, 4 });
            Assert.AreEqual(0, (v - v2).NormSup, 1e-5);
        }

        [TestMethod()]
        public void RowTest()
        {
            int size = 3;
            Matrix m = Matrix.Create(size, size, 0);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    m[i, j] = i * j;
            Vector v = m.Row(2),
                v2 = Vector.Create(new double[] { 0, 2, 4 });
            Assert.AreEqual(0, (v - v2).NormSup, 1e-5);
        }

        #endregion

        [TestMethod()]
        public void PowerTest()
        {
            int n = 5,
                dimension = 6;
            Matrix x = Matrix.CreateRandom(dimension, dimension);
            Matrix y = Matrix.Power(x, n);
            Matrix control = x.Clone;
            for (int i = 1; i < n; i++)
                control *= x;
            Assert.AreEqual((control - y).Norm1, 0, 1e-9, "The power does not behave as expected");
        }

        [TestMethod()]
        public void TransposeBySelfTest()
        {
            int n = 100;
            Matrix m = Matrix.CreateFromColumns(Vector.Create(n, 1.0), Vector.Create(n, 2.0)),
                tmm = Matrix.TransposeBySelf(m);
            Assert.IsTrue(tmm.Rows == 2 && tmm.Columns == 2 && tmm[0, 0] == n && tmm[0, 1] == tmm[1, 0] && tmm[1, 1] == 4 * n);
        }

        [TestMethod()]
        public void FastTransposeBySelfTest()
        {
            int n = 100;
            Matrix m = Matrix.CreateFromColumns(Vector.Create(n, 1.0), Vector.Create(n, 2.0)),
                tmm1 = Matrix.TransposeBySelf(m),
                tmm2 = Matrix.FastTransposeBySelf(m);
            Assert.IsTrue((tmm1 - tmm2).SumOfSquares == 0);
        }

        #region Apply

        [TestMethod()]
        public void ApplyTest()
        {
            int rows = 5,
                cols = 7;
            Func<double, double> function = Math.Sin;
            Matrix m = Matrix.Create(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    m[i, j] = i + 2 * j;

            Matrix result = Matrix.Apply(m, function);

            bool succeeded = true;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (result[i, j] != function(m[i, j]))
                        succeeded = false;

            Assert.IsTrue(succeeded, "The output of Apply does not match the expected values");
        }

        [TestMethod()]
        public void ApplyTestSize()
        {
            int rows = 5,
                cols = 7;
            Func<double, double> function = Math.Sin;
            Matrix m = Matrix.Create(rows, cols),
                result = Matrix.Apply(m, function);

            Assert.IsTrue(result.Rows == m.Rows && result.Columns == m.Columns, "The output of Apply does not match the expected size");
        }

        #endregion

        [TestMethod()]
        public void ScalarTest()
        {
            int rows = 5,
                cols = 7,
                control = 0;
            Matrix m1 = Matrix.Create(rows, cols),
                m2 = Matrix.Create(rows, cols);

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    m1[i, j] = i * j;
                    m2[i, j] = i + j;
                    control += (i * j) * (i + j);
                }
            Assert.IsTrue(control == Matrix.Scalar(m1, m2), "The scalar product does not match");
        }

        [TestMethod()]
        public void EqualsTest()
        {
            Matrix m1 = Matrix.Create(2, 3, 5),
                m2 = Matrix.Create(2, 3);
            for (int i = 0; i < m2.Rows; i++)
                for (int j = 0; j < m2.Columns; j++)
                    m2[i, j] = 5;
            Assert.AreEqual(0, (m1 - m2).Norm2, 1e-10);
        }

        [TestMethod()]
        public void ToStringTest()
        {
            int dimension = 2;
            Matrix m = Matrix.CreateSquare(dimension);
            int k = 1;
            for (int i = 0; i < dimension; i++)
                for (int j = 0; j < dimension; j++)
                {
                    m[i, j] = k;
                    k++;
                }

            string toString = m.ToString(),
                control = string.Format("1;2{0}3;4", Environment.NewLine);
            Assert.IsTrue(toString == control, "The ToString method does not behave as expected");
        }

        [TestMethod()]
        public void LinearCombinationTest()
        {
            Matrix m1 = Matrix.Create(10, 10, 1),
                m2 = Matrix.Create(10, 10, 2),
                m3 = Matrix.LinearCombination(Math.PI, m1, Math.E, m2);
            Assert.AreEqual(Math.PI + 2 * Math.E, m3.NormSup, 1e-10);
        }

        [TestMethod()]
        public void CreateFromColumnsTest()
        {
            Vector v1 = Vector.Create(10, 1.0),
                v2 = Vector.Create(10, 2.0),
                v3 = Vector.Create(10, 3.0),
                v4 = Vector.Create(10, 4.0);
            Matrix m = Matrix.CreateFromColumns(new Vector[] { v1, v2, v3, v4 });
            Assert.IsTrue(m.Rows == 10 && m.Columns == 4 && m[2, 1] == 2.0);
        }
    }
}
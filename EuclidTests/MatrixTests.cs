using Euclid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Euclid.Tests
{
    [TestClass()]
    public class MatrixTests
    {
        [TestMethod()]
        public void MatrixTest()
        {
            Matrix m = new Matrix();
            Assert.AreEqual(m.Size, 4, "Failed on the argument less matrix creation");
        }

        [TestMethod()]
        public void MatrixTest1()
        {
            int n = 10;
            Matrix m = new Matrix(n);
            Assert.IsTrue(m.Rows == m.Columns && m.Rows == n, "Failed to build a square matrix");
        }

        [TestMethod()]
        public void MatrixTest2()
        {
            int r = 5, c = 7;
            Matrix m = new Matrix(r, c);
            Assert.IsTrue(m.Rows == r && m.Columns == c, "Failed to build a rectangular matrix");
        }

        [TestMethod()]
        public void MatrixTest3()
        {
            int r = 5, c = 7;
            double d = Math.PI;
            Matrix m = new Matrix(r, c, d);
            Assert.IsTrue(m.Rows == r && m.Columns == c && m.Data.All(v => v == d), "Failed to build a rectangular full matrix");
        }

        [TestMethod()]
        public void SetColTest()
        {
            int targetColumn = 3,
                dimension = 5;
            Matrix m = new Matrix(dimension),
                newCol = new Matrix(dimension, 1);

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
        public void SolveWithTest()
        {
            int dimension = 5;
            Matrix a = new Matrix(dimension);
            for (int i = 0; i < dimension; i++)
                for (int j = 0; j <= i; j++)
                    a[i, j] = 1;
            Matrix b = Matrix.RandomMatrix(dimension, 1);
            Matrix x = a.SolveWith(b);
            Assert.AreEqual((a * x - b).Norm1, 0, 1e-9, "The Solve with does not match the expected result");
        }

        [TestMethod()]
        public void ColumnTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RowTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PowerTest()
        {
            int n = 5,
                dimension = 6;
            Matrix x = Matrix.RandomMatrix(dimension, dimension);
            Matrix y = Matrix.Power(x, n);
            Matrix control = x.Clone;
            for (int i = 1; i < n; i++)
                control *= x;
            Assert.AreEqual((control - y).Norm1, 0, 1e-9, "The power does not behave as expected");
        }

        [TestMethod()]
        public void ZeroMatrixTest()
        {
            int rows = 5,
                cols = 8;
            Matrix z = Matrix.ZeroMatrix(rows, cols);

            bool succeeded = true;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (z[i, j] != 0)
                        succeeded = false;
            Assert.IsTrue(succeeded, "The zero matrix doesnot contain only zeros");
        }

        [TestMethod()]
        public void IdentityMatrixTest()
        {
            int rows = 4,
                cols = rows + 1;
            Matrix m = Matrix.IdentityMatrix(rows, cols),
                control = new Matrix(rows, cols);
            for (int i = 0; i < rows; i++)
                control[i, i] = 1;
            Assert.AreEqual((control - m).Norm1, 0, "The IdentityMatrix does not match the expected");
        }

        [TestMethod()]
        public void BandMatrixTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RandomMatrixTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RandomMatrixTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TransposeBySelfTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FastTransposeBySelfTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ApplyTest()
        {
            int rows = 5,
                cols = 7;
            Func<double, double> function = Math.Sin;
            Matrix m = new Matrix(rows, cols);
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
            Matrix m = new Matrix(rows, cols),
                result = Matrix.Apply(m, function);

            Assert.IsTrue(result.Rows == m.Rows && result.Columns == m.Columns, "The output of Apply does not match the expected size");
        }

        [TestMethod()]
        public void ScalarTest()
        {
            int rows = 5,
                cols = 7,
                control = 0;
            Matrix m1 = new Matrix(rows, cols),
                m2 = new Matrix(rows, cols);

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
            Assert.Fail();
        }

        [TestMethod()]
        public void ToStringTest()
        {
            int dimension = 2;
            Matrix m = new Matrix(dimension);
            for (int i = 0; i < m.Size; i++)
                m[i] = i + 1;

            string toString = m.ToString(),
                control = string.Format("1;2{0}3;4", Environment.NewLine);
            Assert.IsTrue(toString == control, "The ToString method does not behave as expected");
        }


    }
}
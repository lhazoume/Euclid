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
            Assert.AreEqual(m.Size, 4, "Failed on the argumentless matrix creation");
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
        public void CreateTest5()
        {
            int rows = 5,
                cols = 8;
            Matrix z = Matrix.Create(rows, cols);

            bool succeeded = true;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (z[i, j] != 0)
                    {
                        succeeded = false;
                        break;
                    }
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

        #endregion

        #region Inversions

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

        [TestMethod()]
        public void InverseTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 4.0;
            m[0, 1] = 7.0;
            m[1, 0] = 2.0;
            m[1, 1] = 6.0;
            Matrix inverse = m.Inverse;

            Assert.IsNotNull(inverse);
            Assert.AreEqual(0.6, inverse[0, 0], 1e-9);
            Assert.AreEqual(-0.7, inverse[0, 1], 1e-9);
            Assert.AreEqual(-0.2, inverse[1, 0], 1e-9);
            Assert.AreEqual(0.4, inverse[1, 1], 1e-9);
        }

        [TestMethod()]
        public void FastInverseTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 4.0;
            m[0, 1] = 7.0;
            m[1, 0] = 2.0;
            m[1, 1] = 6.0;
            Matrix inverse = m.FastInverse;

            Assert.IsNotNull(inverse);
            Assert.AreEqual(0.6, inverse[0, 0], 1e-9);
            Assert.AreEqual(-0.7, inverse[0, 1], 1e-9);
            Assert.AreEqual(-0.2, inverse[1, 0], 1e-9);
            Assert.AreEqual(0.4, inverse[1, 1], 1e-9);
        }

        [TestMethod()]
        public void DiagonalTest()
        {
            Matrix m = Matrix.Create(3, 3);
            m[0, 0] = 1.0;
            m[1, 1] = 2.0;
            m[2, 2] = 3.0;
            Matrix diagonal = m.Diagonal;

            Assert.AreEqual(1.0, diagonal[0, 0]);
            Assert.AreEqual(2.0, diagonal[1, 1]);
            Assert.AreEqual(3.0, diagonal[2, 2]);
            Assert.AreEqual(0.0, diagonal[0, 1]);
        }

        [TestMethod()]
        public void CoMatrixTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[0, 1] = 2.0;
            m[1, 0] = 3.0;
            m[1, 1] = 4.0;
            Matrix coMatrix = m.CoMatrix;

            Assert.AreEqual(4.0, coMatrix[0, 0]);
            Assert.AreEqual(-3.0, coMatrix[0, 1]);
            Assert.AreEqual(-2.0, coMatrix[1, 0]);
            Assert.AreEqual(1.0, coMatrix[1, 1]);
        }

        [TestMethod()]
        public void CholeskyLowerTest()
        {
            Matrix m = Matrix.Create(3, 3);
            m[0, 0] = 4.0;
            m[0, 1] = 12.0;
            m[0, 2] = -16.0;
            m[1, 0] = 12.0;
            m[1, 1] = 37.0;
            m[1, 2] = -43.0;
            m[2, 0] = -16.0;
            m[2, 1] = -43.0;
            m[2, 2] = 98.0;
            Matrix cholesky = m.CholeskyLower;

            Assert.AreEqual(2.0, cholesky[0, 0], 1e-9);
            Assert.AreEqual(6.0, cholesky[1, 0], 1e-9);
            Assert.AreEqual(1.0, cholesky[1, 1], 1e-9);
            Assert.AreEqual(-8.0, cholesky[2, 0], 1e-9);
            Assert.AreEqual(5.0, cholesky[2, 1], 1e-9);
            Assert.AreEqual(3.0, cholesky[2, 2], 1e-9);
        }
        #endregion

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

        #region Manipulations

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
        public void TransposeTest()
        {
            Matrix m = Matrix.Create(2, 3);
            m[0, 1] = 2.0;
            m[1, 2] = 3.0;
            Matrix transpose = m.Transpose;
            Assert.AreEqual(3, transpose.Rows);
            Assert.AreEqual(2, transpose.Columns);
            Assert.AreEqual(2.0, transpose[1, 0]);
            Assert.AreEqual(3.0, transpose[2, 1]);
        }

        [TestMethod()]
        public void FastTransposeTest()
        {
            Matrix m = Matrix.Create(2, 3);
            m[0, 1] = 2.0;
            m[1, 2] = 3.0;
            Matrix transpose = m.FastTranspose;

            Assert.AreEqual(3, transpose.Rows);
            Assert.AreEqual(2, transpose.Columns);
            Assert.AreEqual(2.0, transpose[1, 0]);
            Assert.AreEqual(3.0, transpose[2, 1]);
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


        [TestMethod()]
        public void SymmetricPartTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 1] = 2.0;
            m[1, 0] = 3.0;
            Matrix symmetricPart = m.SymmetricPart;

            Assert.AreEqual(0.0, symmetricPart[0, 0]);
            Assert.AreEqual(2.5, symmetricPart[0, 1]);
            Assert.AreEqual(2.5, symmetricPart[1, 0]);
            Assert.AreEqual(0.0, symmetricPart[1, 1]);
        }

        [TestMethod()]
        public void AntiSymmetricPartTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 1] = 2.0;
            m[1, 0] = 3.0;
            Matrix antiSymmetricPart = m.AntiSymmetricPart;

            Assert.AreEqual(0.0, antiSymmetricPart[0, 0]);
            Assert.AreEqual(-0.5, antiSymmetricPart[0, 1]);
            Assert.AreEqual(0.5, antiSymmetricPart[1, 0]);
            Assert.AreEqual(0.0, antiSymmetricPart[1, 1]);
        }

        [TestMethod()]
        public void HadamardTest()
        {
            Matrix m1 = Matrix.Create(2, 2);
            m1[0, 0] = 1.0;
            m1[0, 1] = 2.0;
            m1[1, 0] = 3.0;
            m1[1, 1] = 4.0;

            Matrix m2 = Matrix.Create(2, 2);
            m2[0, 0] = 2.0;
            m2[0, 1] = 0.5;
            m2[1, 0] = 1.5;
            m2[1, 1] = 2.0;

            Matrix hadamard = Matrix.Hadamard(m1, m2);

            Assert.AreEqual(2.0, hadamard[0, 0]);
            Assert.AreEqual(1.0, hadamard[0, 1]);
            Assert.AreEqual(4.5, hadamard[1, 0]);
            Assert.AreEqual(8.0, hadamard[1, 1]);
        }

        [TestMethod()]
        public void MaxTest()
        {
            Matrix m1 = Matrix.Create(2, 2);
            m1[0, 0] = 1.0;
            m1[0, 1] = 2.0;
            m1[1, 0] = 3.0;
            m1[1, 1] = 4.0;

            Matrix m2 = Matrix.Create(2, 2);
            m2[0, 0] = 2.0;
            m2[0, 1] = 1.5;
            m2[1, 0] = 2.5;
            m2[1, 1] = 3.5;

            Matrix max = Matrix.Max(m1, m2);

            Assert.AreEqual(2.0, max[0, 0]);
            Assert.AreEqual(2.0, max[0, 1]);
            Assert.AreEqual(3.0, max[1, 0]);
            Assert.AreEqual(4.0, max[1, 1]);
        }

        #endregion

        #region Miscellaneaous

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

        #endregion

        [TestMethod()]
        public void ColumnsTest()
        {
            Matrix m = Matrix.Create(3, 4);
            Assert.AreEqual(4, m.Columns);
        }

        [TestMethod()]
        public void RowsTest()
        {
            Matrix m = Matrix.Create(3, 4);
            Assert.AreEqual(3, m.Rows);
        }

        [TestMethod()]
        public void IsSquareTest()
        {
            Matrix m1 = Matrix.Create(3, 3);
            Matrix m2 = Matrix.Create(3, 4);
            Assert.IsTrue(m1.IsSquare);
            Assert.IsFalse(m2.IsSquare);
        }

        [TestMethod()]
        public void IsSymetricTest()
        {
            Matrix m1 = Matrix.Create(3, 3);
            m1[0, 1] = 1;
            m1[1, 0] = 1;
            Assert.IsTrue(m1.IsSymetric);

            Matrix m2 = Matrix.Create(3, 3);
            m2[0, 1] = 1;
            m2[1, 0] = 2;
            Assert.IsFalse(m2.IsSymetric);
        }

        [TestMethod()]
        public void SizeTest()
        {
            Matrix m = Matrix.Create(3, 4);
            Assert.AreEqual(12, m.Size);
        }

        [TestMethod()]
        public void DataTest()
        {
            Matrix m = Matrix.Create(2, 2, 5.0);
            double[] data = m.Data;
            Assert.AreEqual(5.0, data[0]);
            Assert.AreEqual(5.0, data[1]);
            Assert.AreEqual(5.0, data[2]);
            Assert.AreEqual(5.0, data[3]);
        }

        [TestMethod()]
        public void ArrayTest()
        {
            Matrix m = Matrix.Create(2, 2, 5.0);
            double[,] array = m.Array;
            Assert.AreEqual(5.0, array[0, 0]);
            Assert.AreEqual(5.0, array[0, 1]);
            Assert.AreEqual(5.0, array[1, 0]);
            Assert.AreEqual(5.0, array[1, 1]);
        }

        [TestMethod()]
        public void JaggedArrayTest()
        {
            Matrix m = Matrix.Create(2, 2, 5.0);
            double[][] jaggedArray = m.JaggedArray;
            Assert.AreEqual(5.0, jaggedArray[0][0]);
            Assert.AreEqual(5.0, jaggedArray[0][1]);
            Assert.AreEqual(5.0, jaggedArray[1][0]);
            Assert.AreEqual(5.0, jaggedArray[1][1]);
        }

        [TestMethod()]
        public void IndexerTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[1, 1] = 2.0;
            Assert.AreEqual(1.0, m[0, 0]);
            Assert.AreEqual(2.0, m[1, 1]);
        }

        [TestMethod()]
        public void LTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 4.0;
            m[0, 1] = 3.0;
            m[1, 0] = 6.0;
            m[1, 1] = 3.0;
            Matrix L = m.L;
            Assert.AreEqual(1.0, L[0, 0]);
            Assert.AreEqual(0.0, L[0, 1]);
            Assert.AreEqual(1.5, L[1, 0]);
            Assert.AreEqual(1.0, L[1, 1]);
        }

        [TestMethod()]
        public void UTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 4.0;
            m[0, 1] = 3.0;
            m[1, 0] = 6.0;
            m[1, 1] = 3.0;
            Matrix U = m.U;
            Assert.AreEqual(4.0, U[0, 0]);
            Assert.AreEqual(3.0, U[0, 1]);
            Assert.AreEqual(0.0, U[1, 0]);
            Assert.AreEqual(-1.5, U[1, 1]);
        }

        [TestMethod()]
        public void DeterminantTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 4.0;
            m[0, 1] = 3.0;
            m[1, 0] = 6.0;
            m[1, 1] = 3.0;
            double det = m.Determinant;
            Assert.AreEqual(-6.0, det, 1e-9);
        }

        [TestMethod()]
        public void TraceTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[1, 1] = 2.0;
            double trace = m.Trace;
            Assert.AreEqual(3.0, trace);
        }



        [TestMethod()]
        public void Norm1Test()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[0, 1] = -2.0;
            m[1, 0] = 3.0;
            m[1, 1] = -4.0;
            double norm1 = m.Norm1;
            Assert.AreEqual(10.0, norm1);
        }

        [TestMethod()]
        public void Norm2Test()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[0, 1] = -2.0;
            m[1, 0] = 3.0;
            m[1, 1] = -4.0;
            double norm2 = m.Norm2;
            Assert.AreEqual(5.477225575051661, norm2, 1e-9);
        }

        [TestMethod()]
        public void NormSupTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[0, 1] = -2.0;
            m[1, 0] = 3.0;
            m[1, 1] = -4.0;
            double normSup = m.NormSup;
            Assert.AreEqual(4.0, normSup);
        }

        [TestMethod()]
        public void SumOfSquaresTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[0, 1] = -2.0;
            m[1, 0] = 3.0;
            m[1, 1] = -4.0;
            double sumOfSquares = m.SumOfSquares;
            Assert.AreEqual(30.0, sumOfSquares);
        }

        [TestMethod()]
        public void SumTest()
        {
            Matrix m = Matrix.Create(2, 2);
            m[0, 0] = 1.0;
            m[0, 1] = -2.0;
            m[1, 0] = 3.0;
            m[1, 1] = -4.0;
            double sum = m.Sum;
            Assert.AreEqual(-2.0, sum);
        }


        [TestMethod()]
        public void SetRowTest()
        {
            Matrix m = Matrix.Create(2, 2);
            Vector row = Vector.Create(2);
            row[0] = 1.0;
            row[1] = 2.0;
            m.SetRow(row, 1);
            Assert.AreEqual(1.0, m[1, 0]);
            Assert.AreEqual(2.0, m[1, 1]);
        }

        #region Operators

        [TestMethod()]
        public void OperatorMultiplyScalarTest()
        {
            Matrix m = Matrix.Create(2, 2, 2.0);
            Matrix result = m * 3.0;
            Assert.AreEqual(6.0, result[0, 0]);
            Assert.AreEqual(6.0, result[0, 1]);
            Assert.AreEqual(6.0, result[1, 0]);
            Assert.AreEqual(6.0, result[1, 1]);
        }

        [TestMethod()]
        public void OperatorMultiplyScalarReverseTest()
        {
            Matrix m = Matrix.Create(2, 2, 2.0);
            Matrix result = 3.0 * m;
            Assert.AreEqual(6.0, result[0, 0]);
            Assert.AreEqual(6.0, result[0, 1]);
            Assert.AreEqual(6.0, result[1, 0]);
            Assert.AreEqual(6.0, result[1, 1]);
        }

        [TestMethod()]
        public void OperatorDivideScalarTest()
        {
            Matrix m = Matrix.Create(2, 2, 6.0);
            Matrix result = m / 3.0;
            Assert.AreEqual(2.0, result[0, 0]);
            Assert.AreEqual(2.0, result[0, 1]);
            Assert.AreEqual(2.0, result[1, 0]);
            Assert.AreEqual(2.0, result[1, 1]);
        }

        [TestMethod()]
        public void OperatorMultiplyMatrixTest()
        {
            Matrix m1 = Matrix.Create(2, 2);
            m1[0, 0] = 1.0;
            m1[0, 1] = 2.0;
            m1[1, 0] = 3.0;
            m1[1, 1] = 4.0;

            Matrix m2 = Matrix.Create(2, 2);
            m2[0, 0] = 2.0;
            m2[0, 1] = 0.5;
            m2[1, 0] = 1.5;
            m2[1, 1] = 2.0;

            Matrix result = m1 * m2;

            Assert.AreEqual(5.0, result[0, 0]);
            Assert.AreEqual(4.5, result[0, 1]);
            Assert.AreEqual(13.0, result[1, 0]);
            Assert.AreEqual(9.5, result[1, 1]);
        }

        [TestMethod()]
        public void OperatorMultiplyMatrixParallelTest()
        {
            Matrix m1 = Matrix.Create(2, 2);
            m1[0, 0] = 1.0;
            m1[0, 1] = 2.0;
            m1[1, 0] = 3.0;
            m1[1, 1] = 4.0;

            Matrix m2 = Matrix.Create(2, 2);
            m2[0, 0] = 2.0;
            m2[0, 1] = 0.5;
            m2[1, 0] = 1.5;
            m2[1, 1] = 2.0;

            Matrix result = m1 ^ m2;

            Assert.AreEqual(5.0, result[0, 0]);
            Assert.AreEqual(4.5, result[0, 1]);
            Assert.AreEqual(13.0, result[1, 0]);
            Assert.AreEqual(9.5, result[1, 1]);
        }

        [TestMethod()]
        public void OperatorAddScalarTest()
        {
            Matrix m = Matrix.Create(2, 2, 2.0);
            Matrix result = m + 3.0;
            Assert.AreEqual(5.0, result[0, 0]);
            Assert.AreEqual(5.0, result[0, 1]);
            Assert.AreEqual(5.0, result[1, 0]);
            Assert.AreEqual(5.0, result[1, 1]);
        }

        #endregion

        #region Additional Tests

        [TestMethod()]
        public void CloneTest()
        {
            Matrix m = Matrix.Create(3, 3, 5.0);
            Matrix clone = m.Clone;
            Assert.AreEqual(m.Rows, clone.Rows);
            Assert.AreEqual(m.Columns, clone.Columns);
            Assert.IsTrue(m.Data.SequenceEqual(clone.Data));
        }
        #endregion
    }
}
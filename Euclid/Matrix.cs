﻿using Euclid.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Euclid
{
    /// <summary>
    /// Matrix of double
    /// </summary>
    public class Matrix
    {
        #region Declarations

        private readonly int _rows, _cols;
        private readonly double[] _data;
        private Matrix _L, _U;
        private int[] _pi;
        private double _detOfP = 1;

        #endregion

        #region Constructors

        /// <summary>Builds a rectangular matrix filled with the specified value</summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="cols">the number of columns</param>
        /// <param name="value">the value for all the fields of the matrix</param>
        private Matrix(int rows, int cols, double value)
        {
            if (rows <= 0) throw new ArgumentException("No matrix can have less than one row");
            if (cols <= 0) throw new ArgumentException("No matrix can have less than one column");
            _rows = rows;
            _cols = cols;

            int size = _rows * _cols;
            _data = new double[size];
            if (value != 0)
                for (int i = 0; i < size; i++)
                    _data[i] = value;
        }

        #endregion

        #region Accessors
        /// <summary>Returns the number of columns of the <c>Matrix</c></summary>
        public virtual int Columns => _cols;

        /// <summary>Returns the number of rows of the <c>Matrix</c></summary>
        public int Rows => _rows;

        /// <summary>Specifies whether the <c>Matrix</c> is square</summary>
        public bool IsSquare => (_rows == _cols);

        /// <summary>Specifies whether the <c>Matrix</c> is square and symmetric</summary>
        public bool IsSymmetric
        {
            get
            {
                if (_rows != _cols) return false;
                for (int i = 0; i < _rows; i++)
                    for (int j = 0; j < _cols; j++)
                        if (_data[i * _cols + j] != _data[j * _cols + i])
                            return false;
                return true;
            }
        }

        /// <summary>Returns the number of values in the <c>Matrix</c></summary>
        public int Size => _data.Length;

        /// <summary>Returns the data of the matrix as a array of double with the rows one after the other</summary>
        public double[] Data => _data;

        /// <summary>Gets the matrix' data as a 2d-array</summary>
        public double[,] Array
        {
            get
            {
                double[,] result = new double[_rows, _cols];
                for (int i = 0; i < _rows; i++)
                    for (int j = 0; j < _cols; j++)
                        result[i, j] = _data[i * _cols + j];
                return result;
            }
        }

        /// <summary>Gets the matrix' data as a jagged array</summary>
        public double[][] JaggedArray
        {
            get
            {
                double[][] result = Arrays.Build<double>(_rows, _cols);
                for (int i = 0; i < _rows; i++)
                    for (int j = 0; j < _cols; j++)
                        result[i][j] = _data[i * _cols + j];
                return result;
            }
        }

        /// <summary>Allows reading and modifying the coefficients of the <c>Matrix</c></summary>
        /// <param name="i">the row</param>
        /// <param name="j">the column</param>
        /// <returns>a double value</returns>
        public double this[int i, int j]
        {
            get { return _data[i * _cols + j]; }
            set { _data[i * _cols + j] = value; }
        }

        /// <summary> Allows reading and modifying the coefficients of the <c>Matrix</c></summary>
        /// <param name="i">the coefficient index</param>
        /// <returns>a double value</returns>
        private double this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        /// <summary>Returns a deep copy of the <c>Matrix</c></summary>
        public Matrix Clone
        {
            get
            {
                Matrix result = new Matrix(_rows, _cols, 0);
                for (int i = 0; i < _rows * _cols; i++)
                    result._data[i] = _data[i];
                return result;
            }
        }


        /// <summary>Returns the diagonal matrix of the current Matrix</summary>
        public Matrix Diagonal
        {
            get
            {
                Matrix result = new Matrix(_rows, _cols, 0);
                for (int i = 0; i < Math.Min(_rows, _cols); i++)
                    result[i * (1 + _cols)] = this[i * (1 + _cols)];
                return result;
            }
        }

        #region Inversion

        /// <summary>The Lower triangular part of the LU decomposition</summary>
        public Matrix L
        {
            get
            {
                if (_L == null) MakeLU();
                return _L;
            }
        }

        /// <summary>Returns the upper triangular part of the LU decomposition</summary>
        public Matrix U
        {
            get
            {
                if (_U == null) MakeLU();
                return _U;
            }
        }

        /// <summary>Returns the determinant of the <c>Matrix</c></summary>
        public double Determinant
        {
            get
            {
                try { if (_L == null) MakeLU(); }
                catch { return 0; }

                double det = _detOfP;
                for (int i = 0; i < _rows; i++) det *= _U[i, i];
                return det;
            }
        }

        /// <summary>Returns the inverse of the <c>Matrix</c> when possible, null otherwise</summary>
        public Matrix Inverse
        {
            get
            {
                try { if (_L == null) MakeLU(); }
                catch { return null; }

                Matrix inv = Matrix.Create(_rows, _cols);
                for (int i = 0; i < _rows; i++)
                {
                    Vector Ei = Vector.Create(_rows);
                    Ei[i] = 1;
                    Vector col = SolveWith(Ei);
                    inv.SetCol(col, i);
                }
                return inv;
            }
        }

        /// <summary>Returns the inverse of the <c>Matrix</c> when possible, null otherwise. Done using multithreading !</summary>
        public Matrix FastInverse
        {
            get
            {
                double[,] both = new double[_rows, 2 * _cols];
                Parallel.For(0, _rows * _cols, l =>
                {
                    int i = l / _rows, j = l % _rows;
                    both[i, j] = _data[i * _cols + j];
                    if (i == j) both[i, i + _rows] = 1;
                });

                for (int i = 0; i < _rows; i++)
                {
                    int pivot = i;
                    for (int j = i + 1; j < _rows; j++)
                        if (both[j, i] > both[pivot, i])
                            pivot = j;

                    #region Row Swap
                    if (i != pivot)
                        for (int col = 0; col < 2 * _rows; col++)
                        {
                            double tmp = both[i, col];
                            both[i, col] = both[pivot, col];
                            both[pivot, col] = tmp;
                        }
                    #endregion

                    //dividing the row by a[i][i]
                    if (Math.Abs(both[i, i]) < _ACCURACY_) return null;
                    double norm = 1 / both[i, i];
                    Parallel.For(0, 2 * _rows, col => { both[i, col] *= norm; });

                    //making other elements 0 in order to make the matrix a[][] an indentity matrix and obtaining a inverse b[][] matrix
                    Parallel.For(0, _rows, q =>
                    {
                        if (i != q)
                        {
                            double fact = both[q, i];
                            for (int j = 0; j < 2 * _rows; j++) both[q, j] -= fact * both[i, j];
                        }
                    });
                }

                Matrix result = new Matrix(_rows, _rows, 0);
                Parallel.For(0, _rows * _rows, l =>
                {
                    int i = l / _rows, j = l % _rows;
                    result[i * _rows + j] = both[i, j + _rows];
                });
                return result;
            }
        }

        /// <summary>Returns the co-matrix of the current matrix</summary>
        public Matrix CoMatrix
        {
            get
            {
                Matrix ret = new Matrix(_rows, _cols, 0);
                if (_rows == 1)
                    ret[0, 0] = 1;
                else
                {
                    for (int i = 0; i < _rows; i++)
                        for (int j = 0; j < _rows; j++)
                            ret[i, j] = Expo(i + j) * this.SubMatrix(i, j).Determinant;
                }
                return ret;
            }
        }

        /// <summary>Returns the lower Cholesky factor</summary>
        public Matrix CholeskyLower
        {
            get
            {
                if (_rows != _cols)
                    return null;

                Matrix ret = new Matrix(_rows, _rows, 0);
                for (int r = 0; r < _rows; r++)
                    for (int c = 0; c <= r; c++)
                    {
                        double sum = 0;

                        for (int j = 0; j < c; j++)
                            sum += ret[r, j] * ret[c, j];

                        ret[r, c] = c == r ? Math.Sqrt(this[r, c] - sum) : 1.0 / ret[c, c] * (this[r, c] - sum);
                    }

                return ret;
            }
        }

        #endregion

        /// <summary>Returns the trace of the <c>Matrix</c></summary>
        public double Trace
        {
            get
            {
                double sum = 0;
                if (_rows == _cols)
                    for (int i = 0; i < _rows; i++)
                        sum += this[i * (1 + _cols)];
                return sum;
            }
        }

        /// <summary>Returns the transposed <c>Matrix</c></summary>
        public virtual Matrix Transpose
        {
            get
            {
                Matrix t = new Matrix(_cols, _rows, 0);
                if (_rows == 1 || _cols == 1)
                    for (int k = 0; k < _data.Length; k++)
                        t._data[k] = _data[k];
                else
                    for (int k = 0; k < _data.Length; k++)
                        t._data[k] = _data[(k % _rows) * _cols + (k / _rows)];
                return t;
            }
        }

        /// <summary>Returns the transposed <c>Matrix</c> filled using multithreading</summary>
        public Matrix FastTranspose
        {
            get
            {
                Matrix t = Matrix.Create(_cols, _rows);
                Parallel.For(0, _rows * _cols, k => { t._data[k] = _data[(k % _rows) * _cols + (k / _rows)]; });
                return t;
            }
        }

        /// <summary>Returns the symmetric part of the <c>Matrix</c> ( 1/2 . (A^T + A))</summary>
        public Matrix SymmetricPart => 0.5 * (this + this.Transpose);

        /// <summary>Returns the symmetric part of the <c>Matrix</c> (1/2 . (A - A^T))</summary>
        public Matrix AntiSymmetricPart => 0.5 * (this - this.Transpose);

        #region Norms and sums

        /// <summary>Returns the sum of the absolute values</summary>
        public double Norm1 => _data.Sum(Math.Abs);

        /// <summary>Returns the square root of the sum of squares (Frobenius norm)</summary>
        public double Norm2 => Math.Sqrt(this.SumOfSquares);

        /// <summary>Returns the largest value of the <c>Matrix</c> in absolute value</summary>
        public double NormSup => _data.Max(Math.Abs);

        /// <summary>Returns the sum of the squared values</summary>
        public double SumOfSquares
        {
            get
            {
                double result = 0;
                for (int k = 0; k < _data.Length; k++)
                    result += _data[k] * _data[k];
                return result;
            }
        }

        /// <summary>
        /// Returns a matrix with sqrt values
        /// </summary>
        public Matrix Sqrt
        {
            get
            {
                Matrix result = Create(_cols, _rows);
                for (int i = 0; i < _rows; i++)
                    for (int j = 0; j < _cols; j++)
                        result[i, j] += Math.Sqrt(this[i, j]);

                return result;
            }
        }

        /// <summary>Returns the sum of the values</summary>
        public double Sum => _data.Sum();

        #endregion

        #endregion

        #region Methods

        #region Private methods

        #region particular multiplication
        /// <summary>
        /// Multiply rotation matrix by a matrix from the left side only the rows p and q will affect the other matrix
        /// </summary>
        /// <param name="a">Matrix</param>
        /// <param name="r">Rotation matrix</param>
        /// <param name="p">row p</param>
        /// <param name="q">row q</param>
        /// <param name="size">size</param>
        public static void MultTranspLeft(Matrix a, Matrix r, int p, int q, int size)
        {
            int j, k;
            double[,] al = new double[2, size];
            for (j = 0; j < size; j++)
            {
                al[0, j] = 0.0;
                al[1, j] = 0.0;
                for (k = 0; k < size; k++)
                {
                    al[0, j] = al[0, j] + (r[k, p] * a[k, j]);
                    al[1, j] = al[1, j] + (r[k, q] * a[k, j]);
                }
            }
            for (j = 0; j < size; j++)
            {
                a[p, j] = al[0, j];
                a[q, j] = al[1, j];
            }
        }

        /// <summary>
        /// Multiply rotation matrix by a matrix from the right side only the rows p and q will affect the other matrix
        /// </summary>
        /// <param name="a">Matrix</param>
        /// <param name="r">Rotation matrix</param>
        /// <param name="p">row p</param>
        /// <param name="q">row q</param>
        /// <param name="size">size</param>
        public static void MultRight(Matrix a, Matrix r, int p, int q, int size)
        {
            int i, k;
            double[,] al = new double[size, 2];
            for (i = 0; i < size; i++)
            {
                al[i, 0] = 0.0;
                al[i, 1] = 0.0;
                for (k = 0; k < size; k++)
                {
                    al[i, 0] = al[i, 0] + (a[i, k] * r[k, p]);
                    al[i, 1] = al[i, 1] + (a[i, k] * r[k, q]);
                }
            }
            for (i = 0; i < size; i++)
            {
                a[i, p] = al[i, 0];
                a[i, q] = al[i, 1];
            }
        }

        #endregion

        #region Inversion services

        /// <summary>Evaluates the LU decomposition of the matrix and stores the results in the private attributes _L and _U.</summary>
        private void MakeLU()
        {
            if (!IsSquare) throw new Exception("The matrix is not square!");
            _L = CreateIdentityMatrix(_rows, _cols);
            _U = this.Clone;

            _pi = new int[_rows];
            for (int i = 0; i < _rows; i++) _pi[i] = i;

            double pom2;
            int k0 = 0;

            for (int k = 0; k < _cols - 1; k++)
            {
                double p = 0;
                for (int i = k; i < _rows; i++)      // find the row with the biggest pivot
                {
                    if (Math.Abs(_U[i * _U._cols + k]) > p)
                    {
                        p = Math.Abs(_U[i * _U._cols + k]);
                        k0 = i;
                    }
                }
                if (p == 0) throw new Exception("The matrix is singular");

                int pom1 = _pi[k];
                _pi[k] = _pi[k0];
                _pi[k0] = pom1;    // switch two rows in permutation matrix

                for (int i = 0; i < k; i++)
                {
                    pom2 = _L[k * _L._cols + i];
                    _L[k * _L._cols + i] = _L[k0 * _L._cols + i];
                    _L[k0 * _L._cols + i] = pom2;
                }

                if (k != k0) _detOfP *= -1;

                for (int i = 0; i < _cols; i++)                  // Switch rows in U
                {
                    pom2 = _U[k * _U._cols + i];
                    _U[k * _U._cols + i] = _U[k0 * _U._cols + i];
                    _U[k0 * _U._cols + i] = pom2;
                }

                for (int i = k + 1; i < _rows; i++)
                {
                    _L[i * _L._cols + k] = _U[i * _U._cols + k] / _U[k * _U._cols + k];
                    for (int j = k; j < _cols; j++)
                        _U[i * _U._cols + j] = _U[i * _U._cols + j] - _L[i * _L._cols + k] * _U[k * _U._cols + j];
                }
            }
        }

        private static Vector SubsForth(Matrix A, Vector b)
        {
            int n = A.Rows;
            Vector x = Vector.Create(n);

            for (int i = 0; i < n; i++)
            {
                x[i] = b[i];
                for (int j = 0; j < i; j++) x[i] -= A[i * A._cols + j] * x[j];
                x[i] = x[i] / A[i * (A._cols + 1)];
            }
            return x;
        }

        private static Vector SubsBack(Matrix A, Vector b)
        {
            int n = A.Rows;
            Vector x = Vector.Create(n);

            for (int i = n - 1; i > -1; i--)
            {
                x[i] = b[i];
                for (int j = n - 1; j > i; j--) x[i] -= A[i * A._cols + j] * x[j];
                x[i] = x[i] / A[i * (A._cols + 1)];
            }
            return x;
        }

        /// <summary>Returns the matrix from which one row and one column have been excluded (indexed by row and col)</summary>
        /// <param name="row">The index of the row to exclude</param>
        /// <param name="col">The index of the column to exclude</param>
        /// <returns>The matrix without the speficied line and the column</returns>
        private Matrix SubMatrix(int row, int col)
        {
            int dim = _rows, ld = 0;
            Matrix sub = new Matrix(_rows - 1, _cols - 1, 0);
            for (int L = 0; L < dim; L++)
            {
                int cd = 0;
                if (L != row)
                {
                    for (int c = 0; c < dim; c++)
                        if (c != col)
                            sub[ld, cd++] = this[L, c];
                    ld++;
                }
            }
            return sub;
        }

        /// <summary>Replaces the column of index k with the input Vector v</summary>
        /// <param name="v">Column replacing the old one</param>
        /// <param name="k">Index of the column to replace</param>
        public void SetCol(Vector v, int k)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));
            for (int i = 0; i < _rows; i++) _data[i * _cols + k] = v[i];
        }

        /// <summary>Replaces the row of index k with the input Vector v</summary>
        /// <param name="v">Row replacing the old one</param>
        /// <param name="k">Index of the row to replace</param>
        public void SetRow(Vector v, int k)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));
            for (int i = 0; i < _cols; i++) _data[k * _cols + i] = v[i];
        }

        /// <summary>Solves the equation : A*x=v, where A is the Matrix, x the unknown, v the input argument</summary>
        /// <param name="v">The right hand side of the equation</param>
        /// <returns>The solution x of A*x=v</returns>
        public Vector SolveWith(Vector v)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));
            if (_rows != _cols) throw new Exception("The matrix is not square!");
            if (_rows != v.Size) throw new Exception("Wrong number of results in solution vector!");
            if (_L == null) MakeLU();

            Vector b = Vector.Create(_rows);
            for (int i = 0; i < _rows; i++) b[i] = v[_pi[i]];   // switch two items in "v" due to permutation matrix

            Vector z = SubsForth(_L, b);
            Vector x = SubsBack(_U, z);

            return x;
        }

        #endregion

        private static double Expo(int n)
        {
            return n % 2 != 0 ? 1 : -1;
        }

        #endregion

        #region Extract row or column

        /// <summary>Extracts a specific column</summary>
        /// <param name="j">the specified column</param>
        /// <returns>a column Vector</returns>
        public Vector Column(int j)
        {
            Vector result = Vector.Create(_rows);
            for (int i = 0; i < _rows; i++)
                result[i] = _data[i * _cols + j];
            return result;
        }

        /// <summary>
        /// Extract a specific column to a matrix [rows, 1]
        /// </summary>
        /// <param name="j">Column rank</param>
        /// <returns>A column matrix</returns>
        public Matrix ColumnMatrix(int j)
        {
            Matrix result = Create(_rows, 1);
            for (int i = 0; i < _rows; i++) result[i, 0] = _data[i * _cols + j];
            return result;
        }

        /// <summary> Extracts a specific row</summary>
        /// <param name="i">the specified row</param>
        /// <returns>a row Vector</returns>
        public Vector Row(int i)
        {
            Vector result = Vector.Create(_cols);
            for (int j = 0; j < _cols; j++)
                result[j] = _data[i * _cols + j];
            return result;
        }

        /// <summary>
        /// Extract a specific row to a matrix [1, col(s)]
        /// </summary>
        /// <param name="i">Row rank</param>
        /// <returns>A row matrix</returns>
        public Matrix RowMatrix(int i)
        {
            Matrix result = Create(1, _cols);
            for (int j = 0; j < _cols; j++) result[0, j] = _data[i * _cols + j];
            return result;
        }

        #endregion

        #endregion

        #region Operators

        #region Multiplications / divisions

        /// <summary>Multiplies a <c>Matrix</c> by a scalar</summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(Matrix m, double f)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Matrix tmp = m.Clone;
            for (int k = 0; k < m.Size; k++)
                tmp[k] = f * m[k];
            return tmp;
        }

        /// <summary> Multiplies a <c>Matrix</c> by a scalar</summary>
        /// <param name="f">the scalar</param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(double f, Matrix m)
        {
            return m * f;
        }

        /// <summary>Divides all the coefficients of a <c>Matrix</c> by a scalar</summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the division</returns>
        public static Matrix operator /(Matrix m, double f)
        {
            return m * (1 / f);
        }

        /// <summary>
        /// Divide a matrix by an another of the same dimension following outer instructions
        /// </summary>
        /// <param name="m1">the left hand side <c>Matrix</c></param>
        /// <param name="m2">the right hand side <c>Matrix</c></param>
        /// <returns>Divided matrix</returns>
        public static Matrix operator /(Matrix m1, Matrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns) throw new Exception($"Wrong dimension of row of matrix m1[{m1.Rows},{m1.Columns}] m2[{m2.Rows},{m2.Columns}]");

            Matrix result = Create(m1.Rows, m1.Columns);

            for (int i = 0; i < m1.Rows; i++)
                for (int j = 0; j < m1.Columns; j++)
                    result[i, j] = m1[i, j] / m2[i, j];

            return result;
        }

        /// <summary>Multiplies two matrices</summary>
        /// <param name="m1">the left hand side <c>Matrix</c></param>
        /// <param name="m2">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));
            if (m1.Columns != m2.Rows) throw new Exception("Wrong dimensions of matrix!");

            Matrix result = Create(m1.Rows, m2.Columns);
            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    for (int k = 0; k < m1.Columns; k++)
                        result._data[i * result._cols + j] += m1._data[i * m1._cols + k] * m2._data[k * m2._cols + j];
            return result;
        }

        /// <summary>Multiplies two matrices using multithreading</summary>
        /// <param name="m1">the left hand side <c>Matrix</c></param>
        /// <param name="m2">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator ^(Matrix m1, Matrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));

            if (m1.Columns != m2.Rows) throw new Exception("Wrong dimensions of matrix!");

            int n = m2._cols,
                p = m1._cols;

            Matrix result = Matrix.Create(m1._rows, n);
            int numberOfPairs = result._rows * n;

            Parallel.For(0, numberOfPairs, l =>
            {
                int i = l / n,
                    j = l % n;

                double sum = 0;
                for (int k = 0; k < p; k++) sum += m1._data[i * p + k] * m2._data[k * n + j];
                result._data[i * n + j] = sum;
            });
            return result;
        }

        #endregion

        #region Additions / substractions

        /// <summary>Performs a matrix addition, after going through dimension compatibility verifications.</summary>
        /// <param name="m1">First matrix</param>
        /// <param name="m2">Second matrix</param>
        /// <returns>The sum of m1 and m2</returns>
        private static Matrix Add(Matrix m1, Matrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns) throw new ArgumentException("Matrices must have the same dimensions!");
            Matrix r = Matrix.Create(m1.Rows, m1.Columns);
            for (int k = 0; k < r.Size; k++)
                r[k] = m1[k] + m2[k];
            return r;
        }

        /// <summary>Builds a Matrix as a linear combination of two matrices</summary>
        /// <param name="f1">the first Matrix' factor</param>
        /// <param name="m1">the first Matrix</param>
        /// <param name="f2">the second Matrix' factor</param>
        /// <param name="m2">the second Matrix</param>
        /// <returns>the Matrix result of f1*m1 + f2*m2</returns>
        public static Matrix LinearCombination(double f1, Matrix m1, double f2, Matrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns) throw new ArgumentException("Matrices must have the same dimensions!");
            Matrix r = Matrix.Create(m1.Rows, m1.Columns);
            for (int k = 0; k < r.Size; k++)
                r[k] = f1 * m1[k] + f2 * m2[k];
            return r;
        }

        /// <summary>Builds a Matrix as a linear combination of matrices</summary>
        /// <param name="factors">the factors</param>
        /// <param name="matrices">the matrices</param>
        /// <returns>the Matrix result of Sum i  fi*mi</returns>
        public static Matrix LinearCombination(double[] factors, Matrix[] matrices)
        {
            if (factors.Length != matrices.Length) throw new ArgumentException("the matrices do not match the factors");
            if (matrices.Any(m => m is null)) throw new ArgumentNullException(nameof(matrices));
            if (matrices.Any(m => m.Size != matrices[0].Size)) throw new ArgumentException("Matrices must have the same dimensions!");

            Matrix r = Matrix.Create(matrices[0].Rows, matrices[0].Columns);
            Parallel.For(0, r.Size, k =>
            {
                for (int i = 0; i < factors.Length; i++)
                    r[k] += factors[i] * matrices[i][k];
            });


            return r;
        }

        /// <summary>Adds a scalar to all the coefficients of a <c>Matrix</c></summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the addition</returns>
        public static Matrix operator +(Matrix m, double c)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] + c;
            return tmp;
        }

        /// <summary>Adds a scalar to all the coefficients of a <c>Matrix</c></summary>
        /// <param name="c">the scalar</param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the addition</returns>
        public static Matrix operator +(double c, Matrix m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] + c;
            return tmp;
        }

        /// <summary>Substracts a scalar to all the coefficients of a <c>Matrix</c></summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the substraction</returns>
        public static Matrix operator -(Matrix m, double c)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] - c;
            return tmp;
        }

        /// <summary>Adds a scalar to the opposite of a <c>Matrix</c></summary>
        /// <param name="c">the scalar</param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the substraction</returns>
        public static Matrix operator -(double c, Matrix m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = c - m[i];
            return tmp;
        }

        /// <summary>Returns the opposite of the <c>Matrix</c></summary>
        /// <param name="m">the input matrix</param>
        /// <returns>the <c>Matrix</c> opposite</returns>
        public static Matrix operator -(Matrix m)
        {
            return m * -1;
        }

        /// <summary>Performs the matrix addition</summary>
        /// <param name="m1">the left hand side matrix</param>
        /// <param name="m2">the right hand side matrix</param>
        /// <returns>a <c>Matrix</c></returns>
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, m2);
        }

        /// <summary>Performs a matrix substraction</summary>
        /// <param name="m1">the left hand side</param>
        /// <param name="m2">the right hand side</param>
        /// <returns>the <c>Matrix</c> result of the substraction</returns>
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, -m2);
        }

        #endregion

        /// <summary>Evaluates the matrix raised to a power specified by pow</summary>
        /// <param name="matrix">the matrix</param>
        /// <param name="pow">The power to raise the matrix to</param>
        /// <returns>The matrix, raised to the power pow</returns>
        public static Matrix Power(Matrix matrix, int pow)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            if (pow == 0) return CreateIdentityMatrix(matrix._rows, matrix._cols);
            if (pow == 1) return matrix.Clone;
            if (pow < 0) return Power(matrix.Inverse, -pow);

            Matrix x = matrix.Clone;

            Matrix ret = CreateIdentityMatrix(matrix._rows, matrix._cols);
            while (pow != 0)
            {
                if ((pow & 1) == 1) ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        #endregion

        #region Create Matrices

        /// <summary>Builds a <c>Matrix</c> from a 2d-array of double</summary>
        /// <param name="model">the 2d-array of data</param>
        public static Matrix Create(double[,] model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Matrix result = new Matrix(model.GetLength(0), model.GetLength(1), 0);

            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    result[i, j] = model[i, j];
            return result;
        }

        /// <summary>Builds a <c>Matrix</c> from a 2d-array of double</summary>
        /// <param name="model">the 2d-array of data</param>
        public static Matrix Create(double[][] model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Length == 0) throw new ArgumentException(nameof(model));
            if (model.Select(t => t.Length).Distinct().Count() != 1) throw new ArgumentException("the jagged array is not uniformely sized", nameof(model));
            Matrix result = new Matrix(model.Length, model[0].Length, 0);

            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    result[i, j] = model[i][j];
            return result;
        }

        /// <summary>Builds a <c>Matrix</c> from a matrix and a vector</summary>
        /// <param name="model">the 2d-array of data</param>
        /// <param name="v">Vector v</param>
        public static Matrix Create(Matrix model, Vector v)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Matrix result = new Matrix(model.Rows, model.Columns + 1, 0);

            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns - 1; j++)
                    result[i, j] = model[i, j];

            int p = result.Columns - 1;
            for (int i = 0; i < result.Rows; i++) result[i, p] = v[i];

            return result;
        }

        /// <summary>Builds a <c>Matrix</c> from a collection of vector</summary>
        /// <param name="model">the collection of vector</param>
        public static Matrix Create(IReadOnlyList<Vector> model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            int N = model.Count, M = model.First().Size;
            Matrix result = new Matrix(M, N, 0);

            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    result[i, j] = model[j][i];

            return result;
        }

        /// <summary>Creates an empty rectangular Matrix </summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="cols">the number of columns</param>
        /// <returns>a Matrix</returns>
        public static Matrix Create(int rows, int cols)
        {
            return new Matrix(rows, cols, 0);
        }

        /// <summary>Creates a rectangular Matrix </summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="cols">the number of cols</param>
        /// <param name="value">the value of the coefficients</param>
        /// <returns>a Matrix</returns>
        public static Matrix Create(int rows, int cols, double value)
        {
            return new Matrix(rows, cols, value);
        }

        /// <summary>Creates an empty square Matrix</summary>
        /// <param name="dimension">the Matrix's number of rows/columns</param>
        /// <returns>a Matrix</returns>
        public static Matrix CreateSquare(int dimension)
        {
            return new Matrix(dimension, dimension, 0);
        }

        /// <summary>Creates a 2x2 Matrix</summary>
        /// <returns>a Matrix</returns>
        public static Matrix Create()
        {
            return new Matrix(2, 2, 0);
        }

        /// <summary>Returns a matrix with ones on the diagonal of ones starting at the (0,0) element.
        /// When the matrix is squared, this is the identity matrix</summary>
        /// <param name="iRows">The number of rows of the output</param>
        /// <param name="iCols">The number of columns of the output</param>
        /// <returns>A  matrix with ones on the diagonal of ones starting at the (0,0) element</returns>
        public static Matrix CreateIdentityMatrix(int iRows, int iCols)
        {
            Matrix matrix = Create(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                matrix[i, i] = 1;
            return matrix;
        }

        /// <summary>Builds a square symmetric band-matrix</summary>
        /// <param name="size">the size of the matrix</param>
        /// <param name="values">the values of the diagonals and sub-diagonals</param>
        /// <returns>a square matrix</returns>
        public static Matrix CreateBandMatrix(int size, params double[] values)
        {
            Matrix result = Create(size, size);

            for (int i = 0; i < size; i++)
                for (int j = i; j < size; j++)
                {
                    int spread = Math.Abs(i - j);
                    if (spread < values.Length)
                    {
                        double value = values[spread];
                        result[i * size + j] = value;
                        result[j * size + i] = value;
                    }
                }

            return result;
        }

        /// <summary>Builds a square tridiagonal matrix</summary>
        /// <param name="size">the size of the matrix</param>
        /// <param name="diagonal">the value on the matrix's diagonal</param>
        /// <param name="upper">the value on the matrix's first super diagonal</param>
        /// <param name="lower">the value on the matrix's first sub diagonal</param>
        /// <returns>a square matrix</returns>
        public static Matrix CreateTridiagonalMatrix(int size, double diagonal, double upper, double lower)
        {
            Matrix result = Create(size, size);

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    int k = i * size + j;
                    if (i == j) result[k] = diagonal;
                    if (j == i - 1) result[k] = lower;
                    if (j == i + 1) result[k] = upper;
                }

            return result;
        }

        /// <summary>Creates a diagonal matrix</summary>
        /// <param name="values">the values to be set on the diagonal</param>
        /// <returns>a <c>Matrix</c> </returns>
        public static Matrix CreateDiagonalMatrix(params double[] values)
        {
            Matrix result = Create(values.Length, values.Length);
            for (int i = 0; i < values.Length; i++)
                result[i, i] = values[i];
            return result;
        }

        /// <summary>Returns a square matrix full of uniform random values</summary>
        /// <param name="size">the number of rows / cols</param>
        /// <returns>a square <c>Matrix</c></returns>
        public static Matrix CreateSquareRandom(int size)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            Matrix result = Matrix.CreateSquare(size);
            for (int i = 0; i < result.Size; i++)
                result[i] = rnd.NextDouble();
            return result;
        }

        /// <summary>Returns a rectangular matrix full of uniform random values</summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="columns">the number of columns</param>
        /// <returns>a rectangular matrix</returns>
        public static Matrix CreateRandom(int rows, int columns)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            Matrix result = Matrix.Create(rows, columns);
            for (int i = 0; i < result.Size; i++)
                result[i] = rnd.NextDouble();
            return result;
        }

        /// <summary>Returns a Matrix made of the given Vectors</summary>
        /// <param name="vectors">the Vectors</param>
        /// <returns>a Matrix</returns>
        public static Matrix CreateFromColumns(params Vector[] vectors)
        {
            #region verifications
            Vector reference = vectors.ElementAt(0);
            for (int i = 1; i < vectors.Length; i++)
                if (vectors.ElementAt(i).Size != reference.Size)
                    throw new ArgumentException("all the vectors do not have the same size");
            #endregion

            Matrix result = Matrix.Create(reference.Size, vectors.Length);
            for (int j = 0; j < result.Columns; j++)
            {
                Vector v = vectors.ElementAt(j);
                for (int i = 0; i < result.Rows; i++)
                    result[i, j] = v[i];
            }
            return result;
        }

        /// <summary>
        /// Create a matrix fills from a vector which is broadcasted to fit to the matrix shape
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="d">True (default), vector is row oriented else column</param>
        /// <param name="k">Targeting dimension to broadcast</param>
        /// <returns>Matrix broadcasted</returns>
        public static Matrix CreateByBroadcast(Vector vector, bool d, int k)
        {
            if (k <= 0) throw new Exception("Impossible to broadcast a dimension inferior or equals to 0");

            int N = vector.Size;
            Matrix result = d ? Create(N, k, 0) : Create(k, N, 0);

            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    result[i, j] = vector[d ? i : j];

            return result;
        }

        private const double _ACCURACY_ = 1e-9;
        #endregion

        #region Extensions
        /// <summary>Returns the product X^T . X</summary>
        /// <param name="X">the <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the product</returns>
        public static Matrix TransposeBySelf(Matrix X)
        {
            if (X == null) throw new ArgumentNullException(nameof(X));
            Matrix result = Matrix.CreateSquare(X.Columns);

            #region List the pairs
            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>(result.Rows * (result.Rows + 1) / 2);
            for (int i = 0; i < result.Rows; i++)
                for (int j = i; j < result.Columns; j++)
                    indexes.Add(new Tuple<int, int>(i, j));

            int numberOfPairs = indexes.Count;
            #endregion

            for (int l = 0; l < numberOfPairs; l++)
            {
                Tuple<int, int> index = indexes[l];

                int i = index.Item1, j = index.Item2;

                double value = 0;
                for (int k = 0; k < X.Rows; k++) value += X[k, i] * X[k, j];

                result[i, j] = value;
                result[j, i] = value;
            }

            return result;
        }

        /// <summary>Returns the product X^T . X using multithreading</summary>
        /// <param name="X">the <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the product</returns>
        public static Matrix FastTransposeBySelf(Matrix X)
        {
            if (X == null) throw new ArgumentNullException(nameof(X));

            Matrix result = Matrix.CreateSquare(X.Columns);

            #region List the pairs
            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>(result.Rows * (result.Rows + 1) / 2);
            for (int i = 0; i < result.Rows; i++)
                for (int j = i; j < result.Columns; j++)
                    indexes.Add(new Tuple<int, int>(i, j));

            int numberOfPairs = indexes.Count;
            #endregion

            Parallel.For(0, numberOfPairs, l =>
            {
                Tuple<int, int> index = indexes[l];
                int i = index.Item1, j = index.Item2;
                double value = 0;
                for (int k = 0; k < X._rows; k++) value += X._data[k * X._cols + i] * X._data[k * X._cols + j];

                result[i * result._cols + j] = value;
                result[j * result._cols + i] = value;
            });

            return result;
        }

        /// <summary>
        /// Estimate the covariance matrix
        /// </summary>
        /// <param name="X">Matrix of variables and observations</param>
        /// <param name="y">Additional set of variables and observations, y has the same form as X</param>
        /// <param name="rowvar">True each row represents a variable with observations in columns. Otherwise (default), the relationship is inversed, columns as variables and rows as observations</param>
        /// <param name="biais">False (default) normalization is done by dividing cov matrix by N - 1 else N. N is the number of observations</param>
        /// <param name="normalization">True (dafault) normalize each variables by computing its mean else native data matrix are used</param>
        /// <returns>Covariance matrix</returns>
        public static Matrix Cov(Matrix X, Vector y = null, bool rowvar = false, bool biais = false, bool normalization = true)
        {
            if (X == null) throw new Exception("X is null");
            if (y != null && (y.Size != X.Columns && y.Size != X.Rows)) throw new Exception($"y is not as the same form of X: y[{y.Size}] X[{X.Rows},{X.Columns}]");

            int ddof = biais ? 0 : 1;
            Matrix D = rowvar ? X.FastTranspose : X;
            
            if(y != null) D = Create(D, y);

            #region normalization
            if(normalization)
            {
                for(int j = 0; j < D.Columns; j++)
                {
                    double mj = 0;
                    for (int i = 0; i < D.Rows; i++) mj += D[i, j];
                    mj /= D.Rows;

                    for (int i = 0; i < D.Rows; i++) D[i, j] -= mj;
                }
            }
            #endregion

            #region calculation
            int fact = D.Rows - ddof;
            Matrix c = FastTransposeBySelf(D);
            c /= fact;
            #endregion

            return c;
        }

        /// <summary>
        /// Estimate the correlation matrix
        /// </summary>
        /// <param name="X">Matrix of variables and observations</param>
        /// <param name="y">Additional set of variables and observations, y has the same form as X</param>
        /// <param name="rowvar">True each row represents a variable with observations in columns. Otherwise (default), the relationship is inversed, columns as variables and rows as observations</param>
        /// <param name="biais">False (default) normalization is done by dividing cov matrix by N - 1 else N. N is the number of observations</param>
        /// <param name="normalization">True (dafault) normalize each variables by computing its mean else native data matrix are used</param>
        /// <returns>Correlation matrix</returns>
        public static Matrix Corr(Matrix X, Vector y = null, bool rowvar = false, bool biais = false, bool normalization = true)
        {
            Matrix c = Cov(X, y, rowvar, biais, normalization);
            Vector d = Diag(c);
            
            Vector s = d.Sqrt;

            Matrix s1 = CreateByBroadcast(s, true, s.Size), s2 = CreateByBroadcast(s, false, s.Size);
            Matrix corr = c / s1;
            corr /= s2;

            //for(int i = 0; i < c.Rows; i++)
            //    for(int j = 0; j < c.Columns; j++)
            //        c[i, j] /= s[i];


           
            return corr;
        }

        /// <summary>
        /// Extract/Build a diagonal
        /// </summary>
        /// <param name="m">Matrix</param>
        /// <param name="k">k = 0 (default) return the main diagonal. Otherwise if k>0 for diagonals above the main one and k<0 for diagonal below the main diagonal</param>
        /// <returns>Vector of diagonal</returns>
        public static Vector Diag(Matrix m, int k = 0)
        {
            if (k < 0 && Math.Abs(k) > m.Rows) throw new Exception($"k {k} has to be inferior to the nb of row the matrix {m.Rows}");
            if (k > 0 && Math.Abs(k) < m.Columns) throw new Exception($"k {k} has to be inferior to the nb of column the matrix {m.Columns}");

            List<double> candidats = new List<double>();

            int n = k >= 0 ? 0 : m.Rows + k, p = k <= 0 ? 0 : m.Columns - k, nb = n;

            for (int j = p; j < m.Columns; j++)
                for (int i = nb; i < m.Rows; i++)
                {
                    candidats.Add(m[i, j]);
                    nb++;
                    break;
                }
                
            return Vector.Create(candidats);
        }

        /// <summary>
        /// Compare two matrix in order to know if ones are element-wise equal within a tolerance
        /// </summary>
        /// <param name="m1">Input matrix left</param>
        /// <param name="m2">Input matrix right</param>
        /// <param name="epsilon">Tolerance</param>
        /// <returns>True if success else false</returns>
        public static bool AllClose(Matrix m1, Matrix m2, double epsilon = 1e-5)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns) throw new Exception($"Wrong dimension of row of matrix m1[{m1.Rows},{m1.Columns}] m2[{m2.Rows},{m2.Columns}]");

            for (int i = 0; i < m1.Rows; i++)
                for (int j = 0; j < m1.Columns; j++)
                    if (Math.Abs(m1[i, j] - m2[i, j]) > epsilon)
                        return false;

            return true;
        }

        /// <summary>
        /// Swicth columns values 
        /// </summary>
        /// <param name="m">Matrix</param>
        /// <param name="from">Targeting column</param>
        /// <param name="to">Destination column</param>
        /// <returns>result</returns>
        public static Matrix SwitchColumns(Matrix m, int from, int to)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Matrix result = Matrix.Create(m._rows, m._cols);
            Parallel.For(0, result.Columns, k => 
            {
                if(k == from || k == to) return;
                for (int i = 0; i < m.Rows; i++) result[i, k] = m[i, k];
            });

            for(int i  = 0; i < m.Rows; i++)
            {
                result[i, to] = m[i, from];
                result[i, from] = m[i, to];
            }

            return result;
        }

        /// <summary>
        /// Extract a sub matrix between row and column range
        /// </summary>
        /// <param name="m">Matrix</param>
        /// <param name="rowIdx">Starting row idx for extracting</param>
        /// <param name="nbRows">Nb of rows to extract from row idx</param>
        /// <param name="columnIdx">Starting columnn idx for extracting</param>
        /// <param name="nbColumns">Nb of columns to extract from column idx</param>
        /// <returns>Sub matrix</returns>
        public static Matrix ExtractSubMatrix(Matrix m, int rowIdx, int nbRows, int columnIdx, int nbColumns)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            int N = rowIdx + nbRows, M = columnIdx + nbColumns;
            if(N > m.Rows) throw new Exception($"can extract a matrix larger than the original matrix rows = {m.Rows} < {N}");
            if (N > m.Rows) throw new Exception($"can extract a matrix larger than the original matrix columns = {m.Columns} < {M}");
            //if (m.Rows < rowIdx) throw new Exception($"can starting row extraction at a higher rank than available #rows {m.Rows} < {rowIdx}");
            //if (m.Columns < columnIdx) throw new Exception($"can starting column extraction at a higher rank than available #columns {m.Columns} < {columnIdx}");
            //if (m.Rows < nbRows) throw new Exception($"can extract a matrix larger than the original matrix rows = {m.Rows} < {nbRows}");
            //if (m.Columns < nbColumns) throw new Exception($"can extract a matrix larger than the original matrix columns = {m.Columns} < {nbColumns}");

            Matrix result = Matrix.Create(N, M);

            for (int i = rowIdx; i < N; i++)
                for (int j = columnIdx; j < M; j++)
                    result[i - rowIdx, j - columnIdx] = m[i, j];

            return result;
        }

        /// <summary>Applies a function to transform the data of the matrix</summary>
        /// <param name="m">the matrix to transform</param>
        /// <param name="func">the transforming function</param>
        /// <returns>a <c>Matrix</c></returns>
        public static Matrix Apply(Matrix m, Func<double, double> func)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Matrix result = Matrix.Create(m._rows, m._cols);
            Parallel.For(0, result.Size, k => { result[k] = func(m._data[k]); });
            return result;
        }

        /// <summary>Returns the Hadamard product</summary>
        /// <param name="m1">the left hand side</param>
        /// <param name="m2">the right hand side</param>
        /// <returns>a <c>Matrix</c> containing the Hadamard product</returns>
        public static Matrix Hadamard(Matrix m1, Matrix m2)
        {
            if (m1.Rows == m2.Rows && m1.Columns == m2.Columns)
            {
                Matrix result = new Matrix(m1.Rows, m2.Columns, 0);
                for (int k = 0; k < m1.Size; k++)
                    result[k] = m1[k] * m2[k];
                return result;
            }
            throw new ArgumentException("The Hadamard product of two matrices can only be performed if they are the same size");
        }

        /// <summary>Returns the scalar product of the matrices</summary>
        /// <param name="m1">the left hand side</param>
        /// <param name="m2">the right hand side</param>
        /// <returns>a double value</returns>
        public static double Scalar(Matrix m1, Matrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));

            if (m1.Rows == m2.Rows && m1.Columns == m2.Columns)
            {
                double result = 0;
                for (int k = 0; k < m1.Size; k++)
                    result += m1[k] * m2[k];
                return result;
            }
            throw new ArgumentException("The scalar product of two matrices can only be performed if they are the same size");
        }

        /// <summary>Returns the point-to-point maximum between two matrices</summary>
        /// <param name="m1">the left hand side</param>
        /// <param name="m2">the right hand side</param>
        /// <returns>a <c>Matrix</c></returns>
        public static Matrix Max(Matrix m1, Matrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns) throw new ArgumentException("Matrices must have the same dimensions!");
            Matrix r = Matrix.Create(m1.Rows, m1.Columns);
            for (int k = 0; k < r.Size; k++)
                r[k] = Math.Max(m1[k], m2[k]);
            return r;
        }

        #endregion

        #region Interface Implementations
        /// <summary>Determines whether the specified object is equal to the current object</summary>
        /// <param name="other">the object to compare with the current object</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c></returns>
        public bool Equals(Matrix other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other is null) return false;
            if (_cols != other._cols || _rows != other._rows)
                return false;

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
                return true;

            // If all else fails, perform elementwise comparison.
            for (int i = 0; i < _rows * _cols; i++)
                if (_data[i] != other._data[i])
                    return false;

            return true;
        }

        /// <summary>Returns a string that represents the matrix</summary>
        /// <returns>a string that represents the matrix</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    sb.Append(this[i, j].ToString());
                    if (j != Columns - 1)
                        sb.Append(";");
                }
                if (i != _rows - 1)
                    sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
        #endregion
    }
}

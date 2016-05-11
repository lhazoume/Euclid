﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Euclid
{
    public sealed class Matrix
    {
        #region Declarations

        private int _rows, _cols;
        private double[] _data;
        private Matrix _L, _U;
        private int[] _pi;
        private double _detOfP = 1;

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a 2x2 <c>Matrix</c>
        /// </summary>
        public Matrix()
            : this(2)
        { }

        /// <summary>
        /// Builds a square matrix full of zeros
        /// </summary>
        /// <param name="dimension">the number of rows or columns</param>
        public Matrix(int dimension)
            : this(dimension, dimension)
        {
        }

        /// <summary>
        /// Builds a rectangular matrix full of zeros
        /// </summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="cols">the number of columns</param>
        public Matrix(int rows, int cols)
            : this(rows, cols, 0)
        { }

        /// <summary>
        /// Builds a rectangular matrix filled with the specified value
        /// </summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="cols">the number of columns</param>
        /// <param name="value">the value for all the fields of the matrix</param>
        public Matrix(int rows, int cols, double value)
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
        /// <summary>
        /// Returns the number of columns of the <c>Matrix</c>
        /// </summary>
        public int Columns
        {
            get { return _cols; }
        }

        /// <summary>
        /// Returns the number of rows of the <c>Matrix</c>
        /// </summary>
        public int Rows
        {
            get { return _rows; }
        }

        /// <summary>
        /// Specifies whether the <c>Matrix</c> is square
        /// </summary>
        public bool IsSquare
        {
            get { return (_rows == _cols); }
        }

        /// <summary>
        /// Specifies whether the <c>Matrix</c> is square and symmetric
        /// </summary>
        public bool IsSymetric
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

        /// <summary>
        /// Returns the number of values in the <c>Matrix</c>
        /// </summary>
        public int Size
        {
            get { return _data.Length; }
        }

        public double[] Data
        {
            get { return _data; }
        }
        public double this[int i, int j]
        {
            get
            {
                RangeCheck(i, j);
                return _data[i * _cols + j];
            }
            set
            {
                RangeCheck(i, j);
                _data[i * _cols + j] = value;
            }
        }
        public double this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        /// <summary>
        /// Returns a deep copy of the <c>Matrix</c>
        /// </summary>
        public Matrix Clone
        {
            get
            {
                Matrix result = new Matrix(_rows, _cols);
                for (int i = 0; i < _rows * _cols; i++)
                    result._data[i] = _data[i];
                return result;
            }
        }

        #region Inversion

        public Matrix L
        {
            get
            {
                if (_L == null) MakeLU();
                return _L;
            }
        }
        public Matrix U
        {
            get
            {
                if (_U == null) MakeLU();
                return _U;
            }
        }
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
        public Matrix Inverse
        {
            get
            {
                try { if (_L == null) MakeLU(); }
                catch { return null; }

                Matrix inv = new Matrix(_rows, _cols);
                for (int i = 0; i < _rows; i++)
                {
                    Matrix Ei = Matrix.ZeroMatrix(_rows, 1);
                    Ei[i] = 1;
                    Matrix col = SolveWith(Ei);
                    inv.SetCol(col, i);
                }
                return inv;
            }
        }
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

                Matrix result = new Matrix(_rows, _rows);
                Parallel.For(0, _rows * _rows, l =>
                {
                    int i = l / _rows, j = l % _rows;
                    result[i * _rows + j] = both[i, j + _rows];
                });
                return result;
            }
        }
        public Matrix CoMatrix
        {
            get
            {
                Matrix m2 = new Matrix(_rows, _cols, 0),
                    ret = new Matrix(_rows, _cols, 0);
                if (_rows == 1)
                    ret[0, 0] = 1;
                else
                {
                    for (int i = 0; i < _rows; i++)
                        for (int j = 0; j < _rows; j++)
                        {
                            m2 = this.SubMatrix(i, j);
                            ret[i, j] = Expo(i + j) * m2.Determinant;
                        }
                }
                return ret;
            }
        }

        #endregion

        /// <summary>
        /// Returns the trace of the <c>Matrix</c>
        /// </summary>
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
        public Matrix Transpose
        {
            get
            {
                Matrix t = new Matrix(_cols, _rows);
                if (_rows == 1 || _cols == 1)
                    for (int k = 0; k < _data.Length; k++)
                        t._data[k] = _data[k];
                else
                    for (int k = 0; k < _data.Length; k++)
                        t._data[k] = _data[(k % _rows) * _cols + (k / _rows)];
                return t;
            }
        }
        public Matrix FastTranspose
        {
            get
            {
                Matrix t = new Matrix(_cols, _rows);
                Parallel.For(0, _rows * _cols, k => { t._data[k] = _data[(k % _rows) * _cols + (k / _rows)]; });
                return t;
            }
        }
        public Matrix SymmetricPart
        {
            get { return 0.5 * (this + this.Transpose); }
        }
        public Matrix AntiSymmetricPart
        {
            get { return 0.5 * (this - this.Transpose); }
        }

        #region Norms and sums

        /// <summary>
        /// Return the sum of the absolute values
        /// </summary>
        public double Norm1
        {
            get
            {
                double result = 0;
                for (int k = 0; k < _data.Length; k++)
                {
                    result += Math.Abs(_data[k]);
                }
                return result;
            }
        }

        /// <summary>
        /// Returns the square root of the sum of squares
        /// </summary>
        public double Norm2
        {
            get { return Math.Sqrt(this.SumOfSquares); }
        }

        /// <summary>
        /// Returns the largest value of the <c>Matrix</c> in absolute value
        /// </summary>
        public double NormSup
        {
            get
            {
                double result = 0;
                foreach (double x in _data) result = Math.Max(result, Math.Abs(x));
                return result;
            }
        }

        /// <summary>
        /// Returns the sum of the squared values
        /// </summary>
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
        /// Returns the sum of the values
        /// </summary>
        public double Sum
        {
            get
            {
                double result = 0;
                for (int k = 0; k < _data.Length; k++)
                {
                    result += _data[k];
                }
                return result;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Private methods

        #region Checks

        private void RowCheck(int row)
        {
            if (row < 0 || row >= _rows) throw new ArgumentOutOfRangeException("row");
        }

        private void ColumnCheck(int column)
        {
            if (column < 0 || column >= _cols) throw new ArgumentOutOfRangeException("column");
        }

        private void RangeCheck(int row, int column)
        {
            RowCheck(row);
            ColumnCheck(column);
        }

        #endregion

        #region Inversion services

        /// <summary>
        /// Evaluates the LU decomposition of the matrix and stores the results in the private attributes _L and _U.
        /// </summary>
        private void MakeLU()
        {
            if (!IsSquare) throw new Exception("The matrix is not square!");
            _L = IdentityMatrix(_rows, _cols);
            _U = this.Clone;

            _pi = new int[_rows];
            for (int i = 0; i < _rows; i++) _pi[i] = i;

            double p = 0;
            double pom2;
            int k0 = 0;
            int pom1 = 0;

            for (int k = 0; k < _cols - 1; k++)
            {
                p = 0;
                for (int i = k; i < _rows; i++)      // find the row with the biggest pivot
                {
                    if (Math.Abs(_U[i * _U._cols + k]) > p)
                    {
                        p = Math.Abs(_U[i * _U._cols + k]);
                        k0 = i;
                    }
                }
                if (p == 0) throw new Exception("The matrix is singular");

                pom1 = _pi[k];
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

        private static Matrix SubsForth(Matrix A, Matrix b)
        {
            int n = A.Rows;
            Matrix x = new Matrix(n, 1);

            for (int i = 0; i < n; i++)
            {
                x[i] = b[i, 0];
                for (int j = 0; j < i; j++) x[i] -= A[i * A._cols + j] * x[j];
                x[i] = x[i] / A[i * (A._cols + 1)];
            }
            return x;
        }

        private static Matrix SubsBack(Matrix A, Matrix b)
        {
            int n = A.Rows;
            Matrix x = new Matrix(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i] = b[i, 0];
                for (int j = n - 1; j > i; j--) x[i] -= A[i * A._cols + j] * x[j];
                x[i] = x[i] / A[i * (A._cols + 1)];
            }
            return x;
        }

        /// <summary>
        /// Returns the matrix from which one row and one column have been excluded (indexed by row and col).
        /// </summary>
        /// <param name="row">The index of the row to exclude</param>
        /// <param name="col">The index of the column to exclude</param>
        /// <returns>The matrix without the speficied line and the column</returns>
        private Matrix SubMatrix(int row, int col)
        {
            RangeCheck(row, col);
            int dim = _rows, ld = 0, cd = 0;
            Matrix sub = new Matrix(_rows - 1, _cols - 1, 0);
            for (int L = 0; L < dim; L++)
            {
                cd = 0;
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

        /// <summary>
        /// Replaces the column of index k with the input matrix v.
        /// </summary>
        /// <param name="v">Column replacing the old one</param>
        /// <param name="k">Index of the column to replace</param>
        public void SetCol(Matrix v, int k)
        {
            for (int i = 0; i < _rows; i++) _data[i * _cols + k] = v[i * v._cols];
        }

        /// <summary>
        /// Solves the equation : A*x=v, where A is the Matrix, x the unknown, v the input argument.
        /// </summary>
        /// <param name="v">The right hand side of the equation</param>
        /// <returns>The solution x of A*x=v</returns>
        public Matrix SolveWith(Matrix v)
        {
            if (_rows != _cols) throw new Exception("The matrix is not square!");
            if (_rows != v.Rows) throw new Exception("Wrong number of results in solution vector!");
            if (_L == null) MakeLU();

            Matrix b = new Matrix(_rows, 1);
            for (int i = 0; i < _rows; i++) b[i] = v[_pi[i], 0];   // switch two items in "v" due to permutation matrix

            Matrix z = SubsForth(_L, b);
            Matrix x = SubsBack(_U, z);

            return x;
        }

        #endregion

        private static double Expo(int n)
        {
            return n % 2 != 0 ? 1 : -1;
        }

        #endregion

        #region Extract row or column

        public Matrix Column(int j)
        {
            ColumnCheck(j);
            Matrix result = new Matrix(_rows, 1);
            for (int i = 0; i < _rows; i++)
                result[i] = _data[i * _cols + j];
            return result;
        }

        public Matrix Row(int i)
        {
            RowCheck(i);
            Matrix result = new Matrix(1, _cols);
            for (int j = 0; j < _cols; j++)
                result[j] = _data[i * _cols + j];
            return result;
        }

        #endregion

        #endregion

        #region Operators

        #region Multiplications / divisions

        public static Matrix operator *(Matrix m, double f)
        {
            Matrix tmp = m.Clone;
            for (int k = 0; k < m.Size; k++)
                tmp[k] = f * m[k];
            return tmp;
        }
        public static Matrix operator *(double f, Matrix m)
        {
            return m * f;
        }
        public static Matrix operator /(Matrix m, double f)
        {
            return m * (1 / f);
        }
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.Columns != m2.Rows) throw new Exception("Wrong dimensions of matrix!");

            Matrix result = ZeroMatrix(m1.Rows, m2.Columns);
            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    for (int k = 0; k < m1.Columns; k++)
                        result._data[i * result._cols + j] += m1._data[i * m1._cols + k] * m2._data[k * m2._cols + j];
            return result;
        }
        public static Matrix operator ^(Matrix m1, Matrix m2)
        {
            if (m1.Columns != m2.Rows) throw new Exception("Wrong dimensions of matrix!");

            int n = m2._cols,
                p = m1._cols;

            Matrix result = new Matrix(m1._rows, n);
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

        #region Implicit Conversions

        public static implicit operator Matrix(List<double> model)
        {
            Matrix result = new Matrix(model.Count, 1);
            for (int i = 0; i < model.Count; i++) result[i, 0] = model[i];
            return result;
        }

        public static implicit operator Matrix(double[] model)
        {
            Matrix result = new Matrix(model.Length, 1);
            for (int i = 0; i < model.Length; i++) result[i] = model[i];
            return result;
        }

        public static implicit operator Matrix(double[,] model)
        {
            Matrix result = new Matrix(model.GetLength(0), model.GetLength(1));

            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    result[i, j] = model[i, j];
            return result;
        }

        #endregion

        #region Additions / substractions

        /// <summary>
        /// Performs a matrix addition, after going through dimension compatibility verifications.
        /// </summary>
        /// <param name="m1">First matrix</param>
        /// <param name="m2">Second matrix</param>
        /// <returns>The sum of m1 and m2</returns>
        private static Matrix Add(Matrix m1, Matrix m2)
        {
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns) throw new ArgumentException("Matrices must have the same dimensions!");
            Matrix r = new Matrix(m1.Rows, m1.Columns);
            for (int k = 0; k < r.Size; k++)
                r[k] = m1[k] + m2[k];
            return r;
        }

        public static Matrix operator +(Matrix m, double c)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] + c;
            return tmp;
        }
        public static Matrix operator +(double c, Matrix m)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] + c;
            return tmp;
        }
        public static Matrix operator -(Matrix m, double c)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] - c;
            return tmp;
        }
        public static Matrix operator -(double c, Matrix m)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] - c;
            return tmp;
        }

        public static Matrix operator -(Matrix m)
        {
            return m * -1;
        }
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, m2);
        }
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, -m2);
        }

        #endregion

        /// <summary>
        /// Evaluates the matrix raised to a power specified by pow.
        /// </summary>
        /// <param name="pow">The power we want to raise the matrix to</param>
        /// <returns>The matrix, raised to the power pow</returns>
        public static Matrix Power(Matrix matrix, int pow)
        {
            if (pow == 0) return IdentityMatrix(matrix._rows, matrix._cols);
            if (pow == 1) return matrix.Clone;
            if (pow < 0) return Power(matrix.Inverse, -pow);

            Matrix x = matrix.Clone;

            Matrix ret = IdentityMatrix(matrix._rows, matrix._cols);
            while (pow != 0)
            {
                if ((pow & 1) == 1) ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        #endregion

        #region Static Matrices

        /// <summary>
        /// Returns a matrix filled with zeroes.
        /// </summary>
        /// <param name="iRows">The number of rows of the output</param>
        /// <param name="iCols">The number of columns of the output</param>
        /// <returns>A matrix filled with zeroes</returns>
        public static Matrix ZeroMatrix(int iRows, int iCols)
        {
            return new Matrix(iRows, iCols, 0);
        }

        /// <summary>
        /// Returns a matrix with ones on the diagonal of ones starting at the (0,0) element.
        /// When the matrix is squared, this is the identity matrix.
        /// </summary>
        /// <param name="iRows">The number of rows of the output</param>
        /// <param name="iCols">The number of columns of the output</param>
        /// <returns>A  matrix with ones on the diagonal of ones starting at the (0,0) element</returns>
        public static Matrix IdentityMatrix(int iRows, int iCols)
        {
            Matrix matrix = ZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                matrix[i, i] = 1;
            return matrix;
        }

        public static Matrix BandMatrix(int size, params double[] values)
        {
            Matrix result = new Matrix(size, size);

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

        public static Matrix RandomMatrix(int size)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            Matrix result = new Matrix(size);
            for (int i = 0; i < result.Size; i++)
                result[i] = rnd.NextDouble();
            return result;
        }

        public static Matrix RandomMatrix(int rows, int columns)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            Matrix result = new Matrix(rows, columns);
            for (int i = 0; i < result.Size; i++)
                result[i] = rnd.NextDouble();
            return result;
        }

        private const double _ACCURACY_ = 1e-9;
        #endregion

        #region Extensions

        public static Matrix TransposeBySelf(Matrix X)
        {
            Matrix result = new Matrix(X.Columns);

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

        public static Matrix FastTransposeBySelf(Matrix X)
        {
            Matrix result = new Matrix(X.Columns);

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

        public static Matrix Apply(Matrix m, Func<double, double> func)
        {
            Matrix result = new Matrix(m._rows, m._cols);
            Parallel.For(0, result.Size, k => { result[k] = func(m._data[k]); });
            return result;
        }

        /// <summary>
        /// Returns the Hadamard product
        /// </summary>
        /// <param name="m1">the left hand side</param>
        /// <param name="m2">the right hand side</param>
        /// <returns>a <c>Matrix</c> containing the Hadamard product</returns>
        private static Matrix Hadamard(Matrix m1, Matrix m2)
        {
            if (m1.Rows == m2.Rows && m1.Columns == m2.Columns)
            {
                Matrix result = new Matrix(m1.Rows, m2.Columns);
                for (int k = 0; k < m1.Size; k++)
                    result[k] = m1[k] * m2[k];
                return result;
            }
            throw new ArgumentException("The Hadamard product of two matrices can only be performed if they are the same size");
        }

        public static double Scalar(Matrix m1, Matrix m2)
        {
            if (m1.Rows == m2.Rows && m1.Columns == m2.Columns)
            {
                double result = 0;
                for (int k = 0; k < m1.Size; k++)
                    result += m1[k] * m2[k];
                return result;
            }
            throw new ArgumentException("The scalar product of two matrices can only be performed if they are the same size");
        }

        #endregion

        #region Interface Implementations

        public bool Equals(Matrix other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null)
                return false;
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
        /// <summary>
        /// Returns a string that represents the matrix
        /// </summary>
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

using System;
using System.Collections.Generic;
using System.Linq;
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

        private int _rows, _cols;
        private double[] _data;
        private Matrix _L, _U;
        private int[] _pi;
        private double _detOfP = 1;

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a rectangular matrix filled with the specified value
        /// </summary>
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
        /// <summary>
        /// Returns the number of columns of the <c>Matrix</c>
        /// </summary>
        public virtual int Columns
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

        /// <summary>
        /// Returns the data of the matrix as a array of double with the rows one after the other
        /// </summary>
        public double[] Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Allows reading and modifying the coefficients of the <c>Matrix</c>
        /// </summary>
        /// <param name="i">the row</param>
        /// <param name="j">the column</param>
        /// <returns>a double value</returns>
        public double this[int i, int j]
        {
            get { return _data[i * _cols + j]; }
            set { _data[i * _cols + j] = value; }
        }

        /// <summary>
        /// Allows reading and modifying the coefficients of the <c>Matrix</c>
        /// </summary>
        /// <param name="i">the coefficient index</param>
        /// <returns>a double value</returns>
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
                Matrix result = new Matrix(_rows, _cols, 0);
                for (int i = 0; i < _rows * _cols; i++)
                    result._data[i] = _data[i];
                return result;
            }
        }

        #region Inversion
        /// <summary>
        /// The Lower triangular part of the LU decomposition
        /// </summary>
        public Matrix L
        {
            get
            {
                if (_L == null) MakeLU();
                return _L;
            }
        }

        /// <summary>
        /// The Upper triangular part of the LU decomposition
        /// </summary>
        public Matrix U
        {
            get
            {
                if (_U == null) MakeLU();
                return _U;
            }
        }

        /// <summary>
        /// Returns the determinant of the <c>Matrix</c>
        /// </summary>
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

        /// <summary>
        /// Returns the inverse of the <c>Matrix</c> when possible, null otherwise
        /// </summary>
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

        /// <summary>
        /// Returns the inverse of the <c>Matrix</c> when possible, null otherwise. Done using multithreading !
        /// </summary>
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

        /// <summary>
        /// Returns the co-matrix of the current matrix
        /// </summary>
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

        /// <summary>
        /// Returns the transposed <c>Matrix</c>
        /// </summary>
        public Matrix Transpose
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

        /// <summary>
        /// Returns the transposed <c>Matrix</c> filled using multithreading
        /// </summary>
        public Matrix FastTranspose
        {
            get
            {
                Matrix t = Matrix.Create(_cols, _rows);
                Parallel.For(0, _rows * _cols, k => { t._data[k] = _data[(k % _rows) * _cols + (k / _rows)]; });
                return t;
            }
        }

        /// <summary>
        /// Returns the symmetric part of the <c>Matrix</c> ( 1/2 . (A^T + A))
        /// </summary>
        public Matrix SymmetricPart
        {
            get { return 0.5 * (this + this.Transpose); }
        }

        /// <summary>
        /// Returns the symmetric part of the <c>Matrix</c> (1/2 . (A - A^T))
        /// </summary>
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

        #region Inversion services

        /// <summary>
        /// Evaluates the LU decomposition of the matrix and stores the results in the private attributes _L and _U.
        /// </summary>
        private void MakeLU()
        {
            if (!IsSquare) throw new Exception("The matrix is not square!");
            _L = CreateIdentityMatrix(_rows, _cols);
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

        /// <summary>
        /// Returns the matrix from which one row and one column have been excluded (indexed by row and col).
        /// </summary>
        /// <param name="row">The index of the row to exclude</param>
        /// <param name="col">The index of the column to exclude</param>
        /// <returns>The matrix without the speficied line and the column</returns>
        private Matrix SubMatrix(int row, int col)
        {
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
        public void SetCol(Vector v, int k)
        {
            for (int i = 0; i < _rows; i++) _data[i * _cols + k] = v[i];
        }

        /// <summary>
        /// Solves the equation : A*x=v, where A is the Matrix, x the unknown, v the input argument.
        /// </summary>
        /// <param name="v">The right hand side of the equation</param>
        /// <returns>The solution x of A*x=v</returns>
        public Vector SolveWith(Vector v)
        {
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

        /// <summary>
        /// Extracts a specific column
        /// </summary>
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
        /// Extracts a specific row
        /// </summary>
        /// <param name="i">the specified row</param>
        /// <returns>a row Vector</returns>
        public Vector Row(int i)
        {
            Vector result = Vector.Create(_cols);
            for (int j = 0; j < _cols; j++)
                result[j] = _data[i * _cols + j];
            return result;
        }

        #endregion

        #endregion

        #region Operators

        #region Multiplications / divisions

        /// <summary>
        /// Multiplies a <c>Matrix</c> by a scalar
        /// </summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(Matrix m, double f)
        {
            Matrix tmp = m.Clone;
            for (int k = 0; k < m.Size; k++)
                tmp[k] = f * m[k];
            return tmp;
        }

        /// <summary>
        /// Multiplies a <c>Matrix</c> by a scalar
        /// </summary>
        /// <param name="f">the scalar</param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(double f, Matrix m)
        {
            return m * f;
        }

        /// <summary>
        /// Divides all the coefficients of a <c>Matrix</c> by a scalar
        /// </summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the division</returns>
        public static Matrix operator /(Matrix m, double f)
        {
            return m * (1 / f);
        }

        /// <summary>
        /// Multiplies two matrices
        /// </summary>
        /// <param name="m1">the left hand side <c>Matrix</c></param>
        /// <param name="m2">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.Columns != m2.Rows) throw new Exception("Wrong dimensions of matrix!");

            Matrix result = CreateZeroMatrix(m1.Rows, m2.Columns);
            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    for (int k = 0; k < m1.Columns; k++)
                        result._data[i * result._cols + j] += m1._data[i * m1._cols + k] * m2._data[k * m2._cols + j];
            return result;
        }

        /// <summary>
        /// Multiplies two matrices using multithreading
        /// </summary>
        /// <param name="m1">the left hand side <c>Matrix</c></param>
        /// <param name="m2">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator ^(Matrix m1, Matrix m2)
        {
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

        /// <summary>
        /// Performs a matrix addition, after going through dimension compatibility verifications.
        /// </summary>
        /// <param name="m1">First matrix</param>
        /// <param name="m2">Second matrix</param>
        /// <returns>The sum of m1 and m2</returns>
        private static Matrix Add(Matrix m1, Matrix m2)
        {
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
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns) throw new ArgumentException("Matrices must have the same dimensions!");
            Matrix r = Matrix.Create(m1.Rows, m1.Columns);
            for (int k = 0; k < r.Size; k++)
                r[k] = f1 * m1[k] + f2 * m2[k];
            return r;
        }

        /// <summary>
        /// Adds a scalar to all the coefficients of a <c>Matrix</c>
        /// </summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the addition</returns>
        public static Matrix operator +(Matrix m, double c)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] + c;
            return tmp;
        }

        /// <summary>
        /// Adds a scalar to all the coefficients of a <c>Matrix</c>
        /// </summary>
        /// <param name="c">the scalar</param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the addition</returns>
        public static Matrix operator +(double c, Matrix m)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] + c;
            return tmp;
        }

        /// <summary>
        /// Substracts a scalar to all the coefficients of a <c>Matrix</c>
        /// </summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Matrix</c> result of the substraction</returns>
        public static Matrix operator -(Matrix m, double c)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] - c;
            return tmp;
        }

        /// <summary>
        /// Adds a scalar to the opposite of a <c>Matrix</c>
        /// </summary>
        /// <param name="c">the scalar</param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the substraction</returns>
        public static Matrix operator -(double c, Matrix m)
        {
            Matrix tmp = m.Clone;
            for (int i = 0; i < m.Size; i++)
                tmp[i] = m[i] - c;
            return tmp;
        }

        /// <summary>
        /// Returns the opposite of the <c>Matrix</c>
        /// </summary>
        /// <param name="m">the input matrix</param>
        /// <returns>the <c>Matrix</c> opposite</returns>
        public static Matrix operator -(Matrix m)
        {
            return m * -1;
        }

        /// <summary>
        /// Performs the matrix addition
        /// </summary>
        /// <param name="m1">the left hand side matrix</param>
        /// <param name="m2">the right hand side matrix</param>
        /// <returns>a <c>Matrix</c></returns>
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, m2);
        }

        /// <summary>
        /// Performs a matrix substraction
        /// </summary>
        /// <param name="m1">the left hand side</param>
        /// <param name="m2">the right hand side</param>
        /// <returns>the <c>Matrix</c> result of the substraction</returns>
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, -m2);
        }

        #endregion

        /// <summary>
        /// Evaluates the matrix raised to a power specified by pow.
        /// </summary>
        /// <param name="matrix">the matrix</param>
        /// <param name="pow">The power to raise the matrix to</param>
        /// <returns>The matrix, raised to the power pow</returns>
        public static Matrix Power(Matrix matrix, int pow)
        {
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

        /// <summary>
        /// Builds a <c>Matrix</c> from a 2d-array of double
        /// </summary>
        /// <param name="model">the 2d-array of data</param>
        public static Matrix Create(double[,] model)
        {
            Matrix result = new Matrix(model.GetLength(0), model.GetLength(1), 0);

            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    result[i, j] = model[i, j];
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

        /// <summary>Creates an empty square Matrix
        /// </summary>
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

        /// <summary>
        /// Returns a matrix filled with zeroes.
        /// </summary>
        /// <param name="iRows">The number of rows of the output</param>
        /// <param name="iCols">The number of columns of the output</param>
        /// <returns>A matrix filled with zeroes</returns>
        public static Matrix CreateZeroMatrix(int iRows, int iCols)
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
        public static Matrix CreateIdentityMatrix(int iRows, int iCols)
        {
            Matrix matrix = CreateZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                matrix[i, i] = 1;
            return matrix;
        }

        /// <summary>
        /// Builds a square symmetric band-matrix
        /// </summary>
        /// <param name="size">the size of the matrix</param>
        /// <param name="values">the values of the diagonals and sub-diagonals</param>
        /// <returns>a square matrix</returns>
        public static Matrix CreateBandMatrix(int size, params double[] values)
        {
            Matrix result = CreateZeroMatrix(size, size);

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

        /// <summary>
        /// Returns a square matrix full of uniform random values
        /// </summary>
        /// <param name="size">the number of rows / cols</param>
        /// <returns>a square <c>Matrix</c></returns>
        public static Matrix CreateSquareRandom(int size)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            Matrix result = Matrix.CreateSquare(size);
            for (int i = 0; i < result.Size; i++)
                result[i] = rnd.NextDouble();
            return result;
        }

        /// <summary>
        /// Returns a rectangular matrix full of uniform random values
        /// </summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="columns">the number of columns</param>
        /// <returns>a rectangular matrix</returns>
        public static Matrix CreateRandom(int rows, int columns)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            Matrix result = Matrix.Create(rows, columns);
            for (int i = 0; i < result.Size; i++)
                result[i] = rnd.NextDouble();
            return result;
        }

        /// <summary>Returns a Matrix made of the given Vectors</summary>
        /// <param name="vectors">the Vectors</param>
        /// <returns>a Matrix</returns>
        public static Matrix CreateFromColumns(IEnumerable<Vector> vectors)
        {
            #region Verifications
            Vector reference = vectors.ElementAt(0);
            for (int i = 1; i < vectors.Count(); i++)
                if (vectors.ElementAt(i).Size != reference.Size)
                    throw new ArgumentException("all the vectors do not have the same size");
            #endregion
            Matrix result = Matrix.Create(reference.Size, vectors.Count());
            for (int j = 0; j < result.Columns; j++)
            {
                Vector v = vectors.ElementAt(j);
                for (int i = 0; i < result.Rows; i++)
                    result[i, j] = v[i];
            }
            return result;
        }

        private const double _ACCURACY_ = 1e-9;
        #endregion

        #region Extensions
        /// <summary>
        /// Returns the product X^T . X
        /// </summary>
        /// <param name="X">the <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the product</returns>
        public static Matrix TransposeBySelf(Matrix X)
        {
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

        /// <summary>
        /// Returns the product X^T . X using multithreading
        /// </summary>
        /// <param name="X">the <c>Matrix</c></param>
        /// <returns>the <c>Matrix</c> result of the product</returns>
        public static Matrix FastTransposeBySelf(Matrix X)
        {
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
        /// Applies a function to transform the data of the matrix
        /// </summary>
        /// <param name="m">the matrix to transform</param>
        /// <param name="func">the transforming function</param>
        /// <returns>a <c>Matrix</c></returns>
        public static Matrix Apply(Matrix m, Func<double, double> func)
        {
            Matrix result = Matrix.Create(m._rows, m._cols);
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
                Matrix result = new Matrix(m1.Rows, m2.Columns, 0);
                for (int k = 0; k < m1.Size; k++)
                    result[k] = m1[k] * m2[k];
                return result;
            }
            throw new ArgumentException("The Hadamard product of two matrices can only be performed if they are the same size");
        }

        /// <summary>
        /// Returns the scalar product of the matrices
        /// </summary>
        /// <param name="m1">the left hand side</param>
        /// <param name="m2">the right hand side</param>
        /// <returns>a double value</returns>
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
        /// <summary>
        /// Determines whether the specified object is equal to the current object
        /// </summary>
        /// <param name="other">the object to compare with the current object</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c></returns>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euclid
{
    public class SparseMatrix
    {
        #region Declarations
        private readonly int _cols, _rows;
        private readonly Dictionary<int, Dictionary<int, double>> _data;
        private const double _ACCURACY_ = 1e-12;
        #endregion

        #region Constructors
        private SparseMatrix(int rows, int cols)
        {
            if (rows <= 0) throw new ArgumentException("No matrix can have less than one row");
            if (cols <= 0) throw new ArgumentException("No matrix can have less than one column");
            _rows = rows;
            _cols = cols;
            _data = new Dictionary<int, Dictionary<int, double>>();
        }
        #endregion

        #region Create Matrices

        public static SparseMatrix Create(int rows, int cols)
        {
            return new SparseMatrix(rows, cols);
        }
        public static SparseMatrix CreateIdentityMatrix(int rows, int cols)
        {
            if (rows != cols) throw new ArgumentException("Identity matrix must be square.");
            SparseMatrix matrix = new SparseMatrix(rows, cols);
            for (int i = 0; i < Math.Min(rows, cols); i++)
                matrix[i, i] = 1.0;
            return matrix;
        }
        #endregion

        #region Accessors
        public int Columns => _cols;
        public int Rows => _rows;
        public bool IsSquare => (_rows == _cols);
        public int CountNonZeros => _data.Sum(d => d.Value.Count);

        public double this[int i, int j]
        {
            get
            {
                if (i < 0 || i >= _rows || j < 0 || j >= _cols)
                    throw new IndexOutOfRangeException("Matrix indices are out of bounds.");
                return _data.TryGetValue(i, out Dictionary<int, double> row) && row.TryGetValue(j, out double value) ? value : 0.0;
            }
            set
            {
                if (i < 0 || i >= _rows || j < 0 || j >= _cols)
                    throw new IndexOutOfRangeException("Matrix indices are out of bounds.");

                if (Math.Abs(value) < _ACCURACY_)
                {
                    if (_data.TryGetValue(i, out Dictionary<int, double> row))
                    {
                        row.Remove(j);
                        if (row.Count == 0)
                        {
                            _data.Remove(i);
                        }
                    }
                }
                else
                {
                    if (!_data.TryGetValue(i, out Dictionary<int, double> row))
                    {
                        row = new Dictionary<int, double>();
                        _data[i] = row;
                    }
                    row[j] = value;
                }
            }
        }

        public SparseMatrix Clone
        {
            get
            {
                SparseMatrix result = new SparseMatrix(_rows, _cols);
                foreach (KeyValuePair<int, Dictionary<int, double>> rowEntry in _data)
                {
                    result._data[rowEntry.Key] = new Dictionary<int, double>(rowEntry.Value);
                }
                return result;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Solves the linear system Ax = b for x using the preconditioned BiConjugate Gradient Stabilized (BiCGSTAB) method.
        /// This is an iterative solver suitable for large, sparse, non-symmetric square matrices.
        /// </summary>
        /// <param name="b">The right-hand side vector.</param>
        /// <param name="maxIterations">The maximum number of iterations to perform.</param>
        /// <param name="tolerance">The convergence tolerance for the norm of the residual.</param>
        /// <returns>The solution vector x. May be an approximate solution if convergence is not reached.</returns>
        public Vector SolveWithBiCGSTAB(Vector b, int maxIterations = 1000, double tolerance = 1e-9)
        {
            #region Initial Validations
            if (!IsSquare) throw new InvalidOperationException("BiCGSTAB solver requires a square matrix.");
            if (b.Size != _rows) throw new ArgumentException("Vector dimension must match matrix dimension.");
            #endregion

            #region Algorithm Initialization
            Vector x = Vector.Create(_rows);
            Vector r = b.Clone;

            Vector r_hat = r.Clone;

            // Pre-calculate the inverse diagonal for preconditioning
            Vector diag_inv = Vector.Create(_rows);
            for (int i = 0; i < _rows; i++)
            {
                double diag_val = this[i, i];
                diag_inv[i] = (Math.Abs(diag_val) > _ACCURACY_) ? 1.0 / diag_val : 1.0;
            }

            double rho_prev = 1.0;
            double alpha = 1.0;
            double omega = 1.0;
            Vector p = Vector.Create(_rows);
            Vector v = Vector.Create(_rows);

            int iter = 0;
            double residualNorm = r.Norm2;
            #endregion

            #region Main BiCGSTAB Loop
            while (iter < maxIterations && residualNorm > tolerance)
            {
                double rho_curr = Vector.Scalar(r_hat, r);

                if (Math.Abs(rho_curr) < _ACCURACY_)
                {
                    break;
                }

                if (iter == 0)
                {
                    p = r.Clone;
                }
                else
                {
                    double beta = (rho_curr / rho_prev) * (alpha / omega);
                    p = r + beta * (p - omega * v);
                }

                // Preconditioning step
                Vector p_hat = Vector.Create(_rows);
                for (int i = 0; i < _rows; i++) { p_hat[i] = p[i] * diag_inv[i]; }

                v = this * p_hat;

                double r_hat_dot_v = Vector.Scalar(r_hat, v);
                if (Math.Abs(r_hat_dot_v) < _ACCURACY_)
                {
                    break;
                }
                alpha = rho_curr / r_hat_dot_v;

                Vector s = r - alpha * v;

                // Preconditioning step
                Vector s_hat = Vector.Create(_rows);
                for (int i = 0; i < _rows; i++) { s_hat[i] = s[i] * diag_inv[i]; }

                Vector t = this * s_hat;

                double t_dot_s = Vector.Scalar(t, s);
                double t_dot_t = Vector.Scalar(t, t);

                if (Math.Abs(t_dot_t) < _ACCURACY_)
                {
                    break;
                }
                omega = t_dot_s / t_dot_t;

                x += alpha * p_hat + omega * s_hat;
                r = s - omega * t;

                rho_prev = rho_curr;
                residualNorm = r.Norm2;
                iter++;
            }
            #endregion

            return x;
        }
       

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"SparseMatrix ({_rows}x{_cols}), {CountNonZeros} non-zero elements:");
            List<int> sortedRows = _data.Keys.ToList();
            sortedRows.Sort();
            foreach (int i in sortedRows)
            {
                List<int> sortedCols = _data[i].Keys.ToList();
                sortedCols.Sort();
                foreach (int j in sortedCols)
                {
                    sb.AppendLine($"  ({i}, {j}) -> {_data[i][j]}");
                }
            }
            return sb.ToString();
        }
        #endregion


        #region Operators

        #region Multiplications / divisions

        /// <summary>Multiplies a sparse matrix by a vector.</summary>
        /// <param name="m">The left hand side sparse matrix.</param>
        /// <param name="v">The right hand side vector.</param>
        /// <returns>The vector result of the multiplication.</returns>
        public static Vector operator *(SparseMatrix m, Vector v)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (v == null) throw new ArgumentNullException(nameof(v));
            if (m.Columns != v.Size) throw new ArgumentException("Matrix and vector dimensions are not compatible for multiplication.");

            Vector result = Vector.Create(m.Rows);
            foreach (KeyValuePair<int, Dictionary<int, double>> rowEntry in m._data)
            {
                int i = rowEntry.Key;
                double sum = 0;
                foreach (KeyValuePair<int, double> colEntry in rowEntry.Value)
                {
                    sum += colEntry.Value * v[colEntry.Key];
                }
                result[i] = sum;
            }
            return result;
        }

        /// <summary>Multiplies a <c>SparseMatrix</c> by a scalar.</summary>
        /// <param name="m">The left hand side <c>SparseMatrix</c>.</param>
        /// <param name="f">The scalar.</param>
        /// <returns>The <c>SparseMatrix</c> result of the multiplication.</returns>
        public static SparseMatrix operator *(SparseMatrix m, double f)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            if (Math.Abs(f) < _ACCURACY_)
                return new SparseMatrix(m.Rows, m.Columns);

            SparseMatrix result = m.Clone;

            foreach (Dictionary<int, double> row in result._data.Values)
            {
                List<int> keys = new List<int>(row.Keys);
                foreach (int j in keys)
                {
                    row[j] *= f;
                }
            }
            return result;
        }

        /// <summary>Multiplies a <c>SparseMatrix</c> by a scalar.</summary>
        /// <param name="f">The scalar.</param>
        /// <param name="m">The right hand side <c>SparseMatrix</c>.</param>
        /// <returns>The <c>SparseMatrix</c> result of the multiplication.</returns>
        public static SparseMatrix operator *(double f, SparseMatrix m)
        {
            return m * f;
        }

        /// <summary>Divides all the coefficients of a <c>SparseMatrix</c> by a scalar.</summary>
        /// <param name="m">The left hand side <c>SparseMatrix</c>.</param>
        /// <param name="f">The scalar.</param>
        /// <returns>The <c>SparseMatrix</c> result of the division.</returns>
        public static SparseMatrix operator /(SparseMatrix m, double f)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (Math.Abs(f) < _ACCURACY_)
                throw new DivideByZeroException("Cannot divide a sparse matrix by zero.");

            return m * (1.0 / f);
        }

        #endregion

        #region Additions / subtractions

        /// <summary>Performs a matrix addition or subtraction, optimized for sparse structures.</summary>
        /// <param name="m1">First matrix.</param>
        /// <param name="m2">Second matrix.</param>
        /// <param name="sign">1.0 for addition (m1 + m2), -1.0 for subtraction (m1 - m2).</param>
        /// <returns>The resulting sparse matrix.</returns>
        private static SparseMatrix Add(SparseMatrix m1, SparseMatrix m2, double sign)
        {
            if (m1.Rows != m2.Rows || m1.Columns != m2.Columns)
                throw new ArgumentException("Matrices must have the same dimensions.");

            SparseMatrix result = m1.Clone;

            foreach (KeyValuePair<int, Dictionary<int, double>> rowEntry2 in m2._data)
            {
                int i = rowEntry2.Key;
                Dictionary<int, double> row2 = rowEntry2.Value;

                if (!result._data.TryGetValue(i, out Dictionary<int, double> resultRow))
                {
                    // Case: Row i exists in m2 but not in m1. We must create it in the result
                    resultRow = new Dictionary<int, double>();
                    result._data[i] = resultRow;
                }

                foreach (KeyValuePair<int, double> colEntry2 in row2)
                {
                    int j = colEntry2.Key;

                    resultRow.TryGetValue(j, out double resultValue);

                    double newValue = resultValue + (colEntry2.Value * sign);

                    if (Math.Abs(newValue) < _ACCURACY_)
                        resultRow.Remove(j);
                    else
                        resultRow[j] = newValue;
                }
            }
            return result;
        }

        /// <summary>Returns the opposite of the <c>SparseMatrix</c>.</summary>
        /// <param name="m">The input matrix.</param>
        /// <returns>The <c>SparseMatrix</c> opposite.</returns>
        public static SparseMatrix operator -(SparseMatrix m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            return m * -1.0;
        }

        /// <summary>Performs the matrix addition.</summary>
        /// <param name="m1">The left hand side matrix.</param>
        /// <param name="m2">The right hand side matrix.</param>
        /// <returns>A <c>SparseMatrix</c>.</returns>
        public static SparseMatrix operator +(SparseMatrix m1, SparseMatrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));
            return Add(m1, m2, 1.0);
        }

        /// <summary>Performs a matrix subtraction.</summary>
        /// <param name="m1">The left hand side.</param>
        /// <param name="m2">The right hand side.</param>
        /// <returns>The <c>SparseMatrix</c> result of the subtraction.</returns>
        public static SparseMatrix operator -(SparseMatrix m1, SparseMatrix m2)
        {
            if (m1 == null) throw new ArgumentNullException(nameof(m1));
            if (m2 == null) throw new ArgumentNullException(nameof(m2));

            return Add(m1, m2, -1.0);
        }

        #endregion
    
        #endregion  
    }
}
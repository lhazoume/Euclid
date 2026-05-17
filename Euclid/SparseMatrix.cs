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

        /// <summary>Creates a matrix of size rows x cols</summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="cols">the number of columns</param>
        /// <returns>a <c>SparseMatrix</c></returns>
        public static SparseMatrix Create(int rows, int cols)
        {
            return new SparseMatrix(rows, cols);
        }

        /// <summary>Creates a square matrix of size n x n</summary>
        /// <param name="n">the size of the matrix</param>
        public static SparseMatrix Create(int n) => new SparseMatrix(n, n);

        /// <summary>Creates a matrix of size rows x cols with all coefficients initialized to the given value</summary>
        /// <param name="cols">the number of columns</param>
        /// <param name="rows">the number of rows</param>
        /// <param name="generator">a function that takes row and column indices and returns the value to set at that position</param>
        /// <returns>a <c>SparseMatrix</c></returns>
        public static SparseMatrix Create(int rows, int cols, Func<int, int, double> generator)
        {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            SparseMatrix m = new SparseMatrix(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    double v = generator(i, j);
                    if (Math.Abs(v) >= _ACCURACY_) m[i, j] = v;
                }
            return m;
        }

        /// <summary>Creates an identity matrix of size rows x cols</summary>
        /// <param name="cols">the number of columns</param>
        /// <param name="rows">the number of rows</param>
        /// <returns>a <c>SparseMatrix</c></returns>
        public static SparseMatrix CreateIdentityMatrix(int rows, int cols)
        {
            if (rows != cols) throw new ArgumentException("Identity matrix must be square.");
            SparseMatrix matrix = new SparseMatrix(rows, cols);
            for (int i = 0; i < Math.Min(rows, cols); i++)
                matrix[i, i] = 1.0;
            return matrix;
        }

        /// <summary>Creates an identity matrix of size n x n</summary>
        /// <param name="n">the size of the matrix</param>
        /// <returns>a <c>SparseMatrix</c></returns>
        public static SparseMatrix CreateIdentityMatrix(int n) => CreateIdentityMatrix(n, n);
        #endregion

        #region Accessors
        /// <summary>Returns the number of columns</summary>
        public int Columns => _cols;

        /// <summary>Returns the number of rows</summary>
        public int Rows => _rows;

        /// <summary>Returns <c>true</c> if the matrix is square, <c>false</c> otherwise</summary>
        public bool IsSquare => (_rows == _cols);

        /// <summary>Returns the number of non-zero coefficients in the matrix</summary>
        public int CountNonZeros => _data.Sum(d => d.Value.Count);

        /// <summary>Gets or sets the value at position (i, j). Setting a value close to zero will remove the entry from the sparse structure.</summary>
        /// <param name="i">the row index</param>
        /// <param name="j">the column index</param>
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

        /// <summary>Returns a deep copy of the matrix</summary>
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

        #region Solveurs
        /// <summary>
        /// Solves the linear system Ax = b for x using the preconditioned BiConjugate Gradient Stabilized (BiCGSTAB) method.
        /// This is an iterative solver suitable for large, sparse, non-symmetric square matrices.
        /// </summary>
        /// <param name="b">The right-hand side vector.</param>
        /// <param name="maxIterations">The maximum number of iterations to perform.</param>
        /// <param name="tolerance">The convergence tolerance for the norm of the residual.</param>
        /// <returns>The solution vector x. May be an approximate solution if convergence is not reached.</returns>

        public Vector SolveWithBiCGSTAB(Vector b, int maxIterations = 1000, double tolerance = 1e-5)
        {
            if (!IsSquare)
                throw new InvalidOperationException("BiCGSTAB solver requires a square matrix.");
            if (b.Size != _rows)
                throw new ArgumentException("Vector dimension must match matrix dimension.");

            // Préconditionneur ILU(0)
            SparseMatrix luFactors = CreateIlu0Factors();


            #region Initialisation de l'algorithme BiCGSTAB
            Vector solution = Vector.Create(_rows);
            Vector residual = b.Clone;
            Vector residualReference = residual.Clone;

            double rho_prev = 1.0;
            double alpha = 1.0;
            double omega = 1.0;

            Vector searchDirection = Vector.Create(_rows);
            Vector matrixProductResult = Vector.Create(_rows);
            int iter = 0;

            #endregion

            while (iter < maxIterations && residual.Norm2 > tolerance)
            {
                double rho_curr = Vector.Scalar(residualReference, residual);
                if (Math.Abs(rho_curr) < _ACCURACY_) break;

                if (iter == 0)
                    searchDirection = residual.Clone;
                else
                {
                    double beta = (rho_curr / rho_prev) * (alpha / omega);
                    searchDirection = residual + (searchDirection - matrixProductResult * omega) * beta;
                }

                Vector preconditionedSearchDirection = ApplyIlu0Preconditioner(luFactors, searchDirection);

                matrixProductResult = this * preconditionedSearchDirection;

                double residualRef_dot_matrixProduct = Vector.Scalar(residualReference, matrixProductResult);
                if (Math.Abs(residualRef_dot_matrixProduct) < _ACCURACY_) break;

                alpha = rho_curr / residualRef_dot_matrixProduct;

                Vector temporaryResidual = residual - matrixProductResult * alpha;

                Vector preconditionedTemporaryResidual = ApplyIlu0Preconditioner(luFactors, temporaryResidual);

                Vector secondMatrixProductResult = this * preconditionedTemporaryResidual;

                double t_dot_s = Vector.Scalar(secondMatrixProductResult, temporaryResidual);
                double t_dot_t = Vector.Scalar(secondMatrixProductResult, secondMatrixProductResult);
                if (Math.Abs(t_dot_t) < _ACCURACY_) break;

                omega = t_dot_s / t_dot_t;

                solution += preconditionedSearchDirection * alpha + preconditionedTemporaryResidual * omega;
                residual = temporaryResidual - secondMatrixProductResult * omega;

                rho_prev = rho_curr;
                iter++;
            }
         
            return solution;
        }
        private SparseMatrix CreateIlu0Factors()
        {
            SparseMatrix luFactors = this.Clone;

            for (int i = 0; i < _rows; i++)
            {
                if (luFactors._data.TryGetValue(i, out Dictionary<int, double> rowI))
                {
                    List<int> j_indices = rowI.Keys.ToList();
                    j_indices.Sort();

                    foreach (int j in j_indices)
                    {
                        if (j >= i) continue;

                        double diag_j = luFactors[j, j];
                        if (Math.Abs(diag_j) < _ACCURACY_) continue;

                        double factor = luFactors[i, j] / diag_j;
                        luFactors[i, j] = factor;

                        if (luFactors._data.TryGetValue(j, out Dictionary<int, double> rowJ))
                        {
                            foreach (KeyValuePair<int, double> colEntryK in rowJ)
                            {
                                int k = colEntryK.Key;
                                if (k > j && rowI.ContainsKey(k))
                                {
                                    luFactors[i, k] -= factor * colEntryK.Value;
                                }
                            }
                        }
                    }
                }
            }
            return luFactors;
        }

        /// <summary>
        /// Applies the ILU(0) preconditioner by solving Mz = r, where M=LU.
        /// </summary>
        /// <param name="luFactors">The sparse matrix containing the L and U factors.</param>
        /// <param name="inputVector">The vector 'r' to precondition.</param>
        /// <returns>The resulting vector 'z'.</returns>
        private Vector ApplyIlu0Preconditioner(SparseMatrix luFactors, Vector inputVector)
        {
            int size = inputVector.Size;
            Vector y = Vector.Create(size); // Résultat intermédiaire de Ly = inputVector

            // Forward Substitution
            for (int i = 0; i < size; i++)
            {
                double sum = 0;
                if (luFactors._data.TryGetValue(i, out Dictionary<int, double> row))
                {
                    foreach (KeyValuePair<int, double> colEntry in row)
                    {
                        if (colEntry.Key < i)
                        {
                            sum += colEntry.Value * y[colEntry.Key];
                        }
                    }
                }
                y[i] = inputVector[i] - sum;
            }

            Vector z = Vector.Create(size); // Résultat final de Uz = y

            // Backward Substitution
            for (int i = size - 1; i >= 0; i--)
            {
                double sum = 0;
                if (luFactors._data.TryGetValue(i, out Dictionary<int, double> row))
                {
                    foreach (KeyValuePair<int, double> colEntry in row)
                    {
                        if (colEntry.Key > i)
                        {
                            sum += colEntry.Value * z[colEntry.Key];
                        }
                    }
                }

                double diag = luFactors[i, i];
                if (Math.Abs(diag) < _ACCURACY_)
                    throw new DivideByZeroException($"The diagonal pivot at index {i} is close to zero. The matrix may be singular.");

                z[i] = (y[i] - sum) / diag;
            }

            return z;
        }

        #endregion

        /// <summary>Returns a string representation of the sparse matrix</summary>
        /// <returns>A string representation of the sparse matrix</returns>
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
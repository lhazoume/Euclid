using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid
{
    /// <summary>
    /// Matrix of double
    /// </summary>
    public class SparseMatrix
    {

        #region Declarations
        private readonly int _cols, _rows;
        private readonly Dictionary<int, Dictionary<int, double>> _data;

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

        /// <summary> Builds a  sparse matrix filled with the specified value </summary>
        /// <param name="rows">the number of rows</param>
        /// <param name="cols">the number of columns</param>
        public static SparseMatrix Create(int rows, int cols)
        {
            return new SparseMatrix(rows, cols);
        }
        #endregion

        #region Accessors
        /// <summary>Returns the number of columns of the <c>Matrix</c></summary>
        public int Columns
        {
            get { return _cols; }
        }

        /// <summary>Returns the number of rows of the <c>Matrix</c></summary>
        public int Rows
        {
            get { return _rows; }
        }

        /// <summary>Specifies whether the <c>Matrix</c> is square</summary>
        public bool IsSquare
        {
            get { return (_rows == _cols); }
        }

        /// <summary>Returns the number of non zero values in the <c>SparseMatrix</c></summary>
        public int CountNonZeros
        {
            get { return _data.Sum(d => d.Value.Count); }
        }

        /// <summary>Allows reading and modifying the coefficients of the <c>Matrix</c></summary>
        /// <param name="i">the row</param>
        /// <param name="j">the column</param>
        /// <returns>a double value</returns>
        public double this[int i, int j]
        {
            get { return _data.ContainsKey(i) && _data[i].ContainsKey(j) ? _data[i][j] : 0; }
            set
            {
                if (i < _rows && j < _cols)
                {
                    if (!_data.ContainsKey(i)) _data.Add(i, new Dictionary<int, double>());
                    _data[i][j] = value;
                }
            }
        }

        /// <summary>Returns a deep copy of the <c>Matrix</c></summary>
        public SparseMatrix Clone
        {
            get
            {
                SparseMatrix result = new SparseMatrix(_rows, _cols);
                foreach (int row in _data.Keys)
                {
                    result._data.Add(row, new Dictionary<int, double>());
                    foreach (int col in _data[row].Keys)
                        result._data[row].Add(col, _data[row][col]);
                }
                return result;
            }
        }
        #endregion

    }
}

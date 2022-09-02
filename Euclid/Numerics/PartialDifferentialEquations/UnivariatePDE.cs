using System;
using System.Linq;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    /// <summary>Represents a one-dimensional PDE</summary>
    public class UnivariatePDE
    {
        #region Variables
        private readonly UnivariateGridConfiguration _config;
        private readonly Vector[] _slices;
        #endregion

        #region Constructors

        /// <summary>Builds a univariate PDE (1D)</summary>
        /// <param name="configuration">the grid configuration</param>
        public UnivariatePDE(UnivariateGridConfiguration configuration)
        {
            _config = configuration;
            _slices = Enumerable.Range(0, _config.TimeCount).Select(i => Vector.Create(_config.XCount, 0.0)).ToArray();
        }

        #endregion

        #region Methods

        /// <summary>Returns the grid value for a given date, x index</summary>
        /// <param name="t">the date index</param>
        /// <param name="xIndex">the x index</param>
        /// <returns>a double</returns>
        public double Value(int t, int xIndex)
        {
            return _slices[t][xIndex];
        }

        /// <summary>
        /// Retropropagates for the given index to the previous step
        /// using the given matrix representation of the PDE operator
        /// </summary>
        /// <param name="operatorMatrix">the operator matrix</param>
        /// <param name="anteriorIndex">the index <b>to</b> which the data is retropropagated</param>
        /// <param name="posteriorIndex">the index <b>from</b> which the data is retropropagated</param>
        public void Retropropagate(Matrix operatorMatrix, int anteriorIndex, int posteriorIndex)
        {
            _slices[anteriorIndex] = operatorMatrix * _slices[posteriorIndex];
        }

        #endregion

        #region Accessors

        /// <summary>Sets the grid value for a given date</summary>
        /// <param name="t">the date index</param>
        /// <param name="value">the value grid</param>
        public Vector this[int t]
        {
            get { return _slices[t]; }
            set { _slices[t] = value; }
        }

        #endregion
    }
}

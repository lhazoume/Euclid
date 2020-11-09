using System.Collections.Generic;
using System.Linq;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    /// <summary>Represents a grid configuration for bivariate PDEs</summary>
    public sealed class BivariateGridConfiguration
    {
        #region Variables
        private readonly double[] _tBuckets, _tIncrements,
            _xBuckets, _xIncrements,
            _yBuckets, _yIncrements;
        #endregion

        /// <summary>Builds a bivariate EDP grid configuration</summary>
        /// <param name="tBuckets">the time buckets</param>
        /// <param name="xBuckets">the x-buckets</param>
        /// <param name="yBuckets">the y-buckets</param>
        public BivariateGridConfiguration(IList<double> tBuckets, IList<double> xBuckets, IList<double> yBuckets)
        {
            _tBuckets = tBuckets.Distinct().OrderBy(d => d).ToArray();
            _xBuckets = xBuckets.Distinct().OrderBy(d => d).ToArray();
            _yBuckets = yBuckets.Distinct().OrderBy(d => d).ToArray();

            _tIncrements = Enumerable.Range(0, _tBuckets.Length - 1).Select(i => _tBuckets[i + 1] - _tBuckets[i]).ToArray();
            _xIncrements = Enumerable.Range(0, _xBuckets.Length - 1).Select(i => _xBuckets[i + 1] - _xBuckets[i]).ToArray();
            _yIncrements = Enumerable.Range(0, _yBuckets.Length - 1).Select(i => _yBuckets[i + 1] - _yBuckets[i]).ToArray();
        }

        #region Time buckets
        /// <summary>Returns the number of time buckets</summary>
        public int TimeCount => _tBuckets.Length;

        /// <summary>Returns the time buckets</summary>
        public double[] TimeBuckets => _tBuckets;

        /// <summary>Returns the time increments</summary>
        public double[] TimeIncrements => _tIncrements;
        #endregion

        #region X buckets
        /// <summary>Returns the number of X buckets</summary>
        public int XCount => _xBuckets.Length;

        /// <summary>Returns the X buckets</summary>
        public double[] XBuckets => _xBuckets;

        /// <summary>Returns the X increments</summary>
        public double[] XIncrements => _xIncrements;
        #endregion

        #region Y Buckets
        /// <summary>Returns the number of Y buckets</summary>
        public int YCount => _yBuckets.Length;

        /// <summary>Returns the Y buckets</summary>
        public double[] YBuckets => _yBuckets;

        /// <summary>Returns the Y increments</summary>
        public double[] YIncrements => _yIncrements;
        #endregion
    }
}

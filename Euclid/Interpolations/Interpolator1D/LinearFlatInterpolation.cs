using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>
    /// Helps interpolations that are flat before the first anchor (the first calibrated bucket/tranche, which
    /// is always a single flat value) and piecewise linear between every subsequent pair of anchors. This matches
    /// bootstrap schemes where the first bucket is calibrated as a flat vol and later buckets ramp linearly from
    /// the previous bucket's end value to the one being calibrated.
    /// </summary>
    public class LinearFlatInterpolation : IInterpolator1D
    {
        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;

        /// <summary>Builds a flat-then-linear interpolator</summary>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public LinearFlatInterpolation(bool allowExtrapolation)
        {
            _extrapolate = allowExtrapolation;
        }

        #region Accessors
        /// <summary>The natural range of the x values</summary>
        public Interval Range => new Interval(_min, _max);

        /// <summary>Specifies if the interpolator allows extrapolation</summary>
        public bool Extrapolation => _extrapolate;

        /// <summary>Specifies if the interpolator is local (vs global)</summary>
        public bool Local => true;
        #endregion

        #region Methods

        /// <summary>Returns the interpolation method</summary>
        public IInterpolator1D Clone() => new LinearFlatInterpolation(_extrapolate);

        /// <summary>Checks if the value is inside the interpolalor's range</summary>
        /// <param name="x">the value</param>
        /// <returns><c>true</c> if the value fits in the range, <c>false</c> otherwise</returns>
        public bool IsInRange(double x)
        {
            return _extrapolate || (x >= _min && x <= _max);
        }

        /// <summary>Interpolates (or extrapolates) for a given value</summary>
        /// <param name="x">the x-value</param>
        /// <returns>the interpolated result</returns>
        public double ValueAt(double x)
        {
            if (!IsInRange(x) && !_extrapolate)
                throw new ArgumentOutOfRangeException(nameof(x), "out of the interpolation range");

            // First tranche: flat at the first anchor's value for everything at or before it
            if (x <= _values[0].X)
                return _values[0].Y;

            // Flat extrapolation beyond the last anchor
            if (_extrapolate && x >= _values[_values.Count - 1].X)
                return _values[_values.Count - 1].Y;

            // From here on, x lies strictly within (_values[0].X, _values[last].X): standard linear bracketing,
            // identical to LinearInterpolation.
            int idx = _values.FindIndex(t => t.X > x);
            int i = idx > 0 ? idx - 1 : 0;
            if (i > _values.Count - 2) i = _values.Count - 2;

            return _values[i].Y + (x - _values[i].X) * (_values[i + 1].Y - _values[i].Y) / (_values[i + 1].X - _values[i].X);
        }

        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="x">the abscisses</param>
        /// <param name="y">the ordinates </param>
        public void SetData(IList<double> x, IList<double> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));

            if (x.Count != y.Count) throw new Exception("the x-values and y-values do not match");
            _values = new List<Point2D>();

            for (int i = 0; i < x.Count; i++)
                _values.Add(new Point2D(x[i], y[i]));

            _values.Sort((a, b) => a.X.CompareTo(b.X));

            _min = _values[0].X;
            _max = _values.Last().X;
        }

        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="points">the points</param>
        public void SetData(IEnumerable<Point2D> points)
        {
            if (points is null) throw new ArgumentNullException(nameof(points));

            _values = points.ToList();

            _values.Sort((a, b) => a.X.CompareTo(b.X));

            _min = _values[0].X;
            _max = _values.Last().X;
        }
        #endregion
    }
}
using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>Helps piecewise-exponential ramp interpolation between consecutive points</summary>
    public class ExponentialInterpolation1D : IInterpolator1D
    {
        private double _min, _max;
        private readonly bool _extrapolate;
        private readonly double _k;
        private List<Point2D> _values;

        /// <summary>Builds an exponential ramp interpolator</summary>
        /// <param name="k">the exponential ramp parameter</param>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public ExponentialInterpolation1D(double k, bool allowExtrapolation)
        {
            _k = k;
            _extrapolate = allowExtrapolation;
        }

        #region Accessors
        /// <summary>The natural range of the x values</summary>
        public Interval Range => new Interval(_min, _max);

        /// <summary>Specifies if the interpolator allows extrapolation</summary>
        public bool Extrapolation => _extrapolate;

        /// <summary>Specifies if the interpolator is local (vs global)</summary>
        public bool Local => true;

        /// <summary>Returns the exponential ramp parameter</summary>
        public double K => _k;
        #endregion

        #region Methods
        /// <summary>Returns a clone of the current interpolator</summary>
        public IInterpolator1D Clone() => new ExponentialInterpolation1D(_k, _extrapolate);

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
            if (!IsInRange(x))
                throw new ArgumentOutOfRangeException(nameof(x), "out of the interpolation range");

            int i;
            if (_extrapolate)
            {
                if (x <= _values[1].X)
                    i = 0;
                else if (x > _values[_values.Count - 2].X)
                    i = _values.Count - 2;
                else
                    i = _values.FindIndex(t => t.X > x) - 1;
            }
            else
            {
                int idx = _values.FindIndex(t => t.X > x);
                i = idx == -1 ? _values.Count - 2 : (idx > 0 ? idx - 1 : 0);
            }

            double x0 = _values[i].X, x1 = _values[i + 1].X;
            double y0 = _values[i].Y, y1 = _values[i + 1].Y;

            double w = x1 > x0 ? Math.Max(0.0, Math.Min(1.0, (x - x0) / (x1 - x0))) : 1.0;

            double shape = Math.Abs(_k) < 1e-10
                ? w
                : (1.0 - Math.Exp(-_k * w)) / (1.0 - Math.Exp(-_k));

            return y0 + shape * (y1 - y0);
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
            for (int idx = 0; idx < x.Count; idx++)
                _values.Add(new Point2D(x[idx], y[idx]));

            _values.Sort((a, b) => a.X.CompareTo(b.X));
            _min = _values[0].X;
            _max = _values.Last().X;
        }

        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="points">the points</param>
        public void SetData(IEnumerable<Point2D> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));

            _values = points.ToList();
            _values.Sort((a, b) => a.X.CompareTo(b.X));
            _min = _values[0].X;
            _max = _values.Last().X;
        }
        #endregion
    }
}

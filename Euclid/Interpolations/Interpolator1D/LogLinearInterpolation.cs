using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>Helps log-linear interpolation </summary>
    public class LogLinearInterpolation : IInterpolator1D
    {
        private double _min, _max;
        private readonly bool _extrapolate;
        private readonly List<Point2D> _values;

        /// <summary>Builds a log linear interpolator</summary>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public LogLinearInterpolation(bool allowExtrapolation)
        {
            _extrapolate = allowExtrapolation;
            _values = new List<Point2D>();
        }

        #region Accessors
        /// <summary>The natural range of the x values</summary>
        public Interval Range => new Interval(_min, _max);
        /// <summary>Specifies if the interpolator allows extrapolation</summary>
        public bool Extrapolation => _extrapolate;

        /// <summary>Specifies if the interpolator is local (vs global)</summary>
        public bool Local => true;
        #endregion

        #region Method
        /// <summary>Checks if the value is inside the interpolalor's range</summary>
        /// <param name="x">the value</param>
        /// <returns><c>true</c> if the value fits in the range, <c>false</c> otherwise</returns>
        public bool IsInRange(double x)
        {
            return _extrapolate || (x >= _min && x <= _max);
        }
        private double LocalInterpolation(double x, int i)
        {
            return _values[i].Y * Math.Exp((x - _values[i].X) * (Math.Log(_values[i + 1].Y) - Math.Log(_values[i].Y)) / (_values[i + 1].X - _values[i].X));
        }

        /// <summary>Interpolates (or extrapolates) for a given value</summary>
        /// <param name="x">the x-value</param>
        /// <returns>the interpolated result</returns>
        public double ValueAt(double x)
        {
            if (!IsInRange(x)) throw new ArgumentOutOfRangeException(nameof(x), "out of the interpolation range");

            int i;
            if (_extrapolate)
            {
                if (x <= _values[1].X)
                    i = 0;
                else if (x > _values[_values.Count - 2].Y)
                    i = _values.Count - 2;
                else
                    i = _values.FindIndex(t => t.X > x) - 1;
            }
            else
                i = _values.FindIndex(t => t.X > x) - 1;

            return LocalInterpolation(x, i);
        }

        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="x">the abscisses</param>
        /// <param name="y">the ordinates </param>
        public void SetData(IList<double> x, IList<double> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (y.Any(t => t <= 0)) throw new ArgumentOutOfRangeException(nameof(y), "no negative values allowed");

            if (x.Count != y.Count) throw new Exception("the x-values and y-values do not match");
            _values.Clear();

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
            if (points.Any(p => p.Y <= 0)) throw new ArgumentOutOfRangeException(nameof(points), "no negative values allowed");

            _values.Clear();
            _values.AddRange(points);

            _values.Sort((a, b) => a.X.CompareTo(b.X));

            _min = _values[0].X;
            _max = _values.Last().X;
        }

        public IInterpolator1D Clone() => new LogLinearInterpolation(_extrapolate);
        #endregion
    }
}

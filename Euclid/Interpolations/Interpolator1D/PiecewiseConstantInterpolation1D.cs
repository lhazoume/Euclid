using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>Defines the interpolation mode when using piecewise constant</summary>
    public enum PiecewiseConstantInterpolationMode
    {
        /// <summary>Piecewise constant right interpolation</summary>
        Right = 0,
        /// <summary>Piecewise constant left interpolation</summary>
        Left = 1,
        /// <summary>Piecewise constant nearest interpolation</summary>
        Nearest = 2
    }

    /// <summary>Allows piecewise constant interpolation</summary>
    public class PiecewiseConstantInterpolation1D : IInterpolator1D
    {
        #region Variables
        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;
        private readonly PiecewiseConstantInterpolationMode _mode;
        #endregion

        /// <summary>Buils a piecewise constant interpolator</summary>
        /// <param name="mode">the mode (left, right, nearest)</param>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public PiecewiseConstantInterpolation1D(PiecewiseConstantInterpolationMode mode,
            bool allowExtrapolation)
        {
            _extrapolate = allowExtrapolation;
            _mode = mode;
        }

        #region Accessors
        /// <summary>The natural range of the x values</summary>
        public Interval Range => new Interval(_min, _max);

        /// <summary>Indicates if the interpolator allows extrapolation</summary>
        public bool Extrapolation => _extrapolate;

        /// <summary>Returns the interpolation mode</summary>
        public PiecewiseConstantInterpolationMode Mode => _mode;

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

            if (_mode == PiecewiseConstantInterpolationMode.Right)
                return _values[i].Y;
            else if (_mode == PiecewiseConstantInterpolationMode.Left)
                return _values[i + 1].Y;
            else
            {
                double mid = 0.5 * (_values[i].X + _values[i + 1].X);
                return _values[x <= mid ? i : i + 1].Y;
            }
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
        /// <param name="points">the abscisses</param>
        public void SetData(IEnumerable<Point2D> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));

            _values = points.ToList();

            _values.Sort((a, b) => a.X.CompareTo(b.X));

            _min = _values[0].X;
            _max = _values.Last().X;
        }

        public IInterpolator1D Clone() => new PiecewiseConstantInterpolation1D(_mode, _extrapolate);
        #endregion
    }
}

using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>Helps cubic spline interpolations</summary>
    public class Hyman : IInterpolator1D
    {
        #region Private fields
        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;
        private Vector _b, _m, _h;

        private Vector f, f_d;
        #endregion

        #region Constructors
        /// <summary>Builds the hyman spline interpolator</summary>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public Hyman(bool allowExtrapolation)
        {
            _extrapolate = allowExtrapolation;
        }
        #endregion

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
                else if (x > _values[_values.Count - 2].X)
                    i = _values.Count - 2;
                else
                    i = _values.FindIndex(H => H.X > x) - 1;
            }
            else
                i = _values.FindIndex(H => H.X > x) - 1;

            double t = (x - _values[i].X);
            double a = _values[i].Y;
            double c = 3 * _m[i] - _b[i - 1] - 2 * _b[i];
            double d = (_b[i + 1] + _b[i] - 2 * _m[i]) / (_h[i] * _h[i]);

            return a + _b[i] * t + c * Math.Pow(t, 2) + d * Math.Pow(t, 3);
        }

        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="x">the abscisses</param>
        /// <param name="y">the ordinates </param>
        public void SetData(IList<double> x, IList<double> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Distinct().Count() != x.Count) throw new ArgumentException("duplicate x-values", nameof(x));
            if (x.Count != y.Count) throw new Exception("the x-values and y-values do not match");

            #region Collect the data
            _values = new List<Point2D>();

            for (int i = 0; i < x.Count; i++)
                _values.Add(new Point2D(x[i], y[i]));
            #endregion

            OrganizeTheData();
        }

        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="points">the points</param>
        public void SetData(IEnumerable<Point2D> points)
        {
            if (points is null) throw new ArgumentNullException(nameof(points));

            _values = points.ToList();

            OrganizeTheData();
        }

        private void OrganizeTheData()
        {
            _values.Sort((a, b) => a.X.CompareTo(b.X));
            _min = _values[0].X;
            _max = _values.Last().X;

            int n = _values.Count;

            #region vectors
            _m = Vector.Create(n - 1);
            _b = Vector.Create(n);
            _h = Vector.Create(n - 1);

            for (int i = 0; i < n - 1; i++)
            {
                _h[i] = _values[i + 1].X - _values[i].X;
                _m[i] = (_values[i + 1].Y - _values[i].Y) / _h[i];
            }

            _b[0] = 0;
            _b[n - 1] = 0;

            for (int i = 1; i < n - 1; i++)
            {
                if (_m[i - 1] * _m[i] > 0)
                {
                    _b[i] = 3 * _m[i] * _m[i - 1] / (Math.Max(_m[i], _m[i - 1]) + 2 * Math.Min(_m[i], _m[i - 1]));

                    if (_b[i] > 0)
                    {
                        _b[i] = Math.Min(Math.Max(0, _b[i]), 3 * Math.Min(Math.Abs(_m[i]), Math.Abs(_m[i - 1])));
                    }
                    else
                    {
                        _b[i] = Math.Max(Math.Min(0, _b[i]), 3 * Math.Max(Math.Abs(_m[i]), Math.Abs(_m[i - 1])));
                    }
                }
                else
                {
                    _b[i] = 0.0;
                }
            }
            #endregion
        }

        public IInterpolator1D Clone() => new Hyman(_extrapolate);
        #endregion
    }
}

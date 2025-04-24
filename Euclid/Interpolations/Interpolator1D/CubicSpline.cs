using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>Helps cubic spline interpolations</summary>
    public class CubicSpline : IInterpolator1D
    {
        #region Private fields
        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;
        private Vector _m, _h;
        #endregion

        #region Constructors
        /// <summary>Builds the cubic spline interpolator</summary>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public CubicSpline(bool allowExtrapolation)
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
                    i = 1;
                else if (x > _values[_values.Count - 2].X)
                    i = _values.Count - 1;
                else
                    i = _values.FindIndex(t => t.X > x);
            }
            else
                i = _values.FindIndex(t => t.X > x);

            return (_m[i - 1] * Math.Pow(_values[i].X - x, 3) + _m[i] * Math.Pow(x - _values[i - 1].X, 3)) / (6 * _h[i - 1]) +
                (_values[i - 1].Y - _m[i - 1] * _h[i] * _h[i] / 6) * (_values[i].X - x) / _h[i] +
                (_values[i].Y - _m[i] * _h[i] * _h[i] / 6) * (x - _values[i - 1].X) / _h[i];
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


            #region Build and solve spline

            int n = _values.Count;

            #region vectors and matrices
            _h = Vector.Create(n);
            Vector d = Vector.Create(n);
            for (int i = 1; i < n; i++)
            {
                _h[i] = _values[i].X - _values[i - 1].X;
                if (i < n - 1)
                    d[i] = 6 * ((_values[i + 1].Y - _values[i].Y) / ((_values[i + 1].X - _values[i].X) * (_values[i + 1].X - _values[i - 1].X)) - (_values[i].Y - _values[i - 1].Y) / (_values[i + 1].X - _values[i - 1].X) * (_values[i + 1].X - _values[i - 1].X));
            }

            Matrix A = 2 * Matrix.CreateIdentityMatrix(n, n);
            A[0, 1] = 1;
            for (int i = 1; i < n - 1; i++)
            {
                A[i, i - 1] = _h[i] / (_h[i + 1] + _h[i]);
                A[i, i + 1] = 1 - A[i, i - 1];
            }
            A[n - 1, n - 2] = _h[n - 2] / (_h[n - 2] + _h[n - 1]);
            #endregion

            _m = A.SolveWith(d);
            #endregion
        }

        public IInterpolator1D Clone() => new CubicSpline(_extrapolate);
        #endregion
    }
}

using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{

    public class QuadraticSpline : IInterpolator1D
    {
        #region Private fields
        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;
        private Vector _slopes;
        private double[] _a, _b, _c;
        #endregion

        #region Constructors
        /// <summary>Builds the quadratic spline interpolator.</summary>
        /// <param name="allowExtrapolation">specifies whether extrapolation is allowed</param>
        public QuadraticSpline(bool allowExtrapolation)
        {
            _extrapolate = allowExtrapolation;
        }
        #endregion

        #region Accessors
        /// <inheritdoc/>
        public Interval Range => new Interval(_min, _max);

        /// <inheritdoc/>
        public bool Extrapolation => _extrapolate;

        /// <inheritdoc/>
        public bool Local => true;
        #endregion

        #region Public methods
        /// <inheritdoc/>
        public IInterpolator1D Clone() => new QuadraticSpline(_extrapolate);

        /// <inheritdoc/>
        public bool IsInRange(double x)=> _extrapolate || (x >= _min && x <= _max);

        /// <inheritdoc/>
        public double ValueAt(double x)
        {
            if (!IsInRange(x))
                throw new ArgumentOutOfRangeException(nameof(x), "out of the interpolation range");

            int n = _values.Count;
            int i;
            if (_extrapolate)
            {
                if (x <= _values[0].X) i = 0;
                else if (x >= _values[n - 1].X) i = n - 2;
                else i = _values.FindIndex(pt => pt.X > x) - 1;
            }
            else
            {
                if (x == _values[n - 1].X) return _values[n - 1].Y;
                i = _values.FindIndex(pt => pt.X > x) - 1;
            }

            double h = x - _values[i].X;
            return _a[i] * h * h + _b[i] * h + _c[i];
        }

        /// <inheritdoc/>
        public void SetData(IList<double> x, IList<double> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Distinct().Count() != x.Count) throw new ArgumentException("duplicate x-values", nameof(x));
            if (x.Count != y.Count) throw new ArgumentException("the x-values and y-values do not match");

            _values = x.Select((xi, idx) => new Point2D(xi, y[idx])).ToList();

            OrganizeTheData();
        }

        /// <inheritdoc/>
        public void SetData(IEnumerable<Point2D> points)
        {
            if (points is null) throw new ArgumentNullException(nameof(points));

            _values = points.ToList();
            if (_values.Select(p => p.X).Distinct().Count() != _values.Count)
                throw new ArgumentException("duplicate x-values in points", nameof(points));

            OrganizeTheData();
        }

        
        private void OrganizeTheData()
        {
            _values.Sort((a, b) => a.X.CompareTo(b.X));
            _min = _values.First().X;
            _max = _values.Last().X;

            ComputeSlopes();
            BuildSegments();
        }

        /// <summary>Computes first derivatives at each data point using finite differences.</summary>
        private void ComputeSlopes()
        {
            int n = _values.Count;
            _slopes = Vector.Create(n);

            _slopes[0] = (_values[1].Y - _values[0].Y) / (_values[1].X - _values[0].X);
            _slopes[n - 1] = (_values[n - 1].Y - _values[n - 2].Y) / (_values[n - 1].X - _values[n - 2].X);

            for (int i = 1; i < n - 1; i++)
            {
                double h = _values[i + 1].X - _values[i - 1].X;
                _slopes[i] = (_values[i + 1].Y - _values[i - 1].Y) / h;
            }
        }

        /// <summary> Builds quadratic coefficients for each segment based on slopes and values. </summary>
        private void BuildSegments()
        {
            int n = _values.Count;
            int m = n - 1;
            _a = new double[m];
            _b = new double[m];
            _c = new double[m];

            for (int i = 0; i < m; i++)
            {
                double x0 = _values[i].X;
                double y0 = _values[i].Y;
                double x1 = _values[i + 1].X;
                double y1 = _values[i + 1].Y;

                double h = x1 - x0;
                double slope0 = _slopes[i];

                _c[i] = y0;
                _b[i] = slope0;
                _a[i] = (y1 - y0 - slope0 * h) / (h * h);
            }
        }
        #endregion
    }
}

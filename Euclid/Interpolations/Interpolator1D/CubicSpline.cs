using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary> Represents a natural boundary condition for the cubic spline interpolation. </summary>
    public enum BoundaryType
    {
        Natural,
        Clamped,
    }
    public class CubicSpline : IInterpolator1D
    {
        #region Private fields

        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;
        private Vector _m, _h;
        private readonly BoundaryType _boundaryType;

        #endregion

        #region Constructors
        /// <summary>Builds the cubic spline interpolator</summary>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public CubicSpline(bool allowExtrapolation, BoundaryType bcType)
        {
            _extrapolate = allowExtrapolation;
            _boundaryType = bcType;
        }
        #endregion

        #region Accessors
        /// <summary>The natural range of the x values</summary>
        public Interval Range => new Interval(_min, _max);

        /// <summary>Specifies if the interpolator allows extrapolation</summary>
        public bool Extrapolation => _extrapolate;

        /// <summary>Specifies if the interpolator is local (vs global)</summary>
        public bool Local => false;

        /// <summary>Returns the interpolation method</summary>
        public IInterpolator1D Clone() => new CubicSpline(_extrapolate, _boundaryType);

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

            if (_values.Count == 2)
            {
                double x0 = _values[0].X, y0 = _values[0].Y;
                double x1 = _values[1].X, y1 = _values[1].Y;
                return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
            }

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
            {
                // x==max renvoie la valeur terminale
                if (x == _max)
                    return _values.Last().Y;
                i = _values.FindIndex(t => t.X > x);
            }

            return (_m[i - 1] * Math.Pow(_values[i].X - x, 3) + _m[i] * Math.Pow(x - _values[i - 1].X, 3)) / (6 * _h[i]) +
                  ((_values[i - 1].Y - _m[i - 1] * _h[i] * _h[i] / 6) * (_values[i].X - x) / _h[i]) +
                  ((_values[i].Y - _m[i] * _h[i] * _h[i] / 6) * (x - _values[i - 1].X) / _h[i]);
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
        /// <summary>
        /// Organizes the data for the interpolation
        /// </summary>
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
                _h[i] = _values[i].X - _values[i - 1].X;
           
            Matrix A = Matrix.Create(n, n);
            // Possibility to add new BC types
            switch (_boundaryType)
            {
                case BoundaryType.Natural:
                    d[0] = 0;
                    d[n - 1] = 0;
                    A[0, 0] = 1;
                    A[n - 1, n - 1] = 1;
                    for (int i = 1; i < n - 1; i++)
                    {
                        A[i, i - 1] = _h[i];
                        A[i, i] = 2 * (_h[i] + _h[i + 1]);
                        A[i, i + 1] = _h[i + 1];
                        d[i] = 6 * ((_values[i + 1].Y - _values[i].Y) / _h[i + 1]
                                  - (_values[i].Y - _values[i - 1].Y) / _h[i]);
                    }
                    break;

                case BoundaryType.Clamped:
                    // first derivative zero at endpoints
                    A[0, 0] = 2 * _h[1];
                    A[0, 1] = _h[1];
                    d[0] = 6 * (((_values[1].Y - _values[0].Y) / _h[1]) - 0);
                    A[n - 1, n - 2] = _h[n - 1];
                    A[n - 1, n - 1] = 2 * _h[n - 1];
                    d[n - 1] = 6 * (0 - ((_values[n - 1].Y - _values[n - 2].Y) / _h[n - 1]));
                    for (int i = 1; i < n - 1; i++)
                    {
                        A[i, i - 1] = _h[i];
                        A[i, i] = 2 * (_h[i] + _h[i + 1]);
                        A[i, i + 1] = _h[i + 1];
                        d[i] = 6 * ((_values[i + 1].Y - _values[i].Y) / _h[i + 1]
                                  - (_values[i].Y - _values[i - 1].Y) / _h[i]);
                    }
                    break;

                 
                default:
                    throw new NotSupportedException($"BC type {_boundaryType} not supported");
            }

            // Resolve the system A * m = d
            _m = A.SolveWith(d);
         
            #endregion

            #endregion
        }

        #endregion
    }
}

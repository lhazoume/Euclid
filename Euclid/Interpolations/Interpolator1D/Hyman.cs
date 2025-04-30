using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>Helps monotonicity-preserving cubic spline (Hyman) interpolations</summary>
    public class Hyman : IInterpolator1D
    {
        #region Private fields
        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;
        private Vector _b, _m, _h;
        #endregion

        #region Constructors
        /// <summary>Builds the Hyman spline interpolator</summary>
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
        /// <summary>Checks if the value is inside the interpolator's range</summary>
        public bool IsInRange(double x)
        {
            return _extrapolate || (x >= _min && x <= _max);
        }

        /// <summary>Interpolates (or extrapolates) for a given value using Hyman's scheme</summary>
        public double ValueAt(double x)
        {
            if (!IsInRange(x))
                throw new ArgumentOutOfRangeException(nameof(x), "out of the interpolation range");

            int i;

            if (_values.Count == 2)
            {
                double x0 = _values[0].X, y0 = _values[0].Y;
                double x1 = _values[1].X, y1 = _values[1].Y;
                return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
            }
            if (x == _max)
                return _values[_values.Count - 1].Y;
            if (_values.FindIndex(p => p.X > x) <= 0)
                i = 0;
            else if (_values.FindIndex(p => p.X > x) == -1)
                i = _values.Count - 2;
            else
                i = _values.FindIndex(p => p.X > x) - 1;

            return _values[i].Y + _b[i] * (x - _values[i].X) + ((3 * _m[i] - 2 * _b[i] - _b[i + 1]) / _h[i]) * (x - _values[i].X) * (x - _values[i].X) + ((_b[i] + _b[i + 1] - 2 * _m[i]) / (_h[i] * _h[i])) * (x - _values[i].X) * (x - _values[i].X) * (x - _values[i].X);
        
        }


        /// <summary>Sets the data for the interpolation</summary>
        public void SetData(IList<double> x, IList<double> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Distinct().Count() != x.Count) throw new ArgumentException("duplicate x-values", nameof(x));
            if (x.Count != y.Count) throw new Exception("the x-values and y-values do not match");

            _values = new List<Point2D>();
            for (int k = 0; k < x.Count; k++)
                _values.Add(new Point2D(x[k], y[k]));
            OrganizeTheData();
        }

        /// <summary>Sets the data for the interpolation</summary>
        public void SetData(IEnumerable<Point2D> points)
        {
            if (points is null) throw new ArgumentNullException(nameof(points));
            _values = points
              .OrderBy(p => p.X)
              .GroupBy(p => p.X)     // enlève les doublons de X
              .Select(g => g.First())
              .ToList();
            OrganizeTheData();
        }

        /// <summary>Organizes the data and computes slopes for Hyman's interpolator</summary>
        private void OrganizeTheData()
        {
     
            _min = _values[0].X;
            _max = _values[_values.Count - 1].X;

            int n = _values.Count;

            _h = Vector.Create(n - 1, 0.0); 
            _m = Vector.Create(n - 1, 0.0); 
            _b = Vector.Create(n, 0.0);
        
            for (int i = 0; i < n - 1; i++)
            {
                _h[i] = _values[i + 1].X - _values[i].X;
                if (_h[i] <= 0) throw new InvalidOperationException("X values must be strictly increasing.");
                _m[i] = (_values[i + 1].Y - _values[i].Y) / _h[i];
            }

            // Special case for n = 2
            if (n == 2)
            {
                _b[0] = _m[0];
                _b[1] = _m[0];
                return; 
            }

            for (int i = 1; i < n - 1; i++)
            {
 
                double dPrev = _m[i - 1]; 
                double dCurr = _m[i];
                double hPrev = _h[i - 1]; 
                double hNext = _h[i];

                // Monotonicity check
                if (dPrev * dCurr <= 0.0)
                {
                    _b[i] = 0.0;
                }
                else
                {
                    //  Fritsch-Butland
                    double w1 = 2 * hNext + hPrev;
                    double w2 = hNext + 2 * hPrev;

                    double denom = w1 / dPrev + w2 / dCurr;
                    if (Math.Abs(denom) < 1e-15) 
                    {
                        _b[i] = 0.0;
                    }
                    else
                    {
                        _b[i] = (w1 + w2) / denom;
                    }
                }
            }

            _b[0] = ComputeEdgeDerivative(_h[0], _h[1], _m[0], _m[1]);
            _b[n - 1] = ComputeEdgeDerivative(_h[n - 2], _h[n - 3], _m[n - 2], _m[n - 3]);
        }
        /// <summary>
        /// Computes the derivative at the edge of the data 
        /// </summary>
        /// <param name="h_adj"></param>
        /// <param name="h_next"></param>
        /// <param name="m_adj"></param>
        /// <param name="m_next"></param>
        /// <returns></returns>
        private static double ComputeEdgeDerivative(double h_adj, double h_next, double m_adj, double m_next)
        {
            double h_sum = h_adj + h_next;

            // Estimation of the derivative
            double d = ((2 * h_adj + h_next) * m_adj - h_adj * m_next) / h_sum;

            int sign_d = Math.Sign(d);
            int sign_m_adj = Math.Sign(m_adj);

            if (sign_d != sign_m_adj) 
            {
                d = 0.0;
            }
            else
            {
                int sign_m_next = Math.Sign(m_next);
                if (sign_m_adj != sign_m_next && Math.Abs(d) > 3.0 * Math.Abs(m_adj))
                {
                    d = 3.0 * m_adj;
                }
            }
            return d;
        }

        /// <summary>
        /// Returns a deep copy of the interpolator
        /// </summary>
        /// <returns></returns>
        public IInterpolator1D Clone() => new Hyman(_extrapolate);
        #endregion
    }
}

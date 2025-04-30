using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary>Helps linear interpolations</summary>
    public class LinearInterpolation : IInterpolator1D
    {
        private double _min, _max;
        private readonly bool _extrapolate;
        private List<Point2D> _values;

        /// <summary>Builds a linear interpolator</summary>
        /// <param name="allowExtrapolation">specifies whether extrapolations are allowed</param>
        public LinearInterpolation(bool allowExtrapolation)
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

            int i = 0;

            if (_extrapolate)
            {
                if (x <= _values[1].X)
                    i = 0;
                else if (x > _values[_values.Count - 2].X)
                    i = _values.Count - 2;
                else
                {
                    int idx = _values.FindIndex(t => t.X > x);
                    i = idx > 0 ? idx - 1 : 0;
                }
            }
            else
            {
                int idx = _values.FindIndex(t => t.X > x);
                if (idx == -1)
                    i = _values.Count - 2;
                else
                    i = idx > 0 ? idx - 1 : 0;//limite
            }

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

        public IInterpolator1D Clone() => new LinearInterpolation(_extrapolate);
        #endregion
    }
}

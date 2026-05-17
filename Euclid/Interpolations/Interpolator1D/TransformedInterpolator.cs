using System;
using System.Collections.Generic;
using System.Linq;
using Euclid.Histograms;

namespace Euclid.Interpolations.Interpolator1D
{
    /// <summary> A class that transforms the data points of an interpolator using a forward function and applies a backward function to the interpolated values. </summary>
    public class TransformedInterpolator : IInterpolator1D
    {
        #region Private fields
        private readonly IInterpolator1D _interpolator;
        private readonly Func<double, double, double> _forward;
        private readonly Func<double, double, double> _backward;
        #endregion

        #region Constructors
        /// <summary>Initializes a new instance of the <see cref="TransformedInterpolator"/> class.</summary>
        /// <param name="interpolator">The underlying interpolator to be transformed</param>
        /// <param name="forward">The function to transform the data points before interpolation</param>
        /// <param name="backward">The function to transform the interpolated values back</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
        public TransformedInterpolator(IInterpolator1D interpolator,Func<double, double, double> forward,Func<double, double, double> backward)
        {
            _interpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));
            _forward = forward ?? throw new ArgumentNullException(nameof(forward));
            _backward = backward ?? throw new ArgumentNullException(nameof(backward));
        }
        #endregion

        #region Accessors
        /// <summary> Gets the natural range of the x values for the interpolation. </summary>
        public Interval Range => _interpolator.Range;
        /// <summary> Gets a value indicating whether the interpolator allows extrapolation outside the range.</summary>
        public bool Extrapolation => _interpolator.Extrapolation;
        /// <summary> Gets a value indicating whether the interpolation is local (as opposed to global</summary>
        public bool Local => _interpolator.Local;
        /// <summary> Checks if the specified x-value is within the interpolator's range.</summary>
        /// <param name="x">The x-value to check.</param>
        /// <returns><c>true</c> if the value is in range; otherwise, <c>false</c>.</returns>
        public bool IsInRange(double x) => _interpolator.IsInRange(x);
        /// <summary> Returns a clone of the current <see cref="TransformedInterpolator"/> instance. </summary>
        /// <returns>A new <see cref="IInterpolator1D"/> that is a copy of this instance.</returns>
        public IInterpolator1D Clone() => new TransformedInterpolator(_interpolator.Clone(), _forward, _backward);
        #endregion

        #region Methods
        /// <summary> Sets the data for the interpolation by transforming the points using the forward function.</summary>
        /// <param name="points">The collection of 2D points to be transformed and used for interpolation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="points"/> is null.</exception>
        public void SetData(IEnumerable<Point2D> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));

            List<Point2D> pts = points.Select(p => new Point2D(p.X, _forward(p.X, p.Y))).OrderBy(p => p.X).ToList();

            _interpolator.SetData(pts);
        }
        /// <summary>Sets the data for the interpolation using two lists of x and y values. </summary>
        /// <param name="x">The list of x-values (abscissas).</param>
        /// <param name="y">The list of y-values (ordinates).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> or <paramref name="y"/> is null.</exception>
        /// <exception cref="Exception">Thrown if <paramref name="x"/> and <paramref name="y"/> do not have the same length.</exception>
        public void SetData(IList<double> x, IList<double> y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Distinct().Count() != x.Count) throw new ArgumentException("duplicate x-values", nameof(x));
            if (x.Count != y.Count) throw new Exception("the x-values and y-values do not match");

            List<double> ys = new List<double>(y.Count);
            for (int i = 0; i < y.Count; i++)
                ys.Add(_forward(x[i], y[i]));

            _interpolator.SetData(x, ys);
        }
        /// <summary>Interpolates (or extrapolates) for a given x-value using the backward transformation.</summary>
        /// <param name="x">The x-value for which to interpolate.</param>
        /// <returns>The interpolated (or extrapolated) y-value after applying the backward transformation.</returns>
        public double ValueAt(double x)
        {
            double yPrime = _interpolator.ValueAt(x);
            return _backward(x, yPrime);
        }
        #endregion

    }
}

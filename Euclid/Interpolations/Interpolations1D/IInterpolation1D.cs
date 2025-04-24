using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Interpolations.Interpolations1D
{
    /// <summary>Interface for all the 1D interpolators</summary>
    public interface IInterpolation1D
    {
        #region Accessors
        /// <summary>The natural range of the x values</summary>
        Interval Range { get; }
        /// <summary>Specifies if the interpolator allows extrapolation</summary>
        bool Extrapolation { get; }

        /// <summary>Specifies if the interpolation is local (vs global)</summary>
        bool Local { get; }

        /// <summary>Returns the interpolation method</summary>
        Interpolation1D Method { get; }
        #endregion

        #region Methods
        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="x">the abscisses</param>
        /// <param name="y">the ordinates </param>
        void SetData(IList<double> x, IList<double> y);

        /// <summary>Sets the data for the interpolation</summary>
        /// <param name="points">the points</param>
        void SetData(IEnumerable<Point2D> points);

        /// <summary>Checks if the value is inside the interpolalor's range</summary>
        /// <param name="x">the value</param>
        /// <returns><c>true</c> if the value fits in the range, <c>false</c> otherwise</returns>
        bool IsInRange(double x);

        /// <summary>Interpolates (or extrapolates) for a given value</summary>
        /// <param name="x">the x-value</param>
        /// <returns>the interpolated result</returns>
        double ValueAt(double x);
        #endregion
    }
}

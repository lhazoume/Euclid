using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Cauchy distribution class
    /// </summary>
    public class CauchyDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _x0, _gamma;
        #endregion

        #region Constructors
        private CauchyDistribution(double x0, double gamma, Random randomSource)
        {
            _x0 = x0;

            if (gamma <= 0) throw new ArgumentException("gamma has to be positive");
            _gamma = gamma;

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        /// <summary>Builds a Cauchy distribution</summary>
        /// <param name="x0">the location</param>
        /// <param name="gamma">the scale</param>
        public CauchyDistribution(double x0, double gamma)
            : this(x0, gamma, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return Math.Log(_gamma) - Math.Log(4 * Math.PI); }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return _x0; }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return _x0; }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return _x0; }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return double.NaN; }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return double.NaN; }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return double.NaN; }
        }
        #endregion

        #region Methods

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static CauchyDistribution Fit(FittingMethod method, double[] sample)
        {
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            return 0.5 + Math.Atan((x - _x0) / _gamma);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _x0 + _gamma * Math.Tan(Math.PI * (p - 0.5));
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return 1 / (Math.PI * _gamma * (1 + Math.Pow((x - _x0) / _gamma, 2)));
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Cauchy(x0 = {0} γ = {1})", _x0, _gamma);
        }
        #endregion
    }
}

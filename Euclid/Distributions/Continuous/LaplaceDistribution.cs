using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Laplace distribution class
    /// </summary>
    public class LaplaceDistribution : ContinuousDistribution
    {
        #region Declarations
        private double _mu, _b;
        #endregion

        #region Constructors
        private LaplaceDistribution(double mu, double b, Random randomSource)
        {
            if (b <= 0) throw new ArgumentException("scale has to be positive");
            _mu = mu;
            _b = b;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        /// <summary>
        /// Builds a Laplace distribution
        /// </summary>
        /// <param name="mu">the location</param>
        /// <param name="b">the scale</param>
        public LaplaceDistribution(double mu, double b)
            : this(mu, b, new Random(DateTime.Now.Millisecond))
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return Math.Log(2 * _b * Math.E); }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return _mu; }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return _mu; }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return _mu; }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return 0; }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return Math.Sqrt(2) * _b; }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return 2 * _b * _b; }
        }
        #endregion

        #region Methods

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            double e = (x - _mu) / _b;
            if (e < 0)
                return 0.5 * Math.Exp(e);
            else
                return 1 - 0.5 * Math.Exp(-e);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function
        /// </summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            if (p >= 0.5)
                return _mu - _b * Math.Log(2 * (1 - p));
            else
                return _mu + _b * Math.Log(2 * p);
        }

        /// <summary>
        /// Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return Math.Exp(-Math.Abs(x - _mu) / _b) / (2 * _b);
        }

        /// <summary>Computes the probability density function's logarithm at x</summary>
        /// <param name="x">the location at which to compute the density</param>
        /// <returns>a double</returns>
        public override double ProbabilityLnDensity(double x)
        {
            return -Math.Abs(x - _mu) / _b - Math.Log(2 * _b);
        }

        #endregion
    }
}

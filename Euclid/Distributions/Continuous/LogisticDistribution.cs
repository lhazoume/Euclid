using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Logistic distribution class</summary>
    public class LogisticDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _mu, _s;
        #endregion

        #region Constructors
        private LogisticDistribution(double mu, double s, Random randomSource)
        {
            _mu = mu;

            if (s <= 0) throw new ArgumentException("scale has to be positive");
            _s = s;

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        /// <summary>Builds a Logistic distribution</summary>
        /// <param name="mu">the location</param>
        /// <param name="s">the scale</param>
        public LogisticDistribution(double mu, double s)
            : this(mu, s, new Random(Guid.NewGuid().GetHashCode()))
        { }

        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return Math.Log(_s) + 2; }
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
            get { return 0.0; }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return _s * Math.PI / Math.Sqrt(3); }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return Math.Pow(_s * Math.PI, 2) / 3; }
        }

        #endregion

        #region Methods

        public override void Fit(FittingMethod method, double[] sample)
        {
            //TODO : implement here
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-(x - _mu) / _s));
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _mu - _s * Math.Log(1 / p - 1);
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            double e = Math.Exp(-(x - _mu) / _s);
            return e / (_s * Math.Pow(1 + e, 2));
        }

        #endregion
    }
}

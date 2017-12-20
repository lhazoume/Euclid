using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Weibull distribution class
    /// </summary>
    public class WeibullDistribution : ContinuousDistribution, IParametricDistribution
    {
        #region Declarations
        private double _lambda, _k, _mu, _sigma2, _sigma;
        #endregion

        private WeibullDistribution(double lambda, double k, Random randomSource)
        {
            if (lambda <= 0) throw new ArgumentException("The scale can not be negative");
            if (k <= 0) throw new ArgumentException("The shape can not be negative");
            _lambda = lambda;
            _k = k;

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(0, double.PositiveInfinity, true, false);

            _mu = _lambda * Fn.Gamma(1 + 1 / _k);
            _sigma2 = _lambda * _lambda * Fn.Gamma(1 + 2 / _k) - _mu * _mu;
            _sigma = Math.Sqrt(_sigma2);
        }

        /// <summary>
        /// Builds a Weibull distribution
        /// </summary>
        /// <param name="lambda">the scale</param>
        /// <param name="k">the shape</param>
        public WeibullDistribution(double lambda, double k)
            : this(lambda, k, new Random(Guid.NewGuid().GetHashCode()))
        { }

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return Fn.EulerGamma * (1 - 1 / _k) + Math.Log(_lambda / _k) + 1; }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return _mu; }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return _lambda * Math.Pow(Math.Log(2), 1 / _k); }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get
            {
                if (_k < 1) return double.NaN;
                else if (_k == 1) return 0;
                return _lambda * Math.Pow((_k - 1) / _k, 1 / _k);
            }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return Fn.Gamma(1 + 3 / _k) * Math.Pow(_lambda / _sigma, 3) - 3 * _mu / _sigma - Math.Pow(_mu / _sigma, 3); }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return _sigma; }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return _sigma2; }
        }
        #endregion

        #region Methods

        /// <summary>Fits the distribution to a sample of data</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public void Fit(FittingMethod method, double[] sample)
        {
            //TODO : implement here
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            return 1 - Math.Exp(-Math.Pow(x / _lambda, _k));
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _lambda * Math.Pow(-Math.Log(1 - p), 1 / _k);
        }

        /// <summary>
        /// Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x < 0) return 0;
            return (_k / _lambda) * Math.Pow(x / _lambda, _k - 1) * Math.Exp(-Math.Pow(x / _lambda, _k));
        }
        #endregion
    }
}

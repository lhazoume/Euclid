using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Log Normal distribution class
    /// </summary>
    public class LogNormalDistribution : ContinuousDistribution
    {
        #region Declarations
        private double _mu, _sigma, _sigma2;
        #endregion

        #region Constructors
        private LogNormalDistribution(double mu, double sigma, Random randomSource)
        {
            if (sigma <= 0) throw new ArgumentException("sigma has to be positive");
            _sigma = sigma;
            _sigma2 = _sigma * _sigma;
            _mu = mu;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;
            _support = new Interval(0, double.PositiveInfinity, false, false);
        }
        /// <summary>Builds a log normal distribution</summary>
        /// <param name="mu">the average</param>
        /// <param name="sigma">the standard deviation</param>
        public LogNormalDistribution(double mu, double sigma)
            : this(mu, sigma, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return Math.Log(_sigma * Math.Exp(_mu + 0.5) * Math.Sqrt(2 * Math.PI)); }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return Math.Exp(_mu + 0.5 * _sigma2); }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return Math.Exp(_mu); }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return Math.Exp(_mu - _sigma2); }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return (Math.Exp(_sigma2) + 2) * Math.Sqrt(Math.Exp(_sigma2) - 1); }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return Math.Sqrt(Variance); }
        }

        /// <summary>Gets the distributions's variance</summary>
        public override double Variance
        {
            get { return (Math.Exp(_sigma2) - 1) * Math.Exp(2 * _mu + _sigma2); }
        }
        #endregion

        #region Methods
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static LogNormalDistribution Fit(FittingMethod method, double[] sample)
        {
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x <= 0) return 0;
            else return Fn.Phi((Math.Log(x) - _mu) / _sigma);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return Math.Exp(_mu + _sigma * Fn.InvPhi(p));
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x <= 0) return 0;
            else return Math.Exp(-0.5 * Math.Pow(Math.Log(x) - _mu, 2) / _sigma2) / (x * _sigma * Math.Sqrt(2 * Math.PI));
        }

        /// <summary>Generates a sequence of samples from the log normal distribution</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                result[i] = Math.Exp(_mu + _sigma * Fn.InvPhi(Math.Log(_randomSource.NextDouble())));
            return result;
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Log-N(μ = {0}, σ = {1})", _mu, _sigma);
        }
        #endregion
    }
}

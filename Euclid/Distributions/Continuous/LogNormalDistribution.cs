using System;
using System.Linq;
using Euclid.Extensions;
using Euclid.Histograms;

namespace Euclid.Distributions.Continuous
{
    /// <summary> Log Normal distribution class </summary>
    public class LogNormalDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _mu, _sigma, _sigma2;
        #endregion

        #region Constructors
        /// <summary>Builds a log normal distribution</summary>
        /// <param name="mu">the average</param>
        /// <param name="sigma">the standard deviation</param>
        public LogNormalDistribution(double mu, double sigma)
        {
            if (sigma <= 0) throw new ArgumentException("sigma has to be positive");
            _sigma = sigma;
            _sigma2 = _sigma * _sigma;
            _mu = mu;
            _support = new Interval(0, double.PositiveInfinity, false, false);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => Math.Exp(_mu + 0.5 * _sigma2);

        /// <summary>Gets the distribution's median</summary>
        public override double Median => Math.Exp(_mu);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => Math.Exp(_mu - _sigma2);

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => Math.Sqrt(Variance);

        /// <summary>Gets the distributions's variance</summary>
        public override double Variance => (Math.Exp(_sigma2) - 1) * Math.Exp(2 * _mu + _sigma2);

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => (Math.Exp(_sigma2) + 2) * Math.Sqrt(Math.Exp(_sigma2) - 1);

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Math.Log(_sigma * Math.Exp(_mu + 0.5) * Math.Sqrt(2 * Math.PI));

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the parameter mu of the normal distribution</summary>
        public double Mu => _mu;

        /// <summary>Gets the parameter sigma of the normal distribution</summary>
        public double Sigma => _sigma;
        #endregion

        #region Methods
        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            return (x <= 0) ? 0 : Fn.Phi((Math.Log(x) - _mu) / _sigma);
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
            return (x <= 0) ? 0 : Math.Exp(-0.5 * Math.Pow(Math.Log(x) - _mu, 2) / _sigma2) / (x * _sigma * Math.Sqrt(2 * Math.PI));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t) => double.NaN;

        /// <summary> Builds a sample of random variables under this distribution </summary>
        /// <param name="size">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int size, int seed)
        {
            NormalDistribution N = new NormalDistribution(_mu, _sigma);
            double[] res = N.Sample(size).Apply(x => Math.Exp(x));
            return res;
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static LogNormalDistribution Fit(double[] sample) => Fit(FittingMethod.MaximumLikelihood, sample);

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static LogNormalDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Length == 0)
                throw new ArgumentException("the sample can't be empty");
            if (sample.Any(d => d <= 0))
                throw new ArgumentOutOfRangeException(nameof(sample));
            int n = sample.Length;
            double mean = 0,
                    variance = 0;

            if (method == FittingMethod.Moments)
            {
                for (int i = 0; i < n; i++)
                {
                    mean += sample[i];
                    variance += sample[i] * sample[i];
                }
                mean /= n;
                variance = variance / n - mean * mean;
                double sigma = Math.Sqrt(Math.Log(1 + variance / Math.Pow(mean, 2))),
                    mu = Math.Log(mean) - Math.Log(1 + variance / Math.Pow(mean, 2)) / 2;

                return new LogNormalDistribution(mu, sigma);

            }
            else if (method == FittingMethod.MaximumLikelihood)
            {
                for (int i = 0; i < n; i++)
                {
                    mean += Math.Log(sample[i]);
                    variance += Math.Log(sample[i]) * Math.Log(sample[i]);
                }
                mean /= n;
                variance = variance / n - mean * mean;

                return new LogNormalDistribution(mean, Math.Sqrt(variance));
            }
            throw new NotImplementedException();
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format($"Log-N(μ = {_mu}, σ = {_sigma})");
        }
        #endregion
    }
}

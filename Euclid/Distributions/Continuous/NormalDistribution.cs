using Euclid.Histograms;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Normal distribution class
    /// </summary>
    public class NormalDistribution : ContinuousDistribution, IParametricDistribution
    {
        #region Declarations
        private double _mean, _standardDeviation;
        #endregion

        #region Constructors
        private NormalDistribution(double mean, double standardDeviation, Random randomSource)
        {
            _mean = mean;
            if (standardDeviation < 0) throw new ArgumentException("The standard deviation can not be negative");
            _standardDeviation = standardDeviation;

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        /// <summary>
        /// Builds a standard normal distribution
        /// </summary>
        public NormalDistribution()
            : this(0, 1)
        { }

        /// <summary>
        /// Builds a normal distribution
        /// </summary>
        /// <param name="mean">the average</param>
        /// <param name="standardDeviation">the standard deviation</param>
        public NormalDistribution(double mean, double standardDeviation)
            : this(mean, standardDeviation, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Methods
        public void Fit(FittingMethod method, double[] sample)
        {
            double avg = sample.Average(),
                stdev = Math.Sqrt(sample.Select(x => x * x).Average() - avg * avg);
            _mean = avg;
            _standardDeviation = stdev;
        }

        /// <summary>
        /// Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            return Fn.Phi((x - _mean) / _standardDeviation);
        }

        /// <summary>
        /// Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return Fn.GaussBell((x - _mean) / _standardDeviation);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _mean + _standardDeviation * Fn.InvPhi(p);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the algorithm
        /// </summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints / 2; i++)
            {
                double u = 1.0 - _randomSource.NextDouble(),
                    v = _randomSource.NextDouble(),
                    x = Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v),
                    y = Math.Sqrt(-2 * Math.Log(u)) * Math.Sin(2 * Math.PI * v);
                result[2 * i] = _mean + _standardDeviation * x;
                result[2 * i + 1] = _mean + _standardDeviation * y;
            }

            if (numberOfPoints % 2 == 1)
            {
                double u = 1.0 - _randomSource.NextDouble(),
                    v = _randomSource.NextDouble(),
                    x = Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
                result[numberOfPoints - 1] = _mean + _standardDeviation * x;
            }

            return result;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Gets the entropy of the normal distribution
        /// </summary>
        public override double Entropy
        {
            get { return Math.Log(_standardDeviation * Math.Sqrt(2 * Math.PI * Math.E)); }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>
        /// Gets the mean(μ) of the normal distribution
        /// </summary>
        public override double Mean
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the median of the normal distribution
        /// </summary>
        public override double Median
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the mode of the normal distribution
        /// </summary>
        public override double Mode
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the skewness of the normal distribution
        /// </summary>
        public override double Skewness
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the distribution's standard deviation
        /// </summary>
        public override double StandardDeviation
        {
            get { return _standardDeviation; }
        }

        /// <summary>
        /// Gets the distribution's variance
        /// </summary>
        public override double Variance
        {
            get { return _standardDeviation * _standardDeviation; }
        }
        #endregion
    }
}

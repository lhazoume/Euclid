using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Uniform distribution class
    /// </summary>
    public class UniformDistribution : ContinuousDistribution
    {
        #region Declarations
        private double _a, _b, _d, _m;
        #endregion

        #region Constructors
        private UniformDistribution(double a, double b, Random randomSource)
        {
            if (a >= b) throw new ArgumentException("the interval is not defined");
            _a = a;
            _b = b;
            _d = _b - _a;
            _m = 0.5 * (_b + _a);

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(_a, _b, true, true);
        }

        /// <summary>
        /// Builds a Uniform distribution
        /// </summary>
        /// <param name="a">the support's lower bound</param>
        /// <param name="b">the support's upper bound</param>
        public UniformDistribution(double a, double b)
            : this(a, b, new Random(DateTime.Now.Millisecond))
        { }

        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return Math.Log(_d); }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return _m; }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return _m; }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return _m; }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return 0; }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return _d / Math.Sqrt(12); }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return _d * _d / 12; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < _a) return 0;
            if (x > _b) return 1;
            return (x - _a) / _d;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _a + _d * p;
        }

        /// <summary>
        /// Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (_support.Contains(x)) return 1 / _d;
            return 0;
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the algorithm
        /// </summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                result[i] = _a + _d * _randomSource.NextDouble();
            return result;
        }
        #endregion
    }
}

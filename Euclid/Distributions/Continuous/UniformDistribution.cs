using System;
using System.Linq;
using Euclid.Histograms;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Uniform distribution class</summary>
    public class UniformDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _a, _b, _d, _m;
        #endregion

        #region Constructors
        /// <summary>Builds a Uniform distribution</summary>
        /// <param name="a">the support's lower bound</param>
        /// <param name="b">the support's upper bound</param>
        public UniformDistribution(double a, double b)
        {
            if (a >= b) throw new ArgumentException("the interval is not defined");
            _a = a;
            _b = b;
            _d = _b - _a;
            _m = 0.5 * (_b + _a);

            _support = new Interval(_a, _b, true, true);
        }

        /// <summary>Builds a standard Uniform distribution </summary>
        public UniformDistribution()
            : this(0, 1)
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _m;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _m;

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _m;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _d / Math.Sqrt(12);

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _d * _d / 12;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => 0;

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Math.Log(_d);

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's support upper bound</summary>
        public double UpperBound => _b;

        /// <summary>Gets the distribution's support lower bound</summary>
        public double LowerBound => _a;
        #endregion

        #region Methods
        /// <summary> Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x) </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < _a) return 0;
            if (x > _b) return 1;
            return (x - _a) / _d;
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _a + _d * p;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return _support.Contains(x) ? 1 / _d : 0;
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>

        public override double MomentGeneratingFunction(double t)
        {
            return t == 0 ? 1.0 : (Math.Exp(t * _b) - Math.Exp(t * _a)) / (t * _d);
        }


        /// <summary> Builds a sample of random variables under this distribution </summary>
        /// <param name="size">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>

        public override double[] Sample(int size, int seed)
        {
            Random random = new Random(seed);
            double[] sample = new double[size];

            for (int i = 0; i < size; i++)
            {
                double u = random.NextDouble();
                sample[i] = _a + _d * u;
            }

            return sample;
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static UniformDistribution Fit(double[] sample) => Fit(FittingMethod.MaximumLikelihood, sample);

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static UniformDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Length == 0)
                throw new ArgumentException("the sample can't be empty");
            if (method == FittingMethod.Moments)
            {
                int n = sample.Length;
                double avg = 0,
                    dev = 0;
                for (int i = 0; i < n; i++)
                {
                    avg += sample[i];
                    dev += sample[i] * sample[i];
                }
                avg /= n;
                dev = Math.Sqrt(3 * (dev / n - avg * avg));
                return new UniformDistribution(avg - dev, avg + dev);
            }
            else if (method == FittingMethod.MaximumLikelihood)
            {
                return new UniformDistribution(sample.Min(), sample.Max());
            }
            throw new NotImplementedException();
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format($"Uniform(a = {_a} b = {_b})");
        }
        #endregion
    }
}

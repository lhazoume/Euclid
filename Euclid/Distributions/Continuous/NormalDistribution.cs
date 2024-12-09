using System;
using System.Threading.Tasks;
using Euclid.Histograms;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Normal distribution class</summary>
    public class NormalDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _mu, _sigma;
        #endregion

        #region Constructors
        /// <summary>Builds a normal distribution</summary>
        /// <param name="mean">the distribution's mean</param>
        /// <param name="standardDeviation">the distributions's standard deviation</param>
        public NormalDistribution(double mean, double standardDeviation)
        {
            _mu = mean;
            if (standardDeviation < 0) throw new ArgumentException("The standard deviation can not be negative");
            _sigma = standardDeviation;

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        /// <summary>Builds a standard normal distribution</summary>
        public NormalDistribution()
            : this(0, 1)
        {}
        #endregion

        #region Accessors
        /// <summary>Gets the mean(μ) of the normal distribution</summary>
        public override double Mean => _mu;

        /// <summary>Gets the median of the normal distribution</summary>
        public override double Median => _mu;

        /// <summary>Gets the mode of the normal distribution</summary>
        public override double Mode => _mu;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _sigma;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _sigma * _sigma;

        /// <summary>Gets the skewness of the normal distribution</summary>
        public override double Skewness => 0;

        /// <summary>Gets the entropy of the normal distribution</summary>
        public override double Entropy => Math.Log(_sigma * Math.Sqrt(2 * Math.PI * Math.E));

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
            return Fn.Phi((x - _mu) / _sigma);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _mu + _sigma * Fn.InvPhi(p);
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return Fn.GaussBell((x - _mu) / _sigma) / _sigma;
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return Math.Exp(_mu * t + 0.5 * Math.Pow(_sigma * t, 2));
        }

        /// <summary>Builds a sample of random variables under this distribution</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {
            Random random = new Random(seed);
            double[] result = new double[numberOfPoints];
            int processorCount = Environment.ProcessorCount;

            #region Initialize the seeds
            int[] seeds = new int[processorCount];
            for (int p = 0; p < processorCount; p++)
                seeds[p] = random.Next();
            int buckets = numberOfPoints / processorCount + (numberOfPoints % processorCount == 0 ? 0 : 1);
            buckets += buckets % 2 == 0 ? 0 : 1;
            #endregion

            Parallel.For(0, processorCount, p =>
            {
                Random rnd = new Random(seeds[p]);
                for (int b = 0; b < buckets / 2; b++)
                {
                    int ix = p * buckets + 2 * b;
                    double r = Math.Sqrt(-2 * Math.Log(1.0 - rnd.NextDouble())),
                        t = 2 * Math.PI * rnd.NextDouble(),
                        x = r * Math.Cos(t),
                        y = r * Math.Sin(t);
                    if (ix >= numberOfPoints)
                        break;
                    else
                        result[ix] = _mu + _sigma * x;
                    if (ix + 1 >= numberOfPoints)
                        break;
                    else
                        result[ix + 1] = _mu + _sigma * y;
                }
            });
            return result;
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static NormalDistribution Fit(double[] sample) => Fit(FittingMethod.MaximumLikelihood, sample);

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static NormalDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Length == 0)
                throw new ArgumentException("the sample can't be empty");
            if (method == FittingMethod.Moments || method == FittingMethod.MaximumLikelihood)
            {
                double mean = 0,
                    stdev = 0;
                int n = sample.Length;
                for (int i = 0; i < n; i++)
                {
                    mean += sample[i];
                    stdev += sample[i] * sample[i];
                }
                mean /= n;
                stdev = Math.Sqrt(stdev / n - mean * mean);
                return new NormalDistribution(mean, stdev);
            }
            throw new NotImplementedException();
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format($"N(μ = {_mu}, σ = {_sigma})");
        }
        #endregion
    }
}

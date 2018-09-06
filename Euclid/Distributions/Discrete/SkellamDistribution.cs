using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Distributions.Discrete
{
    /// <summary>Skellam distribution</summary>
    public class SkellamDistribution : DiscreteDistribution
    {
        #region Declarations
        private double _mu1, _mu2;
        private const double _supportWidthInStandardDeviations = 10;
        #endregion


        private SkellamDistribution(double mu1, double mu2, Random randomSource)
        {
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            if (mu1 <= 0) throw new ArgumentOutOfRangeException("mu1", "The mu1 should be >0");
            _mu1 = mu1;

            if (mu2 <= 0) throw new ArgumentOutOfRangeException("mu2", "The lambda should be >0");
            _mu2 = mu2;

            double lBound = _mu1 - _mu2 - _supportWidthInStandardDeviations * Math.Sqrt(_mu1 + _mu2),
                uBound = _mu1 - _mu2 + _supportWidthInStandardDeviations * Math.Sqrt(_mu1 + _mu2);
            _support = Enumerable.Range(Convert.ToInt32(lBound), Convert.ToInt32(uBound - lBound) + 1).Select(i => Convert.ToDouble(i)).ToArray();
        }

        /// <summary>Initializes a new instance of the Skellam distribution</summary>
        /// <param name="mu1">the rate of the first Poisson</param>
        /// <param name="mu2">the rate of the second Poisson</param>
        /// <remarks>Allows to compute the probabilities of a Poisson race</remarks>
        public SkellamDistribution(double mu1, double mu2)
            : this(mu1, mu2, new Random(Guid.NewGuid().GetHashCode()))
        { }

        #region Accessors

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean { get { return _mu1 - _mu2; } }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness { get { return (_mu1 - _mu2) * Math.Pow(_mu1 + _mu2, 1.5); } }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation { get { return Math.Sqrt(_mu1 + _mu2); } }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance { get { return _mu1 + _mu2; } }

        /// <summary>Gets the distribution's support</summary>
        public override double[] Support
        {
            get { return _support.ToArray(); }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get
            {
                Dictionary<int, double> values = _support.ToDictionary(x => Convert.ToInt32(x), x => ProbabilityDensity(x));

                int maxArg = values.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                return maxArg;
            }
        }

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return _support.Select(x => ProbabilityDensity(x)).Select(p => -p * Math.Log(p)).Sum(); }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get
            {
                double[] probabilities = _support.Select(x => ProbabilityDensity(x)).ToArray();
                int[] values = _support.Select(x => Convert.ToInt32(x)).ToArray();

                int i = 0;
                double sum = 0;
                while (sum < 0.5)
                {
                    sum += probabilities[i];
                    i++;
                }

                return values[i - 1];
            }
        }
        #endregion

        #region Methods
        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            if (p <= 0) return 0;
            if (p >= 1) throw new ArgumentOutOfRangeException("The target probability should <1");
            int k = 0;

            while (CumulativeDistribution(k) < p)
                k++;
            return k - 1;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            int k = Convert.ToInt32(x);
            return Math.Exp(-_mu1 - _mu2) * Math.Pow(_mu1 / _mu2, 0.5 * x) * Fn.ik(Math.Abs(k), 2 * Math.Sqrt(_mu1 * _mu2));
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static SkellamDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.Moments)
            {
                double mean = sample.Average(),
                    var = sample.Average(x => x * x) - mean * mean;
                if (var > Math.Abs(mean))
                    return new SkellamDistribution(0.5 * (var + mean), 0.5 * (var - mean));
            }

            throw new NotImplementedException();
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return Math.Exp(-_mu1 - _mu2 + _mu1 * Math.Exp(t) + _mu2 * Math.Exp(-t));
        }

        /// <summary>Generates a sequence of samples from the distribution</summary>
        /// <param name="size">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
            {
                double L1 = Math.Exp(-_mu1), L2 = Math.Exp(-_mu2),
                    p1 = 1, p2 = 1;
                int k1 = 0, k2 = 0;

                #region Poisson #1
                do
                {
                    k1++;
                    p1 *= (1 - _randomSource.NextDouble());

                } while (p1 > L1);
                #endregion

                #region Poisson #2
                do
                {
                    k2++;
                    p2 *= (1 - _randomSource.NextDouble());

                } while (p2 > L2);
                #endregion

                result[i] = k1 - k2;
            }
            return result;
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x).</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            List<double> subSupport = _support.Where(d => d <= x).ToList();
            if (subSupport.Count == 0) return 0;
            return subSupport.Sum(d => ProbabilityDensity(d));
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Skellam(μ1 = {0} μ2 = {1})", _mu1, _mu2);
        }
        #endregion
    }
}

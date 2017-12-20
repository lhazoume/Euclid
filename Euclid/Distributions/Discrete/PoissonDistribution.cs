using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Distributions.Discrete
{
    /// <summary>Poisson distribution</summary>
    public class PoissonDistribution : DiscreteDistribution, IParametricDistribution
    {
        #region Declarations
        private double _lambda;
        private Dictionary<int, int> _factorial = new Dictionary<int, int>();
        #endregion

        private PoissonDistribution(double lambda, Random randomSource)
        {
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            if (lambda <= 0) throw new ArgumentOutOfRangeException("The lambda should be >0");
            _lambda = lambda;
        }

        public PoissonDistribution(double lambda)
            : this(lambda, new Random(Guid.NewGuid().GetHashCode()))
        { }

        private int Factorial(int k)
        {
            if (k <= 1) return 1;
            if (_factorial.ContainsKey(k)) return _factorial[k];
            int result = k * Factorial(k - 1);
            _factorial.Add(k, result);
            return result;
        }

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return 0.5 * Math.Log(2 * Math.PI * Math.E * _lambda) - 1 / (12 * _lambda) - 1 / (24 * _lambda * _lambda); }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return _lambda; }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return Math.Round(_lambda + 1.0 / 3.0 - 0.02 / _lambda); }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return Math.Round(_lambda); }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return 1 / Math.Sqrt(_lambda); }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return Math.Sqrt(_lambda); }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return _lambda; }
        }

        /// <summary>Gets the distribution's support</summary>
        public override double[] Support
        {
            get
            {
                //TODO : implement here
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Methods

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x).</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            int k = Convert.ToInt32(Math.Floor(x));
            return Fn.IncompleteLowerGamma(k + 1, _lambda) / Factorial(k);
        }

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
            int k = Convert.ToInt32(Math.Round(x));
            if (k < 0) return 0;

            return Math.Exp(-_lambda) * LambdaOverFact(_lambda, k); // Math.Pow(_lambda, k) / Factorial(k);
        }

        public static double Probability(double x, int k)
        {
            return Math.Exp(-x) * LambdaOverFact(x, k);
        }
        public static double LogProbability(double x, int k)
        {
            if (k == 0) return -x;
            return -x + k * Math.Log(x) + Enumerable.Range(1, k).Sum(i => Math.Log(i));
        }

        private static double LambdaOverFact(double lambda, int k)
        {
            if (k == 0) return 1;

            return (lambda / k) * LambdaOverFact(lambda, k - 1);
        }

        /// <summary>Generates a sequence of samples from the distribution</summary>
        /// <param name="size">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
            {
                double L = Math.Exp(-_lambda),
                    p = 1;
                int k = 0;
                do
                {
                    k++;
                    p *= (1 - _randomSource.NextDouble());

                } while (p > L);
                result[i] = k - 1;
            }
            return result;
        }

        /// <summary>Fits the distribution to a sample of data</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public void Fit(FittingMethod method, double[] sample)
        {
            if (sample.Min() < 0) throw new ArgumentOutOfRangeException("The sample can not fit a Poisson law (all data should be>0)");
            _lambda = sample.Average();
        }
        #endregion
    }
}

﻿using System;
using System.Linq;

namespace Euclid.Distributions.Discrete
{
    /// <summary>Poisson distribution</summary>
    public class PoissonDistribution : DiscreteDistribution
    {
        #region Declarations
        private readonly double _lambda;

        private const double _supportWidthInStandardDeviations = 10;

        #endregion

        private PoissonDistribution(double lambda, Random randomSource)
        {
            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");

            if (lambda <= 0) throw new ArgumentOutOfRangeException(nameof(lambda), "The lambda should be >0");
            _lambda = lambda;

            _support = Enumerable.Range(0, Convert.ToInt32(_supportWidthInStandardDeviations * Math.Sqrt(_lambda))).Select(i => Convert.ToDouble(i)).ToArray();
        }

        /// <summary>Initializes a new instance of the Poisson distribution</summary>
        /// <param name="lambda">the rate</param>
        public PoissonDistribution(double lambda)
            : this(lambda, new Random(Guid.NewGuid().GetHashCode()))
        { }

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => 0.5 * Math.Log(2 * Math.PI * Math.E * _lambda) - 1 / (12 * _lambda) - 1 / (24 * _lambda * _lambda);

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _lambda;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => Math.Round(_lambda + 1.0 / 3.0 - 0.02 / _lambda);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => Math.Round(_lambda);

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => 1 / Math.Sqrt(_lambda);

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => Math.Sqrt(_lambda);

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _lambda;

        /// <summary>Gets the distribution's support</summary>
        public override double[] Support => _support.ToArray();
        #endregion

        #region Methods

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x).</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            int k = Convert.ToInt32(Math.Floor(x));
            return Fn.IncompleteLowerGamma(k + 1, _lambda) / Fn.Factorial(k);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            if (p <= 0) return 0;
            if (p >= 1) throw new ArgumentOutOfRangeException(nameof(p), "The target probability should <1");
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
            if (x < 0) return 0;
            if (x == 0) return Math.Exp(-x);
            return Probability(_lambda, Convert.ToInt32(x));
        }

        /// <summary>Computes the probability density of the distribution</summary>
        /// <param name="x">the rate</param>
        /// <param name="k">the value</param>
        /// <returns>a <c>double</c></returns>
        public static double Probability(double x, int k)
        {
            if (k == 0) return Math.Exp(-x);
            return Math.Exp(-x) * Math.Exp(k * Math.Log(x) - Enumerable.Range(1, k).Sum(i => Math.Log(i)));
        }

        /// <summary>Computes the log-probability density of the distribution</summary>
        /// <param name="x">the rate</param>
        /// <param name="k">the value</param>
        /// <returns>a <c>double</c></returns>
        public static double LogProbability(double x, int k)
        {
            if (k == 0) return -x;
            return -x + k * Math.Log(x) + Enumerable.Range(1, k).Sum(i => Math.Log(i));
        }

        /// <summary>Computes the derivative of the log-probability according to the rate</summary>
        /// <param name="x">the rate</param>
        /// <param name="k">the value</param>
        /// <returns>a <c>double</c></returns>
        public static double LogProbabilityDerivative(double x, int k)
        {
            return -1 + k / x;
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return Math.Exp(_lambda * (Math.Exp(t) - 1));
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

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static PoissonDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Min() < 0) throw new ArgumentOutOfRangeException(nameof(sample), "The sample can not fit a Poisson law (all data should be>0)");
            return new PoissonDistribution(sample.Average());
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Poisson(λ = {0})", _lambda);
        }
        #endregion
    }
}

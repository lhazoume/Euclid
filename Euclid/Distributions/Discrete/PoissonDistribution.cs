﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Distributions.Discrete
{
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

            return Math.Exp(-_lambda) * Math.Pow(_lambda, k) / Factorial(k);
        }

        /// <summary>Generates a sequence of samples from the distribution</summary>
        /// <param name="size">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int size)
        {
            //TODO : implement here
            throw new NotImplementedException();
        }

        public void Fit(FittingMethod method, double[] sample)
        {
            if (sample.Min() < 0) throw new ArgumentOutOfRangeException("The sample can not fit a Poisson law (all data should be>0)");
            _lambda = sample.Average();
        }
        #endregion
    }
}

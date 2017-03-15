﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Distributions.Discrete
{
    /// <summary>Abstract class base for continuous distributions</summary>
    public abstract class DiscreteDistribution : IDistribution
    {
        /// <summary>The random number generator</summary>
        protected Random _randomSource;

        /// <summary>The distribution's support </summary>
        protected double[] _support;

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public abstract double Entropy { get; }

        /// <summary>Gets the distribution's support</summary>
        public abstract double[] Support { get; }

        /// <summary>Gets the distribution's mean</summary>
        public abstract double Mean { get; }

        /// <summary>Gets the distribution's median</summary>
        public abstract double Median { get; }

        /// <summary>Gets the distribution's mode</summary>
        public abstract double Mode { get; }

        /// <summary>Gets the distribution's skewness</summary>
        public abstract double Skewness { get; }

        /// <summary>Gets the distribution's standard deviation</summary>
        public abstract double StandardDeviation { get; }

        /// <summary>Gets the distribution's variance</summary>
        public abstract double Variance { get; }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples
        /// </summary>
        public Random RandomSource
        {
            get { return _randomSource; }
            set
            {
                if (value == null) throw new ArgumentException("The random source can not be null");
                _randomSource = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public abstract double CumulativeDistribution(double x);

        /// <summary>Computes the cumulative distribution(CDF) of the distribution between x and y, i.e. P(x ≤ X ≤ y) </summary>
        /// <param name="x">the lower bound of the interval</param>
        /// <param name="y">the upper bound of the interval</param>
        /// <returns>a double</returns>
        public double CumulativeDistributionBetween(double x, double y)
        {
            return CumulativeDistribution(x > y ? x : y) - CumulativeDistribution(x > y ? y : x);
        }

        /// <summary>Computes the inverse of the cumulative distribution function</summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public abstract double InverseCumulativeDistribution(double p);

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public abstract double ProbabilityDensity(double x);

        /// <summary>Computes the probability density function's logarithm at x</summary>
        /// <param name="x">the location at which to compute the density</param>
        /// <returns>a double</returns>
        public virtual double ProbabilityLnDensity(double x)
        {
            return Math.Log(ProbabilityDensity(x));
        }
        
        /// <summary>Generates a sequence of samples from the distribution using the algorithm</summary>
        /// <param name="size">the sample's size</param>
        /// <returns>an array of double</returns>
        public virtual double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                result[i] = InverseCumulativeDistribution(_randomSource.NextDouble());
            return result;
        }
        #endregion
    }
}

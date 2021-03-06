﻿using Euclid.Histograms;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Exponential distribution class</summary>
    public class ExponentialDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _lambda, _beta;
        #endregion

        #region Constructors
        private ExponentialDistribution(double lambda, Random randomSource)
        {
            if (lambda <= 0) throw new ArgumentException("λ has to be positive");
            _lambda = lambda;
            _beta = 1 / _lambda;
            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");
            _support = new Interval(0, double.PositiveInfinity, true, false);
        }

        /// <summary>
        /// Builds a Exponential distribution
        /// </summary>
        /// <param name="lambda">the rate</param>
        public ExponentialDistribution(double lambda)
            : this(lambda, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Methods

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static ExponentialDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.Moments)
            {
                double avg = sample.Average();

                double beta = (avg * Math.Log(2) + 1) / (1 + Math.Log(2) * Math.Log(2));
                return new ExponentialDistribution(1 / beta);
            }

            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            else return 1 - Math.Exp(-_lambda * x);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return -Math.Log(1 - p) * _beta;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x < 0) return 0;
            else return _lambda * Math.Exp(-_lambda * x);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            if (t < _lambda)
                return _lambda / (_lambda - t);
            throw new ArgumentOutOfRangeException(nameof(t), "The argument of the MGF should be lower than the rate");
        }

        /// <summary> Generates a sequence of samples from the normal distribution using the algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                result[i] = -Math.Log(_randomSource.NextDouble()) * _beta;
            return result;
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Exponential(λ = {0})", _lambda);
        }

        #endregion

        #region Accessors

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's entropy </summary>
        public override double Entropy => Math.Log(Math.E * _beta);

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _beta;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _beta * Math.Log(2);

        /// <summary>Gets the distribution's mode </summary>
        public override double Mode => 0;

        /// <summary> Gets the distribution's mode</summary>
        public override double Skewness => 2;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _beta * _beta;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _beta;
        #endregion
    }
}

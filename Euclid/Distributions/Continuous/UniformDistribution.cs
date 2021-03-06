﻿using Euclid.Histograms;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Uniform distribution class</summary>
    public class UniformDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _a, _b, _d, _m;
        #endregion

        #region Constructors
        private UniformDistribution(double a, double b, Random randomSource)
        {
            if (a >= b) throw new ArgumentException("the interval is not defined");
            _a = a;
            _b = b;
            _d = _b - _a;
            _m = 0.5 * (_b + _a);

            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");

            _support = new Interval(_a, _b, true, true);
        }

        /// <summary>Builds a Uniform distribution</summary>
        /// <param name="a">the support's lower bound</param>
        /// <param name="b">the support's upper bound</param>
        public UniformDistribution(double a, double b)
            : this(a, b, new Random(Guid.NewGuid().GetHashCode()))
        { }

        /// <summary>Builds a standard Uniform distribution </summary>
        public UniformDistribution()
            : this(0, 1)
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Math.Log(_d);

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _m;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _m;

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _m;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => 0;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _d / Math.Sqrt(12);

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _d * _d / 12;

        #endregion

        #region Methods
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static UniformDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.Moments)
            {
                double avg = sample.Average(),
                    stdev = Math.Sqrt(12 * (sample.Average(x => x * x) - avg * avg));
                return new UniformDistribution(avg - stdev, avg + stdev);
            }
            return new UniformDistribution(sample.Min(), sample.Max());
        }

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
            if (_support.Contains(x)) return 1 / _d;
            return 0;
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return (Math.Exp(t * _b) - Math.Exp(t * _a)) / (t * (_b - _a));
        }

        /// <summary>Generates a sequence of samples from the normal distribution using the algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                result[i] = _a + _d * _randomSource.NextDouble();
            return result;
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Uniform(a = {0} b = {1})", _a, _b);
        }
        #endregion
    }
}

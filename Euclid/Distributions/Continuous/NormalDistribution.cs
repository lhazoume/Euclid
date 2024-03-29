﻿using Euclid.Histograms;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Normal distribution class</summary>
    public class NormalDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _mean, _standardDeviation;
        #endregion

        #region Constructors
        /// <summary>Builds a normal distribution</summary>
        /// <param name="mean">the distribution's mean</param>
        /// <param name="standardDeviation">the distributions's standard deviation</param>
        /// <param name="randomSource">the random source</param>
        public NormalDistribution(double mean, double standardDeviation, Random randomSource)
        {
            _mean = mean;
            if (standardDeviation < 0) throw new ArgumentException("The standard deviation can not be negative");
            _standardDeviation = standardDeviation;
            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        /// <summary>Builds a standard normal distribution</summary>
        public NormalDistribution()
            : this(0, 1)
        { }

        /// <summary>Builds a normal distribution</summary>
        /// <param name="mean">the average</param>
        /// <param name="standardDeviation">the standard deviation</param>
        public NormalDistribution(double mean, double standardDeviation)
            : this(mean, standardDeviation, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Methods

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static NormalDistribution Fit(FittingMethod method, double[] sample)
        {
            double avg = sample.Average(),
                stdev = Math.Sqrt(sample.Select(x => x * x).Average() - avg * avg);
            return new NormalDistribution(avg, stdev);
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            return Fn.Phi((x - _mean) / _standardDeviation);
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return Fn.GaussBell((x - _mean) / _standardDeviation);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _mean + _standardDeviation * Fn.InvPhi(p);
        }

        /// <summary>Generates a sequence of samples from the normal distribution using the algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            double[] result = new double[numberOfPoints];
            int processorCount = Environment.ProcessorCount;

            #region Initialize the seeds
            int[] seeds = new int[processorCount];
            for (int p = 0; p < processorCount; p++)
                seeds[p] = _randomSource.Next();
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
                        result[ix] = _mean + _standardDeviation * x;
                    if (ix + 1 >= numberOfPoints)
                        break;
                    else
                        result[ix + 1] = _mean + _standardDeviation * y;
                }

            });

            return result;
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return Math.Exp(_mean * t + 0.5 * Math.Pow(_standardDeviation * t, 2));
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("N(μ = {0}, σ = {1})", _mean, _standardDeviation);
        }

        #endregion

        #region Accessors
        /// <summary>Gets the entropy of the normal distribution</summary>
        public override double Entropy => Math.Log(_standardDeviation * Math.Sqrt(2 * Math.PI * Math.E));

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the mean(μ) of the normal distribution</summary>
        public override double Mean => _mean;

        /// <summary>Gets the median of the normal distribution</summary>
        public override double Median => _mean;

        /// <summary>Gets the mode of the normal distribution</summary>
        public override double Mode => _mean;

        /// <summary>Gets the skewness of the normal distribution</summary>
        public override double Skewness => 0;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _standardDeviation;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _standardDeviation * _standardDeviation;
        #endregion
    }
}

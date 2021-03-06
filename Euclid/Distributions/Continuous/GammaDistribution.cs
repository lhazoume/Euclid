﻿using Euclid.Histograms;
using Euclid.Solvers;
using Euclid.Solvers.SingleVariableSolver;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Gamma distribution class</summary>
    public class GammaDistribution : ContinuousDistribution
    {
        #region Variables
        private readonly double _k, _theta, _cdfFactor, _pdfFactor;
        #endregion

        #region Constructors
        private GammaDistribution(double k, double theta, Random randomSource)
        {
            if (k <= 0) throw new ArgumentException("the shape has to be positive");
            if (theta <= 0) throw new ArgumentException("the scale has to be positive");
            _k = k;
            _theta = theta;
            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");

            _support = new Interval(0, double.PositiveInfinity, false, false);

            _cdfFactor = 1 / Fn.Gamma(_k);
            _pdfFactor = Math.Pow(_theta, -_k) * _cdfFactor;
        }

        /// <summary>Builds a Gamma distribution</summary>
        /// <param name="k">the shapee</param>
        /// <param name="theta">the scale</param>
        public GammaDistribution(double k, double theta)
            : this(k, theta, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => _k + Math.Log(_theta) + Math.Log(Fn.Gamma(_k)) + (1 - _k) * Fn.DiGamma(_k);

        /// <summary>Gets the distribution's support </summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's mean </summary>
        public override double Mean => _k * _theta;

        /// <summary>Gets the distribution's median </summary>
        public override double Median => InverseCumulativeDistribution(0.5);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _k >= 1 ? (_k - 1) * _theta : double.NaN;

        /// <summary>Gets the distribution's skewness </summary>
        public override double Skewness => 2 / Math.Sqrt(_k);

        /// <summary>Gets the distribution's standard deviation </summary>
        public override double StandardDeviation => _theta * Math.Sqrt(_k);

        /// <summary>Gets the distribution's variance </summary>
        public override double Variance => _k * _theta * _theta;
        #endregion

        #region Methods

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static GammaDistribution Fit(FittingMethod method, double[] sample)
        {
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x <= 0) return 0;
            return _cdfFactor * Fn.IncompleteLowerGamma(_k, x / _theta);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(1, CumulativeDistribution, 100);
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x <= 0) return 0;
            return _pdfFactor * Math.Pow(x, _k - 1) * Math.Exp(-x / _theta);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            if (_theta * t < 1)
                return Math.Pow(1 - _theta * t, -_k);
            throw new ArgumentOutOfRangeException(nameof(t), "The argument of the MGF should be lower than the rate");
        }

        /// <summary>Generates a sequence of samples using the Ahrens-Dieter algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            int n = Convert.ToInt32(Math.Floor(_k));
            double delta = _k - n;
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                #region Int part
                double sumLog = 0;
                for (int k = 0; k < n; k++)
                    sumLog -= Math.Log(1 - _randomSource.NextDouble());
                #endregion

                #region Remainder
                double e = GenerateAhrensDieterRejection(_randomSource, delta);
                #endregion

                result[i] = _theta * (e + sumLog);
            }
            return result;
        }

        private static double GenerateAhrensDieterRejection(Random random, double delta)
        {
            double u = 1 - random.NextDouble(),
                v = 1 - random.NextDouble(),
                w = 1 - random.NextDouble(),
                e, n;
            if (u * (Math.E + delta) <= Math.E)
            {
                e = Math.Pow(v, 1 / delta);
                n = w * Math.Pow(e, delta - 1);
            }
            else
            {
                e = 1 - Math.Log(v);
                n = w * Math.Exp(-e);
            }

            if (n > Math.Pow(e, delta - 1) * Math.Exp(-e))
                return e;
            else
                return GenerateAhrensDieterRejection(random, delta);

        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Γ(k = {0} θ = {1})", _k, _theta);
        }
        #endregion
    }
}

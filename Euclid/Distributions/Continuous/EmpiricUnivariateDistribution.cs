﻿using Euclid.Distributions.Continuous.Kernels;
using Euclid.Histograms;
using Euclid.Solvers;
using Euclid.Solvers.SingleVariableSolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Distributions.Continuous
{

    /// <summary>Empiric univariate distribution based on kernel functions</summary>
    public class EmpiricUnivariateDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double[] _weights, _values;
        private readonly double[,] _buckets;
        private readonly double _h, _sumWeights, _m1, _m2, _m3;
        private readonly int _n;
        private readonly IDensityKernel _kernel;
        #endregion

        private EmpiricUnivariateDistribution(IList<double> weights, IList<double> values, double h, IDensityKernel kernel, Random randomSource)
        {
            if (weights == null || values == null ||
                weights.Count == 0 || values.Count == 0 ||
                weights.Count != values.Count)
                throw new ArgumentException("The weights and values are not right");
            _n = weights.Count;
            _weights = new double[_n];

            _values = new double[_n];
            _h = h;
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");

            _sumWeights = 0;
            _m1 = 0;
            _m2 = 0;
            _m3 = 0;

            for (int i = 0; i < _n; i++)
            {
                _weights[i] = weights[i];
                _sumWeights += _weights[i];
                _values[i] = values[i];
                _m1 += _weights[i] * _values[i];
                _m2 += _weights[i] * Math.Pow(_values[i], 2);
                _m3 += _weights[i] * Math.Pow(_values[i], 3);
            }
            _m1 /= _sumWeights;
            _m2 = _m2 / _sumWeights + _h * h * _kernel.Variance;
            _m3 = _m3 / _sumWeights + 3 * _h * _h * _kernel.Variance * _m1;

            #region Prepare for the inverse CDF
            double[] orderedValues = _values.Distinct().OrderBy(d => d).ToArray();
            _buckets = new double[orderedValues.Length + 2, 2];
            _buckets[0, 0] = _values.Min() - _h;
            _buckets[0, 1] = 0;
            for (int i = 0; i < orderedValues.Length; i++)
            {
                _buckets[1 + i, 0] = orderedValues[i];
                _buckets[1 + i, 1] = CumulativeDistribution(orderedValues[i]);
            }
            _buckets[orderedValues.Length + 1, 0] = _values.Max() + _h;
            _buckets[orderedValues.Length + 1, 1] = 1;
            #endregion

            _support = new Interval(_values.Min() - _h, _values.Max() + _h);
        }

        #region Creators
        /// <summary>Creates a new empiric univariate distribution</summary>
        /// <param name="weights">the weights</param>
        /// <param name="values">the values</param>
        /// <param name="h">the bandwidth</param>
        /// <param name="kernel">the kernel function</param>
        /// <returns>a <c>EmpiricUnivariateDistribution</c></returns>
        public static EmpiricUnivariateDistribution Create(IList<double> weights, IList<double> values, double h, IDensityKernel kernel)
        {
            return new EmpiricUnivariateDistribution(weights, values, h, kernel, new Random(Guid.NewGuid().GetHashCode()));
        }
        #endregion

        #region Accessors

        /// <summary>Gets the distribution's median</summary>
        public override double Median => InverseCumulativeDistribution(0.5);

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _m1;

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _values[Array.IndexOf(_weights, _weights.Max())];

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => Math.Sqrt(Variance);

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _m2 - _m1 * _m1;

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;



        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get
            {
                double sigma = this.StandardDeviation;
                return (_m3 + 3 * _m1 * _m2 + 2 * Math.Pow(_m1, 3)) / Math.Pow(sigma, 3);
            }
        }

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy { get { throw new NotImplementedException(); } }
        #endregion

        #region Methods

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            double sum = 0;
            for (int i = 0; i < _weights.Length; i++)
                sum += _weights[i] * _kernel.IntegralK((x - _values[i]) / _h);
            return sum / (_sumWeights);
        }

        /// <summary>Computes the inverse of the cumulative distribution function</summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            int i = 0;
            while (_buckets[i, 1] < p)
                i++;

            Bracketing solver = new Bracketing(_buckets[i - 1, 0], _buckets[i, 0], CumulativeDistribution, BracketingMethod.Dichotomy, 10000) { Tolerance = 0.0001 };
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            double sum = 0;
            for (int i = 0; i < _weights.Length; i++)
                sum += _weights[i] * _kernel.K((x - _values[i]) / _h);
            return sum / (_h * _sumWeights);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            double result = 0;
            for (int i = 0; i < _n; i++)
                result += Math.Exp(t * _values[i]) * _weights[i];
            return result;
        }

        #endregion
    }
}

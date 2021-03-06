﻿using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Bounded normal distribution class
    /// </summary>
    public class BoundedNormalDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _mu,
            _sigma, _sigma2,
            _a, _b,
            _alpha, _beta,
            _phiAlpha, _phiBeta,
            _gbAlpha, _gbBeta,
            _dGb,
            _Z;
        #endregion

        #region Constructors
        private BoundedNormalDistribution(double mu, double sigma, double a, double b, Random randomSource)
        {
            if (sigma <= 0) throw new ArgumentException("sigma has to be positive");
            if (a >= b) throw new ArgumentException("the interval is not defined");
            _sigma = sigma;
            _sigma2 = _sigma * _sigma;
            _mu = mu;

            _a = a;
            _b = b;

            _alpha = double.IsNegativeInfinity(_a) ? double.NegativeInfinity : (_a - _mu) / _sigma;
            _beta = double.IsPositiveInfinity(_b) ? double.PositiveInfinity : (_b - _mu) / _sigma;

            _phiAlpha = double.IsNegativeInfinity(_a) ? 0 : Fn.Phi(_alpha);
            _phiBeta = double.IsPositiveInfinity(_b) ? 1 : Fn.Phi(_beta);

            _gbAlpha = double.IsNegativeInfinity(_a) ? 0 : Fn.GaussBell(_alpha);
            _gbBeta = double.IsPositiveInfinity(_b) ? 0 : Fn.GaussBell(_beta);

            _dGb = (double.IsNegativeInfinity(_a) ? 0 : (_alpha * _gbAlpha)) - (double.IsPositiveInfinity(_b) ? 0 : (_beta * _gbBeta));

            _Z = _phiBeta - _phiAlpha;
            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");
            _support = new Interval(_a, _b, true, true);
        }

        /// <summary>
        /// Builds a truncated normal distribution
        /// </summary>
        /// <param name="mu">the location</param>
        /// <param name="sigma">the scale</param>
        /// <param name="a">the interval's lower bound</param>
        /// <param name="b">the interval's upper bound</param>
        public BoundedNormalDistribution(double mu, double sigma, double a, double b)
            : this(mu, sigma, a, b, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Math.Log(Math.Sqrt(2 * Math.PI * Math.E) * _sigma * _Z) + _dGb / (2 * _Z);

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _mu + (_gbAlpha - _gbBeta) * _sigma / _Z;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _mu + _sigma * Fn.InvPhi(0.5 * (_phiBeta + _phiAlpha));

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get
            {
                if (!double.IsNegativeInfinity(_a) && _mu < _a) return _a;
                if (!double.IsPositiveInfinity(_b) && _mu > _b) return _b;
                return _mu;
            }
        }

        /// <summary>
        /// Gets the distribution's skewness
        /// </summary>
        /// <remarks>using Shah and Jaiswal (1966)</remarks>
        public override double Skewness
        {
            get
            {
                double k0 = double.IsNegativeInfinity(_a) ? 0 : _alpha,
                    k1 = double.IsPositiveInfinity(_b) ? 0 : _beta,
                    z0 = _gbAlpha / _Z,
                    z1 = _gbBeta / _Z,
                    dz = z1 - z0, dkz = k1 * z1 - k0 * z0,
                    V = 1 - dkz - Math.Pow(dz, 2),
                    s = -Math.Pow(V, -1.5) * (2 * Math.Pow(dz, 3) + (3 * dkz - 1) * dz + k1 * k1 * z1 - k0 * k0 * z0);
                return s;
            }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => Math.Sqrt(Variance);

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _sigma2 * (1 + _dGb / _Z - Math.Pow((_gbAlpha - _gbBeta) / _Z, 2));
        #endregion

        #region Methods

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static BoundedNormalDistribution Fit(FittingMethod method, double[] sample)
        {
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution function at x</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < _support.LowerBound.Value) return 0;
            else if (x > _support.UpperBound.Value) return 1;
            return (Fn.Phi((x - _mu) / _sigma) - _phiAlpha) / _Z;
        }

        /// <summary>Computes the inverse of the cumulative distribution function</summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _mu + _sigma * Fn.InvPhi(_phiAlpha + p * _Z);
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (!_support.Contains(x)) return 0;
            return Fn.GaussBell((x - _mu) / _sigma) / (_sigma * _Z);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return Math.Exp(_mu * t + _sigma2 * t * t / 2) * (Fn.Phi(_beta - _sigma * t) - Fn.Phi(_alpha - _sigma * t)) / (Fn.Phi(_beta) - Fn.Phi(_alpha));
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("BoundedN(μ = {0}, σ = {1}, a = {2}, b = {3})", _mu, _sigma, _a, _b);
        }
        #endregion
    }
}

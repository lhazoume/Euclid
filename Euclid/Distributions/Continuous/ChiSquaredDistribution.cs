using Euclid.Histograms;
using Euclid.Solvers;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Chi squared distribution
    /// </summary>
    public class ChiSquaredDistribution : ContinousDistribution
    {
        #region Declarations
        private int _k;
        #endregion

        #region Constructors
        private ChiSquaredDistribution(int k, Random randomSource)
        {
            if (k <= 0) throw new ArgumentException("degrees of freedom has to be positive");
            _k = k;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(0, double.PositiveInfinity, true, false);
        }

        /// <summary>
        /// Builds a Chi² distribution
        /// </summary>
        /// <param name="k">the number of freedom degrees</param>
        public ChiSquaredDistribution(int k)
            : this(k, new Random(DateTime.Now.Millisecond))
        { }
        #endregion

        #region Accessors
        /// <summary>
        /// Gets the distribution's entropy
        /// </summary>
        public override double Entropy
        {
            get
            {
                double halfK = 0.5 * _k;
                return halfK + Math.Log(2 * Fn.Gamma(halfK)) + (1 - halfK) * Fn.DiGamma(halfK);
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the distribution's support
        /// </summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>
        /// Gets the distribution's mean
        /// </summary>
        public override double Mean
        {
            get { return _k; }
        }

        /// <summary>
        /// Gets the distribution's median
        /// </summary>
        public override double Median
        {
            get { return _k * Math.Pow(1 - 2 / (9 * _k), 3); }
        }

        /// <summary>
        /// Gets the distribution's mode
        /// </summary>
        public override double Mode
        {
            get { return Math.Max(_k - 2, 0); }
        }

        /// <summary>
        /// Gets the distribution's skewness
        /// </summary>
        public override double Skewness
        {
            get { return Math.Sqrt(8 / _k); }
        }

        /// <summary>
        /// Gets the dsitribution's standard deviation
        /// </summary>
        public override double StandardDeviation
        {
            get { return Math.Sqrt(2 * _k); }
        }

        /// <summary>
        /// Gets the distribution's variance
        /// </summary>
        public override double Variance
        {
            get { return 2 * _k; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x <= 0) return 0;
            return Fn.igam(0.5 * _k, 0.5 * x) / Fn.Gamma(0.5 * _k);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(_k, CumulativeDistribution, 10);
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>
        /// Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x < 0) return 0;
            return Math.Pow(0.5 * x, 0.5 * _k - 1) * Math.Exp(-0.5 * x) / (x * Fn.Gamma(0.5 * _k));
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the algorithm
        /// </summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints)
        {
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                result[i] = InverseCumulativeDistribution(_randomSource.NextDouble());
            return result;
        }

        #endregion
    }
}

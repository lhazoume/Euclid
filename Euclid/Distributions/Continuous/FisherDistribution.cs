using Euclid.Histograms;
using Euclid.Solvers;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Fisher distribution class
    /// </summary>
    public class FisherDistribution : ContinuousDistribution, IParametricDistribution
    {
        #region Declarations
        private double _d1, _d2;
        #endregion

        private FisherDistribution(double d1, double d2, Random randomSource)
        {
            if (d1 <= 0) throw new ArgumentException("The d1 can not be negative");
            _d1 = d1;

            if (d2 <= 0) throw new ArgumentException("The d2 can not be negative");
            _d2 = d2;

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(0, double.PositiveInfinity, true, false);
        }

        #region Creators
        /// <summary>
        /// Creates a new Fisher distribution
        /// </summary>
        /// <param name="d1">the first number of freedom degrees</param>
        /// <param name="d2">the second number of freedom degrees</param>
        /// <returns>a <c>FisherDistribution</c></returns>
        public static FisherDistribution Create(double d1, double d2)
        {
            return new FisherDistribution(d1, d2, new Random(Guid.NewGuid().GetHashCode()));
        }
        #endregion

        #region Accessors

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return _d2 > 2 ? _d2 / (_d2 - 2) : double.NaN; }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return InverseCumulativeDistribution(0.5); }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return _d1 > 2 ? (_d1 - 2) * _d2 / (_d1 * (_d2 + 2)) : double.NaN; }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return _d2 > 6 ? (2 * _d1 + _d2 - 2) / (_d2 - 6) * Math.Sqrt(8 * (_d2 - 4) / (_d1 * (_d1 + _d2 - 2))) : double.NaN; }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return _d2 > 4 ? _d2 / (_d2 - 2) * Math.Sqrt(2 * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4))) : double.NaN; }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return _d2 > 4 ? 2 * Math.Pow(_d2 / (_d2 - 2), 2) * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4)) : double.NaN; }
        }

        #endregion

        #region Methods

        /// <summary>Fits the distribution to a sample of data</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public void Fit(FittingMethod method, double[] sample)
        {
            //TODO : implement here
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            return Fn.IncompleteRegularizedBeta(_d1 * x / (_d1 * x + _d2), 0.5 * _d1, 0.5 * _d2);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function
        /// </summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(1, CumulativeDistribution, 100);
            solver.SlopeTolerance = 1e-10;
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
            return Math.Sqrt(Math.Pow(_d1 * x, _d1) * Math.Pow(_d2, _d2) / Math.Pow(_d1 * x + _d2, _d1 + _d2)) / (x * Fn.Beta(0.5 * _d1, 0.5 * _d2));
        }

        #endregion
    }
}

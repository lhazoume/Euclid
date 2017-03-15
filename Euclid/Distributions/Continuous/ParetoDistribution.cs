using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    ///¨Pareto distribution class
    /// </summary>
    public class ParetoDistribution : ContinuousDistribution
    {
        #region Declarations
        private double _xm, _alpha;
        #endregion

        #region Constructors
        private ParetoDistribution(double xm, double alpha, Random randomSource)
        {
            if (xm <= 0) throw new ArgumentException("xm has to be positive");
            if (alpha <= 0) throw new ArgumentException("alpha has to be positive");
            _alpha = alpha;
            _xm = xm;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(_xm, double.PositiveInfinity, true, false);
        }

        /// <summary>
        /// Builds a Pareto distribution
        /// </summary>
        /// <param name="xm">the scale</param>
        /// <param name="alpha">the shape</param>
        public ParetoDistribution(double xm, double alpha)
            : this(xm, alpha, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return Math.Log((_xm / _alpha) * Math.Exp(1 + 1 / _alpha)); }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support
        {
            get { return _support; }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get
            {
                if (_alpha <= 1) return double.MaxValue;
                else return _alpha * _xm / (_alpha - 1);
            }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get { return _xm * Math.Pow(2, 1 / _alpha); }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return _xm; }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get
            {
                if (_alpha <= 3) return double.MaxValue;
                else return 2 * (1 + _alpha) / (_alpha - 3) * Math.Sqrt((_alpha - 2) / _alpha);
            }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get
            {
                if (_alpha <= 2) return double.MaxValue;
                else return (_xm / (_alpha - 1)) * Math.Sqrt(_alpha / (_alpha - 2));
            }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get
            {
                if (_alpha <= 2) return double.MaxValue;
                else return Math.Pow(_xm / (_alpha - 1), 2) * _alpha / (_alpha - 2);
            }
        }
        #endregion

        #region Methods

        public override void Fit(FittingMethod method, double[] sample)
        {
            //TODO : implement here
            throw new NotImplementedException();
        }

        /// <summary>
        /// Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x >= _xm) return Math.Pow(1 - (_xm / x), _alpha);
            else return 0;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _xm / Math.Exp(Math.Log(1 - p) / _alpha);
        }

        /// <summary>
        /// Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x >= _xm) return _alpha * Math.Pow(_xm / x, _alpha) / x;
            else return 0;
        }

        /// <summary>
        /// Builds a sample of random variables under this distribution
        /// </summary>
        /// <param name="size">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                result[i] = _xm / Math.Exp(Math.Log(_randomSource.NextDouble()) / _alpha);
            return result;
        }
        #endregion
    }
}

using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Student distribution class</summary>
    public class StudentDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly int _k;
        #endregion

        #region Constructors
        private StudentDistribution(int k, Random randomSource)
        {
            if (k <= 0) throw new ArgumentException("the freedom degrees have to be positive");
            _randomSource = randomSource ?? throw new ArgumentException("The random source can not be null");
            _k = k;
            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        public StudentDistribution(int k)
            : this(k, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Accessors
        public override double Mean => _k > 1 ? 0 : double.NaN;

        public override double Median => 0;

        public override double Mode => 0;

        public override double Skewness => _k > 3 ? 0 : double.NaN;

        public override double Variance => _k > 2 ? _k / (_k + 2.0) : double.PositiveInfinity;

        public override double StandardDeviation => _k > 2 ? Math.Sqrt(_k / (_k + 2.0)) : double.PositiveInfinity;

        public override Interval Support => _support;

        #endregion


        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => throw new NotImplementedException();



        public override double CumulativeDistribution(double x)
        {
            throw new NotImplementedException();
        }

        public override double InverseCumulativeDistribution(double p)
        {
            throw new NotImplementedException();
        }

        public override double MomentGeneratingFunction(double t)
        {
            throw new NotImplementedException();
        }

        #region Methods
        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return Fn.Gamma(0.5 * (_k + 1)) * Math.Pow(1 + x * x / _k, -0.5 * (_k + 1)) / (Math.Sqrt(Math.PI * _k) * Fn.Gamma(0.5 * _k));
        }

        #endregion
    }
}

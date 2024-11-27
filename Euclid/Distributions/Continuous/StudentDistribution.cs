using Euclid.DataStructures;
using Euclid.Histograms;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Student distribution class</summary>
    public class StudentDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly int _k;
        #endregion

        #region Constructors
        public StudentDistribution(int k)
        {
            if (k <= 0) throw new ArgumentException("the freedom degrees have to be positive");
            _k = k;
            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }
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


        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            throw new NotImplementedException();
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            throw new NotImplementedException();
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            throw new NotImplementedException();
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static StudentDistribution Fit(double[] sample)
        {
            return Fit(FittingMethod.Moments, sample);
        }
        /// <summary>Fits the distribution to a sample of data</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static StudentDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.Moments)
            {
                double mean = sample.Average();
                double sigma = Math.Sqrt(sample.Select(x => x * x).Average() - mean * mean);
                int nu = (int)Math.Round(-2 * sigma / (1 - sigma));

                return new StudentDistribution(nu);
            }
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

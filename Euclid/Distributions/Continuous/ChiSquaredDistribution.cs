using System;
using System.Linq;
using Euclid.Histograms;
using Euclid.Solvers.SingleVariableSolver;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Chi squared distribution</summary>
    public class ChiSquaredDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly int _freedomDegrees;
        #endregion

        #region Constructors
        /// <summary>Builds a chi² distribution</summary>
        /// <param name="k">the number of freedom degrees</param>
        public ChiSquaredDistribution(int k)
        {
            if (k <= 0) throw new ArgumentException("the degrees of freedom has to be positive");
            _freedomDegrees = k;

            _support = new Interval(0, double.PositiveInfinity, true, false);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _freedomDegrees;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _freedomDegrees * Math.Pow(1 - 2 / (9 * _freedomDegrees), 3);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => Math.Max(_freedomDegrees - 2, 0);

        /// <summary>Gets the dsitribution's standard deviation</summary>
        public override double StandardDeviation => Math.Sqrt(2 * _freedomDegrees);

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => 2 * _freedomDegrees;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => Math.Sqrt(8 / _freedomDegrees);

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get
            {
                double halfK = 0.5 * _freedomDegrees;
                return halfK + Math.Log(2 * Fn.Gamma(halfK)) + (1 - halfK) * Fn.DiGamma(halfK);
            }
        }

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's freedom degrees parameter</summary>
        public double FreedomDegrees => _freedomDegrees;
        #endregion

        #region Methods
        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            return (x <= 0) ? 0 : Fn.IncompleteRegularizedLowerGamma(0.5 * _freedomDegrees, 0.5 * x);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            double epsilon = 1e-8;
            int m = 1;
            while (CumulativeDistribution(Math.Pow(2, m)) < p)
                m++;
            double bracketingLowBound = m == 1 ? 0 : Math.Pow(2, m - 1),
                bracketingUpBound = Math.Pow(2, m);
            int optimalSteps = (int)Math.Ceiling(m - 1 - Math.Log(epsilon) / Math.Log(2));

            Bracketing solver = new Bracketing(bracketingLowBound, bracketingUpBound, CumulativeDistribution, BracketingMethod.Dichotomy, optimalSteps);
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return (x <= 0) ? 0 : Math.Pow(0.5 * x, 0.5 * _freedomDegrees - 1) * Math.Exp(-0.5 * x) / (2 * Fn.Gamma(0.5 * _freedomDegrees));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            if (t < 0.5)
                return Math.Pow(1 - 2 * t, -0.5 * _freedomDegrees);
            throw new ArgumentOutOfRangeException(nameof(t), "the argument of the MGF should be lower than 0.5");
        }

        /// <summary>Builds a sample of random variables under this distribution</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {
            NormalDistribution N = new NormalDistribution();
            double[] normalsample = N.Sample(_freedomDegrees * numberOfPoints, seed),
                sample = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                sample[i] = 0;
                for (int j = 0; j < _freedomDegrees; j++)
                    sample[i] += Math.Pow(normalsample[i * _freedomDegrees + j], 2);
            }
            return sample;
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static ChiSquaredDistribution Fit(double[] sample) => Fit(FittingMethod.Moments, sample);

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static ChiSquaredDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Length == 0)
                throw new ArgumentException("the sample can't be empty");
            if (sample.Any(d => d < 0))
                throw new ArgumentOutOfRangeException(nameof(sample), "the sample can't be lower or equal to 0");
            if (method == FittingMethod.Moments)
            {
                int k = (int)Math.Round(sample.Average());
                return new ChiSquaredDistribution(k);
            }
            throw new NotImplementedException();
        }


        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format($"Χ²(k = {_freedomDegrees})");
        }
        #endregion
    }
}

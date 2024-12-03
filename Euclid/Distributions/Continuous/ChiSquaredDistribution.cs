using Euclid.Histograms;
using Euclid.Solvers;
using Euclid.Solvers.SingleVariableSolver;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Chi squared distribution</summary>
    public class ChiSquaredDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly int _freedomDegrees;
        #endregion

        #region Constructors
        /// <summary>Builds a Chi² distribution</summary>
        /// <param name="k">the number of freedom degrees</param>
        public ChiSquaredDistribution(int k)
        {
            if (k <= 0) throw new ArgumentException("degrees of freedom has to be positive");
            _freedomDegrees = k;

            _support = new Interval(0, double.PositiveInfinity, true, false);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get
            {
                double halfK = 0.5 * _freedomDegrees;
                return halfK + Math.Log(2 * Fn.Gamma(halfK)) + (1 - halfK) * Fn.DiGamma(halfK);
            }
        }

        /// <summary>Gets the distribution's freedom degrees</summary>
        public double FreedomDegrees => _freedomDegrees;

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _freedomDegrees;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _freedomDegrees * Math.Pow(1 - 2 / (9 * _freedomDegrees), 3);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => Math.Max(_freedomDegrees - 2, 0);

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => Math.Sqrt(8 / _freedomDegrees);

        /// <summary>Gets the dsitribution's standard deviation</summary>
        public override double StandardDeviation => Math.Sqrt(2 * _freedomDegrees);

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => 2 * _freedomDegrees;

        #endregion

        #region Methods
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static ChiSquaredDistribution Fit(double[] sample)
        {
            return Fit(FittingMethod.Moments, sample);
        }
        /// <summary>Generates a sequence of samples from the normal distribution using the algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {
            double[] sample = new double[numberOfPoints];
            NormalDistribution N = new NormalDistribution();
            double[] normalsample = N.Sample(_freedomDegrees * numberOfPoints, seed).Select(x => x * x).ToArray();
            for (int i = 0; i < numberOfPoints; i++)
            {
                sample[i] =normalsample.Skip(i*_freedomDegrees).Take(_freedomDegrees).Sum();
            } 
            return sample;
        }
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static ChiSquaredDistribution Fit(FittingMethod method, double[] sample)
        { 
            if (method == FittingMethod.Moments) {
                int k = (int)Math.Round(sample.Average());
                return new ChiSquaredDistribution(k);
            } else { throw new NotImplementedException();}
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x <= 0) return 0;
            return Fn.IncompleteRegularizedLowerGamma(0.5 * _freedomDegrees, 0.5 * x);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            Bracketing solver = new Bracketing(0,10000000, CumulativeDistribution,BracketingMethod.Dichotomy, 1000);
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x < 0) return 0;
            return Math.Pow(0.5 * x, 0.5 * _freedomDegrees - 1) * Math.Exp(-0.5 * x) / (2 * Fn.Gamma(0.5 * _freedomDegrees));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            if (t < 0.5)
                return Math.Pow(1 - 2 * t, -0.5 * _freedomDegrees);
            throw new ArgumentOutOfRangeException(nameof(t), "The argument of the MGF should be lower than 0.5");
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Χ²(k = {0})", _freedomDegrees);
        }
        #endregion
    }
}

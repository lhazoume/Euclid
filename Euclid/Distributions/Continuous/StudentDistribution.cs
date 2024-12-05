using Euclid.DataStructures;
using Euclid.Histograms;
using Euclid.Optimizers;
using Euclid.Solvers.SingleVariableSolver;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Student distribution class</summary>
    public class StudentDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _k;
        #endregion

        #region Constructors
        /// <summary>Builds a Gamma distribution</summary>
        /// <param name="k">the degrees of freedom</param>
        public StudentDistribution(double k)
        {
            if (k <= 0) throw new ArgumentException("the freedom degrees have to be positive");
            _k = k;
            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the degrees of freedom of the distribution</summary>
        public double FreedomDegrees => _k;

        /// <summary>Gets the distribution's mean </summary>
        public override double Mean => _k > 1 ? 0 : double.NaN;

        /// <summary>Gets the distribution's median </summary>
        public override double Median => 0;

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => 0;

        /// <summary>Gets the distribution's skewness </summary>
        public override double Skewness => _k > 3 ? 0 : double.NaN;

        /// <summary>Gets the distribution's variance </summary>
        public override double Variance => _k > 2 ? _k / (_k + 2.0) : double.PositiveInfinity;

        /// <summary>Gets the distribution's standard deviation </summary>
        public override double StandardDeviation => _k > 2 ? Math.Sqrt(_k / (_k + 2.0)) : double.PositiveInfinity;

        /// <summary>Gets the distribution's support </summary>
        public override Interval Support => _support;

        #endregion


        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => throw new NotImplementedException();


        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            
            return (x>=0) ? 1-0.5*Fn.IncompleteRegularizedBeta( _k/(x*x+_k),_k*0.5,0.5) : 0.5 * Fn.IncompleteRegularizedBeta(_k / (x * x + _k), _k*0.5, 0.5);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(-0.05,CumulativeDistribution,1000);
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            throw new NotImplementedException();
        }

        /// <summary>Generates a sequence of samples from the normal distribution using the algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {
            if ( _k == Math.Ceiling(_k)) {
                NormalDistribution normal = new NormalDistribution();
                ChiSquaredDistribution chi = new ChiSquaredDistribution((int)_k);
                double[] y1 = normal.Sample(numberOfPoints, seed),
                    y2 = chi.Sample(numberOfPoints, seed),
                    sample = new double[numberOfPoints];
                for (int i = 0; i < numberOfPoints; i++) { 
                    sample[i] = y1[i] / Math.Sqrt(y2[i] / _k); 
                }
                return sample;
            } else {
                Random random = new Random(seed);
                double[] result = new double[numberOfPoints];
                for (int i = 0; i < numberOfPoints; i++)
                    result[i] = InverseCumulativeDistribution(random.NextDouble());
                return result;
            }
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static StudentDistribution Fit(double[] sample) => Fit(FittingMethod.MaximumLikelihood, sample);

        /// <summary>Fits the distribution to a sample of data</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static StudentDistribution Fit(FittingMethod method, double[] sample)
        {
            int n = sample.Length;
            if (method == FittingMethod.Moments)
            {
                double mean = 0.0,
                    variance = 0.0;
                for (int i = 0;i<n; i++)
                {
                    mean += sample[i];
                    variance += sample[i]*sample[i];
                }
                mean /= n;
                variance = variance / n - mean * mean;
                return new StudentDistribution(-2 * variance / (1 - variance));
            }
            else if (method == FittingMethod.MaximumLikelihood)
            { 
                double fitness (double _nu)
                {
                    StudentDistribution dist = new StudentDistribution(_nu);
                    double sum = 0.0;
                    for (int i = 0; i<n; i++)
                    {
                        sum += Math.Log(dist.ProbabilityDensity(sample[i])); 
                    }
                    return -sum;
                }

                bool feasibilityFunction(double x) => (x > 0);

                GaussianHillClimb1D opti = new GaussianHillClimb1D(1, fitness, feasibilityFunction, OptimizationType.Min, 5, 0.5, 100);
                opti.Optimize();
                
                return new StudentDistribution(opti.Result);
            }
            throw new NotImplementedException();
        }
        #region Methods

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name = "x" > The location at which to compute the density</param>
        /// <returns>a<c> double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return Fn.Gamma(0.5 * (_k + 1)) * Math.Pow(1 + x * x / _k, -0.5 * (_k + 1)) / (Math.Sqrt(Math.PI * _k) * Fn.Gamma(0.5 * _k));
        }

        /// <summary> A string representation of the distribution. </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"StudentT( ν = {_k})";
        }
        #endregion
    }
}

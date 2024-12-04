using Euclid.Benchmarking;
using Euclid.Histograms;
using Euclid.Optimizers;
using Euclid.Solvers;
using Euclid.Solvers.SingleVariableSolver;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Fisher distribution class
    /// </summary>
    public class FisherDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _d1, _d2;
        #endregion

        #region Constructors
        /// <summary>Initializes a new instance of the Fisher distribution</summary>
        /// <param name="d1">the first number of freedom degrees</param>
        /// <param name="d2">the second number of freedom degrees</param>
        public FisherDistribution(double d1, double d2)
        {
            if (d1 <= 0) throw new ArgumentException("The d1 can not be negative");
            _d1 = d1;

            if (d2 <= 0) throw new ArgumentException("The d2 can not be negative");
            _d2 = d2;

            _support = new Interval(0, double.PositiveInfinity, true, false);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's second degree of freedom</summary>
        public double FreedomDegree2 => _d2;

        /// <summary>Gets the distribution's first degree of freedom</summary>
        public double FreedomDegree1 => _d1;

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _d2 > 2 ? _d2 / (_d2 - 2) : double.NaN;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => InverseCumulativeDistribution(0.5);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _d1 > 2 ? (_d1 - 2) * _d2 / (_d1 * (_d2 + 2)) : double.NaN;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => _d2 > 6 ? (2 * _d1 + _d2 - 2) / (_d2 - 6) * Math.Sqrt(8 * (_d2 - 4) / (_d1 * (_d1 + _d2 - 2))) : double.NaN;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _d2 > 4 ? _d2 / (_d2 - 2) * Math.Sqrt(2 * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4))) : double.NaN;

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _d2 > 4 ? 2 * Math.Pow(_d2 / (_d2 - 2), 2) * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4)) : double.NaN;

        #endregion

        #region Methods

        /// <summary> Builds a sample of random variables under this distribution </summary>
        /// <param name="size">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public double[] SampleBis(int size, int seed)
        { 
        if (_d1-Math.Ceiling(_d1)==0 && _d2 - Math.Ceiling(_d2) == 0)
            {
                double[] result = new double[size];
                ChiSquaredDistribution D1 = new ChiSquaredDistribution((int)_d1);
                ChiSquaredDistribution D2 = new ChiSquaredDistribution((int)_d2);
                double[] sample1 = D1.Sample(size, seed);
                double[] sample2 = D2.Sample(size, seed);
                for (int i =0; i<size; i++)
                {
                    result[i] = (sample1[i] / _d1) / (sample2[i] / _d2);
                }
                return result;
            } else
            {
                Random random = new Random(seed);
                double[] result = new double[size];
                for (int i = 0; i < size; i++)
                    result[i] = InverseCumulativeDistribution(random.NextDouble());
                return result;
            }
        }
            
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static FisherDistribution Fit(double[] sample)
        {
            return Fit(FittingMethod.MaximumLikelihood, sample);
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static FisherDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.MaximumLikelihood)
            {
                double d1 = 0.1;
                double var = sample.Select(x => x * x).Average() - Math.Pow(sample.Average(),2);
                double d2 = (var > 1) ? 2*var/(var - 2) :1;
                double func(Vector _x)
                {
                    double _D1 = _x[0];
                    double _D2 = _x[1];
                    FisherDistribution dist = new FisherDistribution(_D1,_D2);
                    double l = -sample.Select(x => Math.Log(dist.ProbabilityDensity(x))).Sum();
                    return l;
                }

                bool feasibilityFunction(Vector _x)
                {
                    if (_x[0] > 0 && _x[1] > 0) { return true; }
                    return false;
                }

                Vector[] initialSimplex = new Vector[3];
                initialSimplex[0] = Vector.Create(d1 + 1.0, d2 +1.0);
                initialSimplex[1] = Vector.Create(d1, d2);
                initialSimplex[2] = Vector.Create(d1, d2 + 2.0);
                NelderMead nelderMead = new NelderMead(feasibilityFunction, func, initialSimplex, OptimizationType.Min, 100);
                nelderMead.Optimize();
                Vector result = nelderMead.Result;
                return new FisherDistribution(result[0], result[1]);
            }
            throw new NotImplementedException(); 
            
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            return Fn.IncompleteRegularizedBeta(_d1 * x / (_d1 * x + _d2), 0.5 * _d1, 0.5 * _d2);
        }

        /// <summary>Computes the inverse of the cumulative distribution function</summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            Bracketing solver = new Bracketing(0,Math.Pow(10,15), CumulativeDistribution, BracketingMethod.Dichotomy, 200); 
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return Math.Sqrt(Math.Pow(_d1 * x, _d1) * Math.Pow(_d2, _d2) / Math.Pow(_d1 * x + _d2, _d1 + _d2)) / (x * Fn.Beta(0.5 * _d1, 0.5 * _d2));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            throw new Exception("The MGF is not defined");
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Fisher(d1 = {0} d2 = {1})", _d1, _d2);
        }

        #endregion
    }
}

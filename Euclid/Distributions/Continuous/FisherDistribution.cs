using System;
using System.Linq;
using Euclid.Histograms;
using Euclid.Optimizers;
using Euclid.Solvers.SingleVariableSolver;

namespace Euclid.Distributions.Continuous
{
    /// <summary> Fisher distribution class </summary>
    public class FisherDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _d1, _d2;
        #endregion

        #region Constructors
        /// <summary>Builds a Fisher distribution</summary>
        /// <param name="d1">the first number of freedom degrees</param>
        /// <param name="d2">the second number of freedom degrees</param>
        public FisherDistribution(double d1, double d2)
        {
            if (d1 <= 0) throw new ArgumentException("d1 can not be negative");
            _d1 = d1;

            if (d2 <= 0) throw new ArgumentException("d2 can not be negative");
            _d2 = d2;

            _support = new Interval(0, double.PositiveInfinity, true, false);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _d2 > 2 ? _d2 / (_d2 - 2) : double.NaN;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => InverseCumulativeDistribution(0.5);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _d1 > 2 ? (_d1 - 2) * _d2 / (_d1 * (_d2 + 2)) : double.NaN;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _d2 > 4 ? _d2 / (_d2 - 2) * Math.Sqrt(2 * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4))) : double.NaN;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _d2 > 4 ? 2 * Math.Pow(_d2 / (_d2 - 2), 2) * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4)) : double.NaN;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => _d2 > 6 ? (2 * _d1 + _d2 - 2) / (_d2 - 6) * Math.Sqrt(8 * (_d2 - 4) / (_d1 * (_d1 + _d2 - 2))) : double.NaN;

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => throw new NotImplementedException();

        /// <summary>Gets the distribution's first degree of freedom</summary>
        public double FreedomDegrees1 => _d1;

        /// <summary>Gets the distribution's second degree of freedom</summary>
        public double FreedomDegrees2 => _d2;
        #endregion

        #region Methods
        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            return (x <= 0) ? 0 : Fn.IncompleteRegularizedBeta(_d1 * x / (_d1 * x + _d2), 0.5 * _d1, 0.5 * _d2);
        }

        /// <summary>Computes the inverse of the cumulative distribution function</summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
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
            return (x <= 0) ? 0 : Math.Sqrt(Math.Pow(_d1 * x, _d1) * Math.Pow(_d2, _d2) / Math.Pow(_d1 * x + _d2, _d1 + _d2)) / (x * Fn.Beta(0.5 * _d1, 0.5 * _d2));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t) => double.NaN;

        /// <summary> Builds a sample of random variables under this distribution </summary>
        /// <param name="size">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int size, int seed)
        {
            if (_d1 - Math.Ceiling(_d1) == 0 && _d2 - Math.Ceiling(_d2) == 0)
            {
                ChiSquaredDistribution chi1 = new ChiSquaredDistribution((int)_d1),
                    chi2 = new ChiSquaredDistribution((int)_d2);
                double[] result = new double[size],
                    sample1 = chi1.Sample(size, seed),
                    sample2 = chi2.Sample(size, seed);
                for (int i = 0; i < size; i++)
                    result[i] = (sample1[i] / _d1) / (sample2[i] / _d2);
                return result;
            }
            else
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
        public static FisherDistribution Fit(double[] sample) => Fit(FittingMethod.MaximumLikelihood, sample);


        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static FisherDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Length == 0)
                throw new ArgumentException("the sample can't be empty");
            if (sample.Any(d => d < 0))
                throw new ArgumentOutOfRangeException(nameof(sample), "the sample can't be lower or equal to 0");
            if (method == FittingMethod.MaximumLikelihood)
            {
                int n = sample.Length;
                double variance = 0, mean = 0;

                for (int i = 0; i < n; i++)
                {
                    mean += sample[i];
                    variance += sample[i] * sample[i];
                }
                mean /= n;
                variance = variance / n - mean * mean;

                double d1 = 0.1,
                    d2 = (variance > 2) ? 2 * variance / (variance - 2) : 1;


                double fitness(Vector v)
                {
                    FisherDistribution dist = new FisherDistribution(v[0], v[1]);
                    double sum = 0;
                    for (int i = 0; i < n; i++)
                        sum -= Math.Log(dist.ProbabilityDensity(sample[i]));
                    return sum;
                }

                bool feasibilityFunction(Vector v) => v[0] > 0 && v[1] > 0;

                Vector[] initialSimplex = {
                    Vector.Create(d1 + 1.0, d2 + 1.0),
                    Vector.Create(d1, d2),
                    Vector.Create(d1, d2 + 2.0)};
                NelderMead nelderMead = new NelderMead(feasibilityFunction, fitness, initialSimplex, OptimizationType.Min, 100);
                nelderMead.Optimize();

                return new FisherDistribution(nelderMead.Result[0], nelderMead.Result[1]);
            }
            throw new NotImplementedException();

        }


        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format($"Fisher(d1 = {_d1} d2 = {_d2})");
        }
        #endregion
    }
}

using System;
using System.Linq;
using Euclid.Histograms;
using Euclid.Numerics;
using Euclid.Optimizers;
using Euclid.Solvers.SingleVariableSolver;



namespace Euclid.Distributions.Continuous
{
    /// <summary>Weibull distribution class</summary>
    public class WeibullDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _lambda, _k, _mu, _sigma2, _sigma;
        #endregion

        #region Constructor
        /// <summary>Builds a Weibull distribution</summary>
        /// <param name="lambda">the scale</param>
        /// <param name="k">the shape</param>
        public WeibullDistribution(double lambda, double k)
        {
            if (lambda <= 0) throw new ArgumentException("The scale can not be negative");
            if (k <= 0) throw new ArgumentException("The shape can not be negative");
            _lambda = lambda;
            _k = k;

            _support = new Interval(0, double.PositiveInfinity, true, false);

            _mu = _lambda * Fn.Gamma(1 + 1 / _k);
            _sigma2 = _lambda * _lambda * Fn.Gamma(1 + 2 / _k) - _mu * _mu;
            _sigma = Math.Sqrt(_sigma2);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _mu;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _lambda * Math.Pow(Math.Log(2), 1 / _k);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get
            {
                if (_k < 1) return double.NaN;
                else if (_k == 1) return 0;
                return _lambda * Math.Pow((_k - 1) / _k, 1 / _k);
            }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _sigma;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _sigma2;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => Fn.Gamma(1 + 3 / _k) * Math.Pow(_lambda / _sigma, 3) - 3 * _mu / _sigma - Math.Pow(_mu / _sigma, 3);

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Fn.EulerGamma * (1 - 1 / _k) + Math.Log(_lambda / _k) + 1;

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's shape parameter</summary>
        public double Shape => _k;

        /// <summary>Gets the distribution's scale parameter</summary>
        public double Scale => _lambda;
        #endregion

        #region Methods
        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            return (x < 0) ? 0 : 1 - Math.Exp(-Math.Pow(x / _lambda, _k));
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _lambda * Math.Pow(-Math.Log(1 - p), 1 / _k);
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return (x <= 0) ? 0 : 
                (_k / _lambda) * Math.Pow(x / _lambda, _k - 1) * Math.Exp(-Math.Pow(x / _lambda, _k));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            if (_k < 1) { throw new ArgumentOutOfRangeException(nameof(_k), "the shape parameter has to be bigger than 1"); }
            double res = 0,
                incr;
            int n = 0;             
            do
            {
                incr = Math.Pow(t * _lambda, n) * Fn.Gamma(1 + n / _k) / Fn.Factorial(n);
                res += incr;
                n++;
            } while (incr > 1e-14);
            return res;
        }

        /// <summary>Builds a sample of random variables under this distribution</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {
            Random random = new Random(seed);
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
                result[i] = _lambda * Math.Pow(-Math.Log(1 - random.NextDouble()), 1 / _k);
            return result;
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static WeibullDistribution Fit(double[] sample) => Fit(FittingMethod.PositionalArgument, sample);

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static WeibullDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample == null || sample.Length < 3)
                throw new ArgumentException("the sample can't be empty");
            if (sample.Any(d => d <= 0))
                throw new ArgumentOutOfRangeException(nameof(sample), "the sample can't be lower or equal to 0");
            int n = sample.Length;
            
            if (method == FittingMethod.PositionalArgument)
            {
                double[] x = new double[n], y = new double[n], f = new double[n];

                double xAvg = 0, yAvg = 0;

                sample = sample.OrderBy(d => d).ToArray();
                for (int i = 0; i < n; i++)
                {
                    f[i] = (i + 0.5) / (n + 1);
                    y[i] = Math.Log(-Math.Log(1 - f[i]));
                    x[i] = Math.Log(sample[i]);
                    xAvg += x[i];
                    yAvg += y[i];
                }
                xAvg /= n;
                yAvg /= n;

                double numerator = 0, denominator = 0;
                for (int i = 0; i < n; i++)
                {
                    numerator += (x[i] - xAvg) * (y[i] - yAvg);
                    denominator += (x[i] - xAvg) * (x[i] - xAvg);
                }

                double k = numerator / denominator, lambda = Math.Exp((-yAvg + k * xAvg) / k);

                return new WeibullDistribution(lambda, k);
            }
            else if (method == FittingMethod.MaximumLikelihood)
            {
                double shape = 0.1,
                    scale = 1 / sample.Average();

                double fitness(Vector v)
                {
                    WeibullDistribution dist = new WeibullDistribution(v[0], v[1]);
                    double sum = 0;
                    for (int i = 0; i < n; i++)
                        sum -= Math.Log(dist.ProbabilityDensity(sample[i]));
                    return sum;
                }

                bool feasibilityFunction(Vector v) => v[0] > 0 && v[1] > 0;
                double epsi = 0.1;

                Vector[] initialSimplex = new Vector[]{
                    Vector.Create(scale , shape),
                    Vector.Create(scale * (1 + epsi) , shape ),
                    Vector.Create(scale, shape *(epsi + 1))};

                NelderMead nelderMead = new NelderMead(feasibilityFunction, fitness, initialSimplex, OptimizationType.Min, 1000);
                nelderMead.Optimize();

                return new WeibullDistribution(nelderMead.Result[0], nelderMead.Result[1]);
            }
            else if (method == FittingMethod.Moments)
            {        
                double m = sample.Average();
                double variance = sample.Sum(x => Math.Pow(x - m, 2)) / n;
                double cv2 = variance / (m * m);

                Func<double, double> f = k =>
                {
                    double gamma1 = Fn.Gamma(1 + 1 / k);
                    double gamma2 = Fn.Gamma(1 + 2 / k);
                    return gamma2 / (gamma1 * gamma1) - 1 - cv2;
                };
                Func<double, double> df = f.Differentiate(DifferenceForm.Central, 1e-6);

                var solver = new NewtonRaphson(1.0, f, df, 1000)
                {
                    AbsoluteTolerance = 1e-10,
                    SlopeTolerance = 1e-10,
                    TrackConvergence = false
                };

                solver.Solve();     

                double kEstimate = solver.Result;
                double lambda = m / Fn.Gamma(1 + 1 / kEstimate);
    
                return new WeibullDistribution(lambda, kEstimate);
            }
            throw new NotImplementedException();
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format($"Weibull(λ = {_lambda} k={_k})");
        }
        #endregion
    }
}

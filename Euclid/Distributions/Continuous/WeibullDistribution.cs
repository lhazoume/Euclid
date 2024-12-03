using Euclid.Analytics.ErrorFunctions;
using Euclid.Histograms;
using Euclid.Optimizers;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>
    /// Weibull distribution class
    /// </summary>
    public class WeibullDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _lambda, _k, _mu, _sigma2, _sigma;
        #endregion

        #region Constructor
        /// <summary>Builds a Weibull distribution</summary>
        /// <param name="lambda">the scale</param>
        /// <param name="k">the shape</param>
        public WeibullDistribution(double lambda, double k )
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
        /// <summary>Gets the distribution's shape parameter</summary>
        public double Shape => _k;

        /// <summary>Gets the distribution's scale parameter</summary>
        public double Scale => _lambda;

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Fn.EulerGamma * (1 - 1 / _k) + Math.Log(_lambda / _k) + 1;

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

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => Fn.Gamma(1 + 3 / _k) * Math.Pow(_lambda / _sigma, 3) - 3 * _mu / _sigma - Math.Pow(_mu / _sigma, 3);

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _sigma;

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _sigma2;
        #endregion

        #region Methods
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static WeibullDistribution Fit(double[] sample)
        {
            return Fit(FittingMethod.LeastSquare, sample);
        }

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static WeibullDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.LeastSquare) {
                int n = sample.Length;
                double[] xi = new double[n];
                double[] yi = new double[n];
                double[] Fi = new double[n];

                sample = sample.OrderBy(x => x).ToArray();
                for (int i = 0; i < n; i++)
                {
                    Fi[i] = (i + 0.5) / (n + 1);
                    yi[i] = Math.Log(-Math.Log(1 - Fi[i]));
                    xi[i] = Math.Log(sample[i]);
                }

                double Xavg = xi.Average();
                double Yavg = yi.Average();

                double numerator = 0;
                double denominator = 0;
                for (int i = 0; i < n; i++)
                {
                    numerator += (xi[i] - Xavg) * (yi[i] - Yavg);
                    denominator += (xi[i] - Xavg) * (xi[i] - Xavg);
                }

                double k = numerator / denominator;
                double lambda = Math.Exp((-Yavg + k * Xavg) / k);

                return new WeibullDistribution(lambda, k);
            } else if (method == FittingMethod.Numeric)
            {
                double shape = 1;
                double scale = sample.Average();

                double func(Vector _x)
                {
                    double _scale = _x[0];
                    double _shape = _x[1];
                    WeibullDistribution dist = new WeibullDistribution(_scale, _shape);
                    double l = -sample.Select(x => Math.Log(dist.ProbabilityDensity(x))).Sum();
                    return l;
                }

                bool feasibilityFunction(Vector _x)
                {
                    if (_x[0] > 0 && _x[1] > 0) { return true; }
                    return false;
                }

                Vector[] initialSimplex = new Vector[3];
                initialSimplex[0] = Vector.Create(scale + 1, shape);
                initialSimplex[1] = Vector.Create(scale + 1, shape + 1);
                initialSimplex[2] = Vector.Create(scale, shape + 1);
                NelderMead nelderMead = new NelderMead(feasibilityFunction, func, initialSimplex, OptimizationType.Min, 100);
                nelderMead.Optimize();
                Vector result = nelderMead.Result;

                return new WeibullDistribution(result[0], result[1]);
            }
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            return 1 - Math.Exp(-Math.Pow(x / _lambda, _k));
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
            if (x < 0) return 0;
            return (_k / _lambda) * Math.Pow(x / _lambda, _k - 1) * Math.Exp(-Math.Pow(x / _lambda, _k));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            if (_k<1) { throw new ArgumentOutOfRangeException(nameof(_k)); }
            double res = 0;
            int n = 0;
            double incr;
            do
            {
                incr = Math.Pow(t*_lambda, n)*Fn.Gamma(1+n/_k)/Fn.Factorial(n);
                res += incr;
                n++;
            } while (incr > Math.Pow(10,-14));
            return res;
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

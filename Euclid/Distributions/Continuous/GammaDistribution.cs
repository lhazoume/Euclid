using Euclid.Histograms;
using Euclid.Solvers;
using Euclid.Solvers.SingleVariableSolver;
using Microsoft.Win32.SafeHandles;
using System;
using System.CodeDom;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Gamma distribution class</summary>
    public class GammaDistribution : ContinuousDistribution
    {
        #region Variables
        private readonly double _k, _theta, _cdfFactor, _pdfFactor;
        #endregion

        #region Constructors
        /// <summary>Builds a Gamma distribution</summary>
        /// <param name="k">the shape</param>
        /// <param name="theta">the scale</param>
        public GammaDistribution(double k, double theta)
        {
            if (k <= 0) throw new ArgumentException("the shape has to be positive");
            if (theta <= 0) throw new ArgumentException("the scale has to be positive");
            _k = k;
            _theta = theta;

            _support = new Interval(0, double.PositiveInfinity, false, false);

            _cdfFactor = 1 / Fn.Gamma(_k);
            _pdfFactor = Math.Pow(_theta, -_k) * _cdfFactor;
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => _k + Math.Log(_theta) + Math.Log(Fn.Gamma(_k)) + (1 - _k) * Fn.DiGamma(_k);

        /// <summary>Gets the distribution's support </summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's mean </summary>
        public override double Mean => _k * _theta;

        /// <summary>Gets the distribution's median </summary>
        public override double Median => InverseCumulativeDistribution(0.5);

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _k >= 1 ? (_k - 1) * _theta : double.NaN;

        /// <summary>Gets the distribution's skewness </summary>
        public override double Skewness => 2 / Math.Sqrt(_k);

        /// <summary>Gets the distribution's standard deviation </summary>
        public override double StandardDeviation => _theta * Math.Sqrt(_k);

        /// <summary>Gets the distribution's variance </summary>
        public override double Variance => _k * _theta * _theta;
        #endregion

        #region Methods
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static GammaDistribution Fit(double[] sample)
        {
            return Fit(FittingMethod.MaximumLikelihood, sample);
        }
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static GammaDistribution Fit(FittingMethod method, double[] sample)
        {
            int n = sample.Length;
            if (method == FittingMethod.Moments)
            {
                double avg = sample.Average();
                double sigma2 = sample.Select(x => x * x).Average() - avg * avg;
                double theta = sigma2 / avg;
                double k = avg * avg / sigma2;
                return new GammaDistribution(k, theta);
            }
            else if (method == FittingMethod.MaximumLikelihood)
            {
                double sumX = sample.Sum();
                double sumLogX = sample.Select(x => Math.Log(x)).Sum();
                double sumXLogX = sample.Select(x => x * Math.Log(x)).Sum();

                double k = (n * sumX) / (n * sumXLogX - sumLogX * sumX);
                double theta = (n * sumXLogX - sumLogX * sumX) / ((n-1)*n);
                k = k - 1 / n * (3 * k - 2 / 3 * (k / (1 + k)) - 4 * k / (5 * Math.Pow(1 + k,2)));
                return new GammaDistribution(k, theta);
            }
            else { throw new NotImplementedException(); }
            
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x <= 0) return 0;
            return _cdfFactor * Fn.IncompleteLowerGamma(_k, x / _theta);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(1, CumulativeDistribution, 100);
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (x <= 0) return 0;
            return _pdfFactor * Math.Pow(x, _k - 1) * Math.Exp(-x / _theta);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            if (_theta * t < 1)
                return Math.Pow(1 - _theta * t, -_k);
            throw new ArgumentOutOfRangeException(nameof(t), "The argument of the MGF should be lower than the rate");
        }

        /// <summary>Generates a sequence of samples using the Ahrens-Dieter algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {
            Random random = new Random(seed);
            double[] result = new double[numberOfPoints];
            int i = 0;
            if (_k < 1)
            {
                double w = _k / Math.Exp(1) / (1 - _k);
                double l = 1 / _k - 1;
                double r = 1 / (1 + w);
                double z, nz, hz;
                do { 
                    double u1 = random.NextDouble();

                    if (u1<=r) {
                        z = -Math.Log(random.NextDouble());
                    } else
                    {
                        z = Math.Log(random.NextDouble())/l;
                    }

                    double u2 = random.NextDouble();

                    nz = (z>=0) ? Math.Exp(-z) : w*l*Math.Exp(l*z);
                    hz = Math.Exp(-z-Math.Exp(-z/_k));
                    if (hz/nz >u2)
                    {
                        result[i] = _theta * Math.Exp(-z / _k);
                        i++;
                    }
                
                } while (i < numberOfPoints);
            }
            else
            {
                // Marsaglia-Tsang
                double d = _k - 1.0 / 3.0;
                double c = 1.0 / Math.Sqrt(9.0 * d);
                do
                {
                    double v, z;
                    do
                    { // Générer une variable normale standard Z
                        double u1 = random.NextDouble();
                        double u2 = random.NextDouble();
                        z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                        v = Math.Pow(1.0 + c * z, 3);
                    } while (v <= 0);
                
                    double u = random.NextDouble();
                    // Test d'acceptation
                    if (Math.Log(u) < 0.5 * z * z + d * (1 - v + Math.Log(v)))
                    {
                        result[i] = d * v * _theta;
                        i++;
                    }
                } while (i < numberOfPoints);
                
            }
            return result;
        }

        private static double GenerateAhrensDieterRejection(Random random, double delta)
        {
            double e, n;
            do
            {
                double u = 1 - random.NextDouble(),
                    v = 1 - random.NextDouble(),
                    w = 1 - random.NextDouble();

                if (u * (Math.E + delta) <= Math.E)
                {
                    e = Math.Pow(v, 1 / delta);
                    n = w * Math.Pow(e, delta - 1);
                }
                else
                {
                    e = 1 - Math.Log(v);
                    n = w * Math.Exp(-e);
                }

            } while (n <= Math.Pow(e, delta - 1) * Math.Exp(-e));

            return e;
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Γ(k = {0} θ = {1})", _k, _theta);
        }
        #endregion
    }
}

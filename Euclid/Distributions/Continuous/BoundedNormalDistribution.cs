using Euclid.Histograms;
using Euclid.Optimizers;
using System;
using System.Linq;

namespace Euclid.Distributions.Continuous
{
    /// <summary> Bounded normal distribution class </summary>
    public class BoundedNormalDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _mu,
            _sigma, _sigma2,
            _a, _b,
            _alpha, _beta,
            _phiAlpha, _phiBeta,
            _gbAlpha, _gbBeta,
            _dGb,
            _Z;
        #endregion

        #region Constructors
        /// <summary>Builds a truncated normal distribution</summary>
        /// <param name="mu">the location</param>
        /// <param name="sigma">the scale</param>
        /// <param name="a">the interval's lower bound</param>
        /// <param name="b">the interval's upper bound</param>
        public BoundedNormalDistribution(double mu, double sigma, double a, double b)
        {
            if (sigma <= 0) throw new ArgumentException("sigma has to be positive");
            if (a >= b) throw new ArgumentException("the interval is not defined");
            _sigma = sigma;
            _sigma2 = _sigma * _sigma;
            _mu = mu;

            _a = a;
            _b = b;

            _alpha = double.IsNegativeInfinity(_a) ? double.NegativeInfinity : (_a - _mu) / _sigma;
            _beta = double.IsPositiveInfinity(_b) ? double.PositiveInfinity : (_b - _mu) / _sigma;

            _phiAlpha = double.IsNegativeInfinity(_a) ? 0 : Fn.Phi(_alpha);
            _phiBeta = double.IsPositiveInfinity(_b) ? 1 : Fn.Phi(_beta);

            _gbAlpha = double.IsNegativeInfinity(_a) ? 0 : Fn.GaussBell(_alpha);
            _gbBeta = double.IsPositiveInfinity(_b) ? 0 : Fn.GaussBell(_beta);

            _dGb = (double.IsNegativeInfinity(_a) ? 0 : (_alpha * _gbAlpha)) - (double.IsPositiveInfinity(_b) ? 0 : (_beta * _gbBeta));

            _Z = _phiBeta - _phiAlpha;
            _support = new Interval(_a, _b, true, true);
        }
        #endregion

        #region Accessors

        /// <summary>Gets the mean parameter of the distribution</summary>
        public double MeanParameter => _mu;

        /// <summary>Gets the standard deviation parameter of the distribution</summary>
        public double StandardDeviationParameter => _sigma;

        /// <summary>Gets the distribution's upper bound</summary>
        public double UpperBound => _b;

        /// <summary>Gets the distribution's lower bound</summary>
        public double LowerBound => _a;

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Math.Log(Math.Sqrt(2 * Math.PI * Math.E) * _sigma * _Z) + _dGb / (2 * _Z);

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _mu + (_gbAlpha - _gbBeta) * _sigma / _Z;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _mu + _sigma * Fn.InvPhi(0.5 * (_phiBeta + _phiAlpha));

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get
            {
                if (!double.IsNegativeInfinity(_a) && _mu < _a) return _a;
                if (!double.IsPositiveInfinity(_b) && _mu > _b) return _b;
                return _mu;
            }
        }

        /// <summary>
        /// Gets the distribution's skewness
        /// </summary>
        /// <remarks>using Shah and Jaiswal (1966)</remarks>
        public override double Skewness
        {
            get
            {
                double k0 = double.IsNegativeInfinity(_a) ? 0 : _alpha,
                    k1 = double.IsPositiveInfinity(_b) ? 0 : _beta,
                    z0 = _gbAlpha / _Z,
                    z1 = _gbBeta / _Z,
                    dz = z1 - z0, dkz = k1 * z1 - k0 * z0,
                    V = 1 - dkz - Math.Pow(dz, 2),
                    s = -Math.Pow(V, -1.5) * (2 * Math.Pow(dz, 3) + (3 * dkz - 1) * dz + k1 * k1 * z1 - k0 * k0 * z0);
                return s;
            }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => Math.Sqrt(Variance);

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _sigma2 * (1 + _dGb / _Z - Math.Pow((_gbAlpha - _gbBeta) / _Z, 2));
        #endregion

        #region Methods
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static BoundedNormalDistribution Fit(double[] sample) => Fit(FittingMethod.MaximumLikelihood, sample); 

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static BoundedNormalDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.MaximumLikelihood)
            {
                int n = sample.Length;
                double mean = 0.0,
                    sigma = 0.0,
                    a=double.PositiveInfinity,
                    b=double.NegativeInfinity;
                for (int i = 0; i < n; i++)
                {
                    mean += sample[i];
                    sigma += sample[i] * sample[i];
                    a = (sample[i]>a) ? a : sample[i];
                    b = (sample[i]<b) ? b : sample[i];
                }
                mean /= n;
                sigma = Math.Sqrt(sigma / n - mean * mean);

                double fitness(Vector v)
                {
                    BoundedNormalDistribution dist = new BoundedNormalDistribution(v[0], v[1], a, b);
                    double sum = 0.0;
                    for (int i = 0; i < n; i++)
                    {
                        sum += Math.Log(dist.ProbabilityDensity(sample[i]));
                    }
                    return -sum;
                }

                bool feasibilityFunction(Vector v) => (v[1] > 0);

                Vector[] initialSimplex = { 
                    Vector.Create(mean - 5, sigma), 
                    Vector.Create(mean + 5, sigma + 5), 
                    Vector.Create(mean + 5, sigma) };
                NelderMead nelderMead = new NelderMead(feasibilityFunction, fitness, initialSimplex, OptimizationType.Min, 100);
                nelderMead.Optimize();
                return new BoundedNormalDistribution(nelderMead.Result[0], nelderMead.Result[1], a, b);
            }
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution function at x</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < _support.LowerBound.Value) return 0;
            else if (x > _support.UpperBound.Value) return 1;
            return (Fn.Phi((x - _mu) / _sigma) - _phiAlpha) / _Z;
        }

        /// <summary>Computes the inverse of the cumulative distribution function</summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _mu + _sigma * Fn.InvPhi(_phiAlpha + p * _Z);
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            if (!_support.Contains(x)) return 0;
            return Fn.GaussBell((x - _mu) / _sigma) / (_sigma * _Z);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return Math.Exp(_mu * t + _sigma2 * t * t / 2) * (Fn.Phi(_beta - _sigma * t) - Fn.Phi(_alpha - _sigma * t)) / (Fn.Phi(_beta) - Fn.Phi(_alpha));
        }

        /// <summary>Generates a sequence of samples using the Ahrens-Dieter algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {

            double[] result = new double[numberOfPoints];
            if (_phiBeta - _phiAlpha > 0.5)
            {
                int i = 0, cpt = 0;
                NormalDistribution N = new NormalDistribution(_mu, _sigma);
                double[] random = N.Sample(2 * numberOfPoints, seed);
                do
                {
                    if (_a < random[cpt] && _b > random[cpt])
                    {
                        result[i] = random[cpt];
                        i++;
                    }
                    cpt++;
                    if (cpt == 2 * numberOfPoints)
                    {
                        random = N.Sample(2 * numberOfPoints, seed);
                        cpt = 0;
                    }
                } while (i < numberOfPoints);
                
            } else {
                Random random = new Random(seed);
                for (int i = 0; i < numberOfPoints; i++)
                    result[i] = InverseCumulativeDistribution(random.NextDouble());
            }

            return result;
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return $"BoundedN(μ = {_mu}, σ = {_sigma}, a = {_a}, b = {_b})";
        }
        #endregion
    }
}

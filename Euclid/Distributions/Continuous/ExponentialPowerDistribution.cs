using Euclid.Analytics.Clustering;
using Euclid.Benchmarking;
using Euclid.Histograms;
using Euclid.Optimizers;
using Euclid.Solvers;
using Euclid.Solvers.SingleVariableSolver;
using System;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Exponential power distribution class</summary>
    public class ExponentialPowerDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _mu, _alpha,
            _beta, _1Beta, _gamma1Beta;
        #endregion

        #region Constructors
        private ExponentialPowerDistribution(double mu, double alpha, double beta, Random randomSource)
        {
            _mu = mu;

            if (alpha <= 0) throw new ArgumentException("scale has to be positive");
            _alpha = alpha;

            if (beta <= 0) throw new ArgumentException("shape has to be positive");
            _beta = beta;
            _1Beta = 1 / _beta;
            _gamma1Beta = Fn.Gamma(_1Beta);
            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        /// <summary> Builds a Exponential power distribution</summary>
        /// <param name="mu">the location</param>
        /// <param name="alpha">the scale</param>
        /// <param name="beta">the shape</param>
        public ExponentialPowerDistribution(double mu, double alpha, double beta)
            : this(mu, alpha, beta, new Random(Guid.NewGuid().GetHashCode()))
        { }
        #endregion

        #region Accessors

        /// <summary>Gets the distribution's scale parameter</summary>
        public double Scale => _alpha;

        /// <summary>Gets the distribution's shape parameter</summary>
        public double Shape => _beta;

        /// <summary>Gets the distribution's location parameter</summary>
        public double Location => _mu;

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => _1Beta - Math.Log(_beta / (2 * _alpha * _gamma1Beta));

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => _mu;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _mu;

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _mu;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => 0.0;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => _alpha * Math.Sqrt(Fn.Gamma(3 / _beta) / _gamma1Beta);

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => _alpha * _alpha * Fn.Gamma(3 / _beta) / _gamma1Beta;

        #endregion

        #region Methods
        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        public static ExponentialPowerDistribution Fit(double[] sample) => Fit(FittingMethod.MaximumLikelihood, sample);

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static ExponentialPowerDistribution Fit(FittingMethod method, double[] sample)
        {
            if (method == FittingMethod.MaximumLikelihood)
            {
                #region NelderMead
                double mu = sample.Average();
                double alpha = sample.Select(x => x * x).Average() - mu * mu;
                double beta = 1;

                double func(Vector _x)
                {
                    double _alpha = _x[0];
                    double _beta = _x[1];
                    ExponentialPowerDistribution dist = new ExponentialPowerDistribution(mu, _alpha, _beta);
                    double l =-sample.Select(x => Math.Log(dist.ProbabilityDensity(x))).Sum();
                    return l;
                }

                bool feasibilityFunction(Vector _x)
                {
                    if (_x[0] > 0 && _x[1] > 0) { return true; }
                    return false;
                }

                Vector[] initialSimplex = new Vector[3];
                initialSimplex[0] = Vector.Create(alpha, beta);
                initialSimplex[1] = Vector.Create(alpha, beta + 10);
                initialSimplex[2] = Vector.Create(alpha + 10, beta);
                NelderMead nelderMead = new NelderMead(feasibilityFunction, func, initialSimplex, OptimizationType.Min, 100);
                nelderMead.Optimize();
                Vector result = nelderMead.Result;
                return new ExponentialPowerDistribution(mu, result[0], result[1]);
                #endregion
            }
            throw new NotImplementedException();
        }

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            return 0.5 + Math.Sign(x - _mu) * Fn.IncompleteLowerGamma(_1Beta, Math.Pow(Math.Abs(x - _mu) / _alpha, _beta)) / (2 * _gamma1Beta);
        }

        /// <summary>Computes the inverse of the cumulative distribution function</summary>
        /// <param name="p">the target probablity</param>
        /// <returns>a double</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(_mu, CumulativeDistribution, 10);
            solver.Solve(p);
            return solver.Result;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return _beta * Math.Exp(-Math.Pow(Math.Abs(x - _mu) / _alpha, _beta)) / (2 * _alpha * _gamma1Beta);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            throw new NotImplementedException("The MGF is not implemented");
        }

        /// <summary>Generates a sequence of samples from the normal distribution using the algorithm</summary>
        /// <param name="numberOfPoints">the sample's size</param>
        /// <param name="seed">the random number generator's seed</param>
        /// <see cref="https://cran.r-project.org/web/packages/gnorm/vignettes/gnormUse.html"/>
        /// <returns>an array of double</returns>
        public override double[] Sample(int numberOfPoints, int seed)
        {
            double[] samples = new double[numberOfPoints];
            GammaDistribution G = new GammaDistribution(1 + 1 / _beta, Math.Pow(2, _beta / 2));
            double[] Y = G.Sample(numberOfPoints);
            double delta;
            for (int i = 0; i < numberOfPoints; i++)
            {
                delta = _alpha * Math.Pow(Y[i], 1 / _beta) / Math.Sqrt(2);
                UniformDistribution uniformDistribution = new UniformDistribution(_mu - delta, _mu + delta);
                samples[i] = uniformDistribution.Sample(1)[0];
            }
            return samples;
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("ExponentialPower(μ = {0}, α = {1}, β = {2})", _mu, _alpha, _beta);
        }

        #endregion
    }
}


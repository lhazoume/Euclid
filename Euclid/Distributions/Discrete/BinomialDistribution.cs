using Euclid.Arithmetics;
using System;
using System.Linq;

namespace Euclid.Distributions.Discrete
{
    /// <summary> Binonmial distribution class</summary>
    public class BinomialDistribution : DiscreteDistribution
    {

        #region Declarations
        private readonly double _q, _p;
        private readonly int _n;
        private readonly BinomialCoefficients _bc;
        #endregion

        #region Constructors
        private BinomialDistribution(int n, double p, Random randomSource)
        {
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            if (p > 1 || p < 0) throw new ArgumentOutOfRangeException("The probability should be in [0,1]");
            if (n <= 0) throw new ArgumentOutOfRangeException("The number of trials should be >0");
            _p = p;
            _q = 1 - p;
            _n = n;
            _bc = new BinomialCoefficients(_n);
        }

        /// <summary>Initializes a new instance of the binomial distribution</summary>
        /// <param name="n">the number of runs</param>
        /// <param name="p">the unitary probability</param>
        public BinomialDistribution(int n, double p)
            : this(n, p, new Random(Guid.NewGuid().GetHashCode()))
        { }

        #endregion

        #region Accessors
        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy
        {
            get { return 0.5 * Math.Log(2 * Math.PI * Math.E * _n * _p * _q); }
        }

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean
        {
            get { return _n * _p; }
        }

        /// <summary>Gets the distribution's median</summary>
        public override double Median
        {
            get
            {
                double floor = Math.Floor(_n * _p),
                    ceiling = Math.Ceiling(_n * _p);
                return 0.5 * (floor + ceiling);
            }
        }

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode
        {
            get { return 0.5 * (Math.Floor((_n + 1) * _p) + Math.Ceiling((_n + 1) * _p) - 1); }
        }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness
        {
            get { return (1 - 2 * _p) / Math.Sqrt(_n * _p * _q); }
        }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation
        {
            get { return Math.Sqrt(_n * _p * _q); }
        }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance
        {
            get { return _n * _p * _q; }
        }

        /// <summary>Gets the distribution's support</summary>
        public override double[] Support
        {
            get { return Enumerable.Range(0, _n + 1).Select(d => Convert.ToDouble(d)).ToArray(); }
        }
        #endregion

        #region Methods

        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x).</summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            if (x > _n + 1) return 1;
            int k = Convert.ToInt32(Math.Floor(x));
            return Fn.IncompleteRegularizedBeta(_q, _n - k, k + 1);
        }

        /// <summary>Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function</summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            if (p <= 0) return 0;
            if (p >= 1) return _n;
            int k = 0;

            while (CumulativeDistribution(k) < p)
                k++;
            return k - 1;
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            int k = Convert.ToInt32(Math.Round(x));
            if (k != x) return 0;
            if (k < 0 || k > _n) return 0;

            return _bc[k] * Math.Pow(_p / _q, k) * Math.Pow(_q, _n);
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t)
        {
            return Math.Pow(1 - _p + _p * Math.Exp(t), _n);
        }

        /// <summary>Generates a sequence of samples from the distribution</summary>
        /// <param name="size">the sample's size</param>
        /// <returns>an array of double</returns>
        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < _n; j++)
                    result[i] += _randomSource.NextDouble() <= _p ? 1 : 0;
            return result;
        }

        /// <summary>Fits the distribution to a sample of data</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static BinomialDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Min() < 0 || sample.Any(x => x != Convert.ToInt32(x)))
                throw new ArgumentOutOfRangeException("sample");

            if (method == FittingMethod.Moments)
            {
                double m = sample.Average(),
                    v = sample.Average(x => x * x) - m * m,
                    p = 1 - v / m;
                if (v >= m)
                    throw new ArgumentOutOfRangeException("sample", "The moments do not match a Binomial distribution");
                
                int n = Convert.ToInt32(m * m / (m - v));

                return new BinomialDistribution(n, p);
            }

            throw new NotImplementedException();
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format("Binomial(n = {0} p = {1})", _n, _p);
        }
        #endregion
    }
}

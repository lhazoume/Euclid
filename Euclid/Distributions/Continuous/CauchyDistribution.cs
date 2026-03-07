using System;
using System.Linq;
using Euclid.Histograms;

namespace Euclid.Distributions.Continuous
{
    /// <summary> Cauchy distribution class </summary>
    public class CauchyDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _x0, _gamma;
        #endregion

        #region Constructors
        /// <summary>Builds a Cauchy distribution</summary>
        /// <param name="x0">the location</param>
        /// <param name="gamma">the scale</param>
        public CauchyDistribution(double x0, double gamma)
        {
            _x0 = x0;

            if (gamma <= 0) throw new ArgumentException("gamma has to be positive");
            _gamma = gamma;

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the distribution's mean</summary>
        public override double Mean => double.NaN;

        /// <summary>Gets the distribution's median</summary>
        public override double Median => _x0;

        /// <summary>Gets the distribution's mode</summary>
        public override double Mode => _x0;

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation => double.NaN;

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance => double.NaN;

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness => double.NaN;

        /// <summary>Gets the distribution's entropy</summary>
        public override double Entropy => Math.Log(_gamma * 4 * Math.PI);

        /// <summary>Gets the distribution's support</summary>
        public override Interval Support => _support;

        /// <summary>Gets the distribution's scale</summary>
        public double Scale => _gamma;

        /// <summary>Gets the distribution's location parameter</summary>
        public double Location => _x0;
        #endregion

        #region Methods
        /// <summary>Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)</summary>
        /// <param name="x">the location at which to compute the function</param>
        /// <returns>a double</returns>
        public override double CumulativeDistribution(double x)
        {
            return 0.5 + Math.Atan((x - _x0) / _gamma) / Math.PI;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return _x0 + _gamma * Math.Tan(Math.PI * (p - 0.5));
        }

        /// <summary>Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x</summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>a <c>double</c></returns>
        public override double ProbabilityDensity(double x)
        {
            return 1 / (Math.PI * _gamma * (1 + Math.Pow((x - _x0) / _gamma, 2)));
        }

        /// <summary>Evaluates the moment-generating function for a given t</summary>
        /// <param name="t">the argument</param>
        /// <returns>a double</returns>
        public override double MomentGeneratingFunction(double t) => double.NaN;

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>

        public override double[] Sample(int size, int seed)
        {
            Random random = new Random(seed);
            double[] sample = new double[size];

            for (int i = 0; i < size; i++)
            {
                double u = random.NextDouble();
                sample[i] = InverseCumulativeDistribution(u);
            }

            return sample;
        }

        ///<inheritdoc/>
        public static CauchyDistribution Fit(double[] sample) => Fit(FittingMethod.PositionalArgument, sample);

        /// <summary>Creates a new instance of the distribution fitted on the data sample</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="method">the fitting method</param>
        public static CauchyDistribution Fit(FittingMethod method, double[] sample)
        {
            if (sample.Length == 0)
                throw new ArgumentException("the sample can't be empty");
            if (method == FittingMethod.PositionalArgument)
            {
                double[] sortedData = sample.OrderBy(x => x).ToArray();
                int n = sortedData.Length;

                double median = n % 2 == 0 ? (sortedData[n / 2 - 1] + sortedData[n / 2]) / 2.0 : sortedData[n / 2];

                double[] firstHalf = sortedData.Take(n / 2).ToArray(),
                    secondHalf = sortedData.Skip((n + 1) / 2).ToArray();
                int nFirst = firstHalf.Length,
                    nSecond = secondHalf.Length;
                double q1 = nFirst % 2 == 0 ? (firstHalf[nFirst / 2 - 1] + firstHalf[nFirst / 2]) / 2 : firstHalf[nFirst / 2],
                    q3 = nSecond % 2 == 0 ? (secondHalf[nSecond / 2 - 1] + secondHalf[nSecond / 2]) / 2 : secondHalf[nSecond / 2],
                    interquartileRange = q3 - q1;

                return new CauchyDistribution(median, interquartileRange / 2);
            }
            throw new NotImplementedException();
        }

        /// <summary>Returns a string that represents this instance</summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return string.Format($"Cauchy(x0 = {_x0} γ = {_gamma})");
        }
        #endregion
    }
}

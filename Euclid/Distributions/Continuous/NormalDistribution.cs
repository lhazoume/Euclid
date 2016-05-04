using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Distributions.Continuous
{
    public class NormalDistribution : ContinousDistribution
    {
        #region Declarations
        private double _mean, _standardDeviation;
        private Random _randomSource;
        #endregion

        #region Constructors
        private NormalDistribution(double mean, double standardDeviation, Random randomSource)
        {
            _mean = mean;
            if (standardDeviation < 0) throw new ArgumentException("The standard deviation can not be negative");
            _standardDeviation = standardDeviation;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;
        }
        public NormalDistribution()
            : this(0, 1)
        { }
        public NormalDistribution(double mean, double standardDeviation)
            : this(mean, standardDeviation, new Random(DateTime.Now.Millisecond))
        { }
        #endregion

        #region Public static methods

        /// <summary>
        /// Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x).
        /// </summary>
        /// <param name="mean">The mean(μ) of the normal distribution</param>
        /// <param name="standardDeviation">The standard deviation (σ) of the normal distribution.Range: σ ≥ 0</param>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public static double CumulativeDistributionFunction(double mean, double standardDeviation, double x)
        {
            // This algorithm is ported from dcdflib:
            // Cody, W.D. (1993). "ALGORITHM 715: SPECFUN - A Portabel FORTRAN
            // Package of Special Function Routines and Test Drivers"
            // acm Transactions on Mathematical Software. 19, 22-32.
            int i;
            double del, xden, xnum, xsq;
            double result, ccum;
            double arg = (x - mean) / standardDeviation;
            const double sixten = 1.60e0;
            const double sqrpi = 3.9894228040143267794e-1;
            const double thrsh = 0.66291e0;
            const double root32 = 5.656854248e0;
            const double zero = 0.0e0;
            const double min = Double.Epsilon;
            double z = arg;
            double y = Math.Abs(z);
            const double half = 0.5e0;
            const double one = 1.0e0;

            double[] a = { 2.2352520354606839287e00, 1.6102823106855587881e02, 1.0676894854603709582e03, 1.8154981253343561249e04, 6.5682337918207449113e-2 };

            double[] b = { 4.7202581904688241870e01, 9.7609855173777669322e02, 1.0260932208618978205e04, 4.5507789335026729956e04 };

            double[] c =
            {
                3.9894151208813466764e-1, 8.8831497943883759412e00, 9.3506656132177855979e01,
                5.9727027639480026226e02, 2.4945375852903726711e03, 6.8481904505362823326e03,
                1.1602651437647350124e04, 9.8427148383839780218e03, 1.0765576773720192317e-8
            };

            double[] d =
            {
                2.2266688044328115691e01, 2.3538790178262499861e02, 1.5193775994075548050e03,
                6.4855582982667607550e03, 1.8615571640885098091e04, 3.4900952721145977266e04,
                3.8912003286093271411e04, 1.9685429676859990727e04
            };
            double[] p =
            {
                2.1589853405795699e-1, 1.274011611602473639e-1, 2.2235277870649807e-2,
                1.421619193227893466e-3, 2.9112874951168792e-5, 2.307344176494017303e-2
            };


            double[] q = { 1.28426009614491121e00, 4.68238212480865118e-1, 6.59881378689285515e-2, 3.78239633202758244e-3, 7.29751555083966205e-5 };
            if (y <= thrsh)
            {
                //
                // Evaluate  anorm  for  |X| <= 0.66291
                //
                xsq = zero;
                if (y > double.Epsilon) xsq = z * z;
                xnum = a[4] * xsq;
                xden = xsq;
                for (i = 0; i < 3; i++)
                {
                    xnum = (xnum + a[i]) * xsq;
                    xden = (xden + b[i]) * xsq;
                }
                result = z * (xnum + a[3]) / (xden + b[3]);
                double temp = result;
                result = half + temp;
            }

            //
            // Evaluate  anorm  for 0.66291 <= |X| <= sqrt(32)
            //
            else if (y <= root32)
            {
                xnum = c[8] * y;
                xden = y;
                for (i = 0; i < 7; i++)
                {
                    xnum = (xnum + c[i]) * y;
                    xden = (xden + d[i]) * y;
                }
                result = (xnum + c[7]) / (xden + d[7]);
                xsq = Math.Floor(y * sixten) / sixten;
                del = (y - xsq) * (y + xsq);
                result = Math.Exp(-(xsq * xsq * half)) * Math.Exp(-(del * half)) * result;
                ccum = one - result;
                if (z > zero)
                {
                    result = ccum;
                }
            }

            //
            // Evaluate  anorm  for |X| > sqrt(32)
            //
            else
            {
                xsq = one / (z * z);
                xnum = p[5] * xsq;
                xden = xsq;
                for (i = 0; i < 4; i++)
                {
                    xnum = (xnum + p[i]) * xsq;
                    xden = (xden + q[i]) * xsq;
                }
                result = xsq * (xnum + p[4]) / (xden + q[4]);
                result = (sqrpi - result) / y;
                xsq = Math.Floor(z * sixten) / sixten;
                del = (z - xsq) * (z + xsq);
                result = Math.Exp(-(xsq * xsq * half)) * Math.Exp(-(del * half)) * result;
                ccum = one - result;
                if (z > zero)
                {
                    result = ccum;
                }
            }

            if (result < min)
                result = 0.0e0;
            return result;

        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution</param>
        /// <param name="standardDeviation">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0</param>
        /// <returns>true if the parameter set is valid, false, otherwise</returns>
        private static bool IsValidParameterSet(double mean, double standardDeviation)
        {
            return standardDeviation >= 0;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution</param>
        /// <param name="standardDeviation">The standard deviation (σ) of the normal distribution.Range: σ ≥ 0</param>
        /// <param name="probability">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public static double InverseCumulativeDistributionFunction(double mean, double standardDeviation, double probability)
        {
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException("p", probability, "The probability must be comprised in [0, 1].");
            if (probability == 0) return double.MinValue;
            if (probability == 1) return double.MaxValue;

            #region Const values
            double[] a = { -3.969683028665376e+01, 2.209460984245205e+02, -2.759285104469687e+02, 1.383577518672690e+02, -3.066479806614716e+01, 2.506628277459239e+00 },
                b = { -5.447609879822406e+01, 1.615858368580409e+02, -1.556989798598866e+02, 6.680131188771972e+01, -1.328068155288572e+01 },
                c = { -7.784894002430293e-03, -3.223964580411365e-01, -2.400758277161838e+00, -2.549732539343734e+00, 4.374664141464968e+00, 2.938163982698783e+00 },
                d = { 7.784695709041462e-03, 3.224671290700398e-01, 2.445134137142996e+00, 3.754408661907416e+00 };
            #endregion

            double pLow = 0.02425,
                pHigh = 1 - pLow,
                p = Convert.ToDouble(probability),
                result;

            double q;

            if (p < pLow)
            {
                // Rational approximation for lower region:
                q = Math.Sqrt(-2 * Math.Log(p));
                result = (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                    ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            }
            else if (pHigh < p)
            {
                // Rational approximation for upper region:
                q = Math.Sqrt(-2 * Math.Log(1 - p));
                result = -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                    ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            }
            else
            {
                // Rational approximation for central region:
                q = p - 0.5;
                double r = q * q;
                result = (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * q /
                    (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1);
            }

            return mean + result * standardDeviation;
        }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution</param>
        /// <param name="standardDeviation">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0</param>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>the density at x</returns>
        public static double ProbabilityDensityFunction(double mean, double standardDeviation, double x)
        {
            double nx = (x - mean) / standardDeviation;
            if (nx == double.MinValue) return -1;
            if (nx == double.MaxValue) return 1;
            return Math.Exp(Convert.ToDouble(-0.5 * nx * nx)) / Math.Sqrt(2.0 * Math.PI);
        }

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x)
        /// </summary>
        /// <param name="mean">The mean (μ) of the normal distribution</param>
        /// <param name="standardDeviation">The standard deviation (σ) of the normal distribution. Range: σ ≥ 0</param>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns>the log density at x</returns>
        public static double ProbabilityDensityFunctionLn(double mean, double standardDeviation, double x)
        {
            return Math.Log(ProbabilityDensityFunction(mean, standardDeviation, x));
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the algorithm
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <param name="mean">The mean(μ) of the normal distribution</param>
        /// <param name="standardDeviation">The standard deviation (σ) of the normal distribution.Range: σ ≥ 0</param>
        /// <param name="randomSource">The random number generator to use</param>
        /// <returns>a sequence of samples from the distribution</returns>
        public static double[] Samples(int numberOfPoints, double mean, double standardDeviation, Random randomSource)
        {
            double[] result = new double[numberOfPoints];
            for (int i = 0; i < numberOfPoints / 2; i++)
            {
                double u = 1.0 - randomSource.NextDouble(),
                    v = randomSource.NextDouble(),
                    x = Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v),
                    y = Math.Sqrt(-2 * Math.Log(u)) * Math.Sin(2 * Math.PI * v);
                result[2 * i] = mean + standardDeviation * x;
                result[2 * i + 1] = mean + standardDeviation * y;
            }

            if (numberOfPoints % 2 == 1)
            {
                double u = 1.0 - randomSource.NextDouble(),
                    v = randomSource.NextDouble(),
                    x = Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
                result[numberOfPoints - 1] = mean + standardDeviation * x;
            }

            return result;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Computes the cumulative distribution(CDF) of the distribution at x, i.e.P(X ≤ x)
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function</param>
        /// <returns>the cumulative distribution at location x</returns>
        public override double CumulativeDistribution(double x)
        {
            return NormalDistribution.CumulativeDistributionFunction(_mean, _standardDeviation, x);
        }

        /// <summary>
        /// Computes the probability density of the distribution(PDF) at x, i.e. ∂P(X ≤ x)/∂x
        /// </summary>
        /// <param name="x">The location at which to compute the density</param>
        /// <returns></returns>
        public override double ProbabilityDensity(double x)
        {
            return NormalDistribution.ProbabilityDensityFunction(_mean, _standardDeviation, x);
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function(InvCDF) for the distribution at the given probability.This is also known as the quantile or percent point function
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density</param>
        /// <returns>the inverse cumulative density at p</returns>
        public override double InverseCumulativeDistribution(double p)
        {
            return InverseCumulativeDistributionFunction(_mean, _standardDeviation, p);
        }

        /// <summary>
        /// Generates a sequence of samples from the normal distribution using the algorithm
        /// </summary>
        /// <param name="numberOfPoints">the size of the sample to generate</param>
        /// <returns>a sequence of samples from the distribution</returns>
        public override double[] Sample(int numberOfPoints)
        {
            return Samples(numberOfPoints, _mean, _standardDeviation, _randomSource);
        }


        //string ToString()
        //A string representation of the distribution.
        //Return
        //string
        //a string representation of the distribution.
        #endregion

        #region Accessors

        /// <summary>
        /// Gets the entropy of the normal distribution
        /// </summary>
        public override double Entropy
        {
            get { return Math.Log(_standardDeviation * Math.Sqrt(2 * Math.PI * Math.E)); }
        }

        /// <summary>
        /// Gets the maximum of the normal distribution
        /// </summary>
        public override double Maximum
        {
            get { return double.MaxValue; }
        }

        /// <summary>
        /// Gets or sets the mean(μ) of the normal distribution
        /// </summary>
        public override double Mean
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the median of the normal distribution
        /// </summary>
        public override double Median
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets the minimum of the normal distribution
        /// </summary>
        public override double Minimum
        {
            get { return double.MinValue; }
        }

        /// <summary>
        /// Gets the mode of the normal distribution
        /// </summary>
        public override double Mode
        {
            get { return _mean; }
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples
        /// </summary>
        public Random RandomSource
        {
            get { return _randomSource; }
            set
            {
                if (value == null) throw new ArgumentException("The random source can not be null");
                _randomSource = value;
            }
        }

        /// <summary>
        /// Gets the skewness of the normal distribution
        /// </summary>
        public override double Skewness
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets or sets the standard deviation(σ) of the normal distribution.Range: σ ≥ 0
        /// </summary>
        public override double StandardDeviation
        {
            get { return _standardDeviation; }
        }

        /// <summary>
        /// Gets or sets the variance of the normal distribution
        /// </summary>
        public override double Variance
        {
            get { return _standardDeviation * _standardDeviation; }
        }
        #endregion
    }
}

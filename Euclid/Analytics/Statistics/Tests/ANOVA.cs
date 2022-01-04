using System;
using System.Linq;

namespace Euclid.Analytics.Statistics.Tests
{
    /// <summary>
    /// Class which descibes Analysis of variance: ANOVA
    /// </summary>
    public sealed class ANOVA
    {
        #region vars
        /// <summary>
        /// Two-dimensional arrays of double
        /// </summary>
        public double[][] Data { get; }
        // ssb, msb, ssw, msw
        /// <summary>
        /// F-Statistics
        /// </summary>
        public double F { get; private set; }
        /// <summary>
        /// Sum of squares between groups
        /// </summary>
        public double Ssb { get; private set; }
        /// <summary>
        /// Mean square between groups
        /// </summary>
        public double Msb { get; private set; }
        /// <summary>
        /// Sum of squares within groups
        /// </summary>
        public double Ssw { get; private set; }
        /// <summary>
        /// Mean square within groups
        /// </summary>
        public double Msw { get; private set; }
        /// <summary>
        /// Degree of fredom
        /// </summary>
        public int[] DF { get; private set; }
        /// <summary>
        /// Probability that all population means are equal 
        /// </summary>
        public double Pvalue { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for ANOVA's object
        /// </summary>
        /// <param name="data">Sample</param>
        public ANOVA(double[][] data) { Data = data; }
        #endregion

        #region methods
        /// <summary>
        /// Run ANOVA computation for one factor
        /// </summary>
        /// <returns>True computation done with sucess else an error has been raised</returns>
        public bool RunOneWay()
        {
            try
            {
                #region initialization
                DF = new int[2];
                Ssb = Msb = Ssw = Msw = 0;
                #endregion

                #region computation
                FstatOneWay();
                Pvalue = PF(DF.First(), DF.Last(), F);
                #endregion

                return true;
            }
            catch (Exception) { return false; }
            
        }

        #region Compute F-Statistics
        /// <summary>
        /// Compute the F-statisics for One way ANONVA method
        /// </summary>
        /// <returns>F-Statistics</returns>
        private void FstatOneWay()
        {
            int K = Data.Length; // Number groups
            int[] n = new int[K]; // Number items each group
            int N = 0; // total number data points

            for (int i = 0; i < K; ++i)
            {
                n[i] = Data[i].Length;
                N += Data[i].Length;
            }

            double[] means = new double[K];
            double mean = 0.0;
            for (int i = 0; i < K; ++i)
            {
                for (int j = 0; j < Data[i].Length; ++j)
                {
                    means[i] += Data[i][j];
                    mean += Data[i][j];
                }
                means[i] /= n[i];
            }
            mean /= N;

            Ssb = 0.0;
            for (int i = 0; i < K; ++i)
                Ssb += n[i] * (means[i] - mean) * (means[i] - mean);
            double Msb = Ssb / (K - 1);

            Ssw = 0.0;
            for (int i = 0; i < K; ++i)
                for (int j = 0; j < Data[i].Length; ++j)
                    Ssw += (Data[i][j] - means[i]) * (Data[i][j] - means[i]);
            Msw = Ssw / (N - K);

            DF = new int[] { K - 1, N - K };
            F = Msb / Msw;
        }
        #endregion

        #region approximation log-gamma by Lanczos algorithm (code provided by James MC Caffrey: https://jamesmccaffrey.wordpress.com/2016/02/12/the-area-under-the-f-distribution-using-c/)
        /// <summary>
        /// Log gamma approximation by using Lanczos algorithm
        /// </summary>
        /// <param name="x">value</param>
        /// <returns>approximation</returns>
        static double LogGammaByLanczosAlgorithm(double x)
        {
            // Log of Gamma from Lanczos with g=5, n=6/7
            double[] coef = new double[6] { 76.18009172947146,
        -86.50532032941677, 24.01409824083091,
        -1.231739572450155, 0.1208650973866179E-2,
        -0.5395239384953E-5 };
            double LogSqrtTwoPi = 0.91893853320467274178;
            double denom = x + 1;
            double y = x + 5.5;
            double series = 1.000000000190015;
            for (int i = 0; i < 6; ++i)
            {
                series += coef[i] / denom;
                denom += 1.0;
            }
            return (LogSqrtTwoPi + (x + 0.5) * Math.Log(y) -
            y + Math.Log(series / x));
        }

        /// <summary>
        /// Imcomplete beta approximation by using Lanczos algorithm
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns>Imcomplete beta approximation</returns>
        private static double BetaInc(double a, double b, double x)
        {
            // Incomplete Beta function
            // A & S 6.6.2 and 26.5.8
            double bt;
            if (x == 0.0 || x == 1.0)
                bt = 0.0;
            else
                bt = Math.Exp(LogGammaByLanczosAlgorithm(a + b) - LogGammaByLanczosAlgorithm(a) -
                  LogGammaByLanczosAlgorithm(b) + a * Math.Log(x) + b *
                  Math.Log(1.0 - x));

            if (x < (a + 1.0) / (a + b + 2.0))
                return bt * BetaIncCf(a, b, x) / a;
            else
                return 1.0 - bt * BetaIncCf(b, a, 1.0 - x) / b;
        }

        /// <summary> Approximate Incomplete Beta computed by continued fraction (Handbook of Mathematical Functions: with Formulas, Graphs, and Mathematical Tables, A &amp; S 26.5.8)</summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns>Approximate Incomplete Beta</returns>
        private static double BetaIncCf(double a, double b, double x)
        {
            int max_it = 100;
            double epsilon = 3.0e-7;
            double small = 1.0e-30;

            int m2; // 2*m
            double aa, del;

            double qab = a + b;
            double qap = a + 1.0;
            double qam = a - 1.0;
            double c = 1.0;
            double d = 1.0 - qab * x / qap;
            if (Math.Abs(d) < small) d = small;
            d = 1.0 / d;
            double result = d;

            int m;
            for (m = 1; m <= max_it; ++m)
            {
                m2 = 2 * m;
                aa = m * (b - m) * x / ((qam + m2) *
                  (a + m2));
                d = 1.0 + aa * d;
                if (Math.Abs(d) < small) d = small;
                c = 1.0 + aa / c;
                if (Math.Abs(c) < small) c = small;
                d = 1.0 / d;
                result *= d * c;
                aa = -(a + m) * (qab + m) * x / ((a + m2) *
                  (qap + m2));
                d = 1.0 + aa * d;
                if (Math.Abs(d) < small) d = small;
                c = 1.0 + aa / c;
                if (Math.Abs(c) < small) c = small;
                d = 1.0 / d;
                del = d * c;
                result *= del;
                if (Math.Abs(del - 1.0) < epsilon) break;
            }
            if (m > max_it) throw new Exception("BetaIncCf() failure ");
            return result;
        }

        /// <summary>
        /// Approximate lower tail of F-dist (area from 0.0 to x) equivalent to the R pf() function only accurate to about 3 decimals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns>Probability</returns>
        private static double PF(double a, double b, double x)
        {
            double z = (a * x) / (a * x + b);
            return BetaInc(a / 2, b / 2, z);
        }
        #endregion

        #endregion
    }
}

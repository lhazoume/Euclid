using Euclid.Solvers;
using Euclid.Solvers.SingleVariableSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Tests
{
    /// <summary> Helps testing empiric distribution adequacy using Kolmogorov-Smirnov</summary>
    public static class KolmogorovSmirnovTest
    {

        private static double PValue(double[] series1, double[] series2)
        {
            int i = 0, j = 0,
                n1 = series1.Length, n2 = series2.Length;
            double maxSpread = 0;
            while (i < n1 && j < n2)
            {
                double fi = i * 1.0 / n1, fj = j * 1.0 / n2;

                maxSpread = Math.Max(maxSpread, Math.Abs(fi - fj));
                if (i == series1.Length) j++;
                if (j == series2.Length) i++;

                if (series1[i] < series2[j]) { i++; }
                else if (series1[i] == series2[j]) { i++; j++; }
                else { j++; }
            }

            return Math.Sqrt((n1 * 1.0 * n2) / (n1 + n2)) * maxSpread;
        }

        /// <summary>
        /// Performs the Kolmogorov Smirnov test.
        /// </summary>
        /// <param name="series1">The first sample.</param>
        /// <param name="series2">The second sample.</param>
        /// <param name="alpha">The confidence level of the test</param>
        /// <returns>True if the series appear to be drawn from the same distribution, false otherwise.</returns>
        public static bool IsSameDistribution(double[] series1, double[] series2, double alpha)
        {
            double pValue = PValue(series1, series2);
            Bracketing rb = new Bracketing(1, 3, Fn.SupBrownianBridgeCDF, BracketingMethod.Dichotomy, 100);
            rb.Solve(alpha);
            double cap = rb.Result;

            return  pValue <cap;
        }
    }
}

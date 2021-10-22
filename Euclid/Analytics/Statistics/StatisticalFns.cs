using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Statistics
{
    /// <summary>
    /// Collection of statistical functions
    /// </summary>
    public static class StatisticalFns
    {
        #region percentile
        /// <summary>
        /// Compute percentile(s) over a collection of data
        /// </summary>
        /// <param name="values">Collection of data</param>
        /// <param name="percentiles">Percentiles</param>
        /// <returns>Percentiles values</returns>
        public static double[] Percentile(this IEnumerable<double> values, IList<double> percentiles)
        {
            double[] percentileValues = new double[percentiles.Count];
            double[] elts = values.ToArray(); Array.Sort(elts); int nbElts = elts.Length;
            if (nbElts < 1) return percentileValues;

            for (int i = 0; i < percentiles.Count; i++)
            {
                double rIndex = percentiles[i] * (nbElts - 1);
                int index = (int)rIndex;
                double frac = rIndex - index;
                if (index + 1 < nbElts)
                { percentileValues[i] = elts[index] * (1 - frac) + elts[index + 1] * frac; continue; }
                percentileValues[i] = elts[index];
            }

            return percentileValues;
        }
        #endregion

        #region summary statistics
        /// <summary>
        /// Compute the descriptive statistics from the serie 
        /// </summary>
        /// <param name="values">Serie of data</param>
        /// <returns>A statistical summary of the numeric data</returns>
        public static SummaryStatistics Describe(this IEnumerable<double> values)
        {
            if (values == null) return new SummaryStatistics();
            #region compute percentile(s)
            List<double> percentiles = new List<double>(3) { 0.1, 0.5, 0.9 };
            double[] percentilesValues = values.Percentile(percentiles);
            #endregion

            #region compute statistics
            double min = double.MaxValue, max = double.MinValue, sum = 0, sumSquared = 0;
            int n = 0;
            foreach (double x in values)
            {
                n++;
                if (min > x) min = x;
                if (max < x) max = x;
                sum += x;
                sumSquared += x * x;
            }

            if (n == 0) return new SummaryStatistics();

            double inverse = 1.0 / n;
            #endregion
            double average = sum * inverse;

            double sumOfCubes = 0, sumOfFours = 0;
            foreach (double x in values)
            {
                double deviation = x - average, cubicDeviations = Math.Pow(deviation, 3);
                sumOfCubes += cubicDeviations;
                sumOfFours += deviation * cubicDeviations;
            }

            double std = Math.Sqrt(sumSquared * inverse - (average * average)),
                skew = std == 0 ? 0 : sumOfCubes / (n * Math.Pow(std, 1.5)), kurt = std == 0 ? 0 : sumOfFours / (n * Math.Sqrt(std));

            return new SummaryStatistics(min, sum * inverse, max, std, max - min, percentilesValues[0], percentilesValues[1], percentilesValues[2], n, skew, kurt, sum);
        }
        #endregion
    }
}

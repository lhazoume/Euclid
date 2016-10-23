using Euclid.IndexedSeries;
using System;
using System.Linq;

namespace Euclid.Analytics
{
    public static class InterRelations<T, U, V>
    {
        public static DataFrame<V, double, V> CovarianceEstimatorMatrix<T, V>(DataFrame<T, double, V> dataFrame) where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IComparable<V>, IConvertible
        {
            DataFrame<V, double, V> result = DataFrame<V, double, V>.Create(dataFrame.Columns, dataFrame.Columns);
            result.SetLabels(dataFrame.Labels);
            result.SetLegends(dataFrame.Labels);
            int n = dataFrame.Rows;

            #region Averages
            double[] averages = new double[dataFrame.Columns];
            for (int i = 0; i < dataFrame.Columns; i++)
            {
                averages[i] = 0;
                for (int k = 0; k < n; k++)
                    averages[i] += dataFrame[k, i];
                averages[i] /= n;
            }
            #endregion

            #region Vars and covars
            for (int i = 0; i < dataFrame.Columns; i++)
                for (int j = i; j < dataFrame.Columns; j++)
                {
                    double cov = 0;
                    for (int k = 0; k < n; k++)
                        cov += dataFrame[k, i] * dataFrame[k, j];
                    cov = (cov - n * averages[i] * averages[j]) / (n - 1);

                    result[i, j] = cov;
                    if (j > i)
                        result[j, i] = cov;
                }
            #endregion

            return result;
        }
        public static DataFrame<V, double, V> CorrelationMatrix<T, V>(DataFrame<T, double, V> dataFrame) where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IComparable<V>, IConvertible
        {
            DataFrame<V, double, V> result = DataFrame<V, double, V>.Create(dataFrame.Columns, dataFrame.Columns);
            result.SetLabels(dataFrame.Labels);
            result.SetLegends(dataFrame.Labels);
            int n = dataFrame.Rows;

            #region Averages
            double[] averages = new double[dataFrame.Columns],
                stdevs = new double[dataFrame.Columns];
            for (int i = 0; i < dataFrame.Columns; i++)
            {
                averages[i] = 0;
                stdevs[i] = 0;
                for (int k = 0; k < n; k++)
                {
                    averages[i] += dataFrame[k, i];
                    stdevs[i] += Math.Pow(dataFrame[k, i], 2);
                }
                averages[i] /= n;
                stdevs[i] = Math.Sqrt(stdevs[i] / n - averages[i] * averages[i]);
            }
            #endregion

            #region Vars and covars
            for (int i = 0; i < dataFrame.Columns; i++)
            {
                result[i, i] = 1;
                for (int j = i + 1; j < dataFrame.Columns; j++)
                {
                    double cov = 0;
                    for (int k = 0; k < n; k++) cov += dataFrame[k, i] * dataFrame[k, j];
                    cov = stdevs[i] * stdevs[j] == 0 ? 0 : (cov / n - averages[i] * averages[j]) / (stdevs[i] * stdevs[j]);

                    result[i, j] = cov;
                    result[j, i] = cov;
                }
            }
            #endregion

            return result;
        }
        public static DataFrame<V, double, V> RedundancyMatrix<T, V>(DataFrame<T, double, V> dataFrame) where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IComparable<V>, IConvertible
        {
            DataFrame<V, double, V> result = DataFrame<V, double, V>.Create(dataFrame.Columns, dataFrame.Columns);
            result.SetLabels(dataFrame.Labels);
            result.SetLegends(dataFrame.Labels);
            int n = dataFrame.Rows;

            #region Distinct values
            double[][] distincts = new double[dataFrame.Columns][];
            for (int i = 0; i < dataFrame.Columns; i++)
                distincts[i] = dataFrame.GetSeriesAt(i).Data.Distinct().OrderBy(d=>d).ToArray();
            #endregion

            #region Redundancy
            for (int i = 0; i < dataFrame.Columns; i++)
            {
                result[i, i] = 1;
                for (int j = i + 1; j < dataFrame.Columns; j++)
                {
                    int[,] occurences = new int[distincts[i].Length, distincts[j].Length];
                    for (int k = 0; k < n; k++)
                        occurences[Array.IndexOf<double>(distincts[i], dataFrame[k, i]), Array.IndexOf<double>(distincts[j], dataFrame[k, j])]++;
                    double redundancy = Entropy.Redundancy(occurences);
                    result[i, j] = redundancy;
                    result[j, i] = redundancy;
                }
            }
            #endregion

            return result;
        }
    }
}

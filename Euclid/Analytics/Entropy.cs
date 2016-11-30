﻿using System;
using System.Linq;

namespace Euclid.Analytics
{
    /// <summary>
    /// Helper class for entropy calculations
    /// </summary>
    public static class Entropy
    {
        /// <summary>
        /// Entropy is a measure of unpredictability of information content. Here is used Shannon's entropy expressed in nat (base e)
        /// </summary>
        /// <param name="occurences">the occurences in each bucket</param>
        /// <returns>the entropy in natural bits of information</returns>
        public static double EntropyMetric(int[] occurences)
        {
            int rows = occurences.Length,
                totalSum = occurences.Sum();

            #region Entropy
            double entropy = 0;
            for (int i = 0; i < rows; i++)
                if (occurences[i] != 0)
                    entropy -= occurences[i] * Math.Log(occurences[i] * 1.0 / totalSum);
            #endregion

            return entropy / totalSum;
        }

        /// <summary>
        /// Redundancy of information in the pair of variables
        /// </summary>
        /// <param name="occurences">the occurences in each bucket</param>
        /// <returns> 0 when the variables are independent and 1 when they are totally redundant</returns>
        public static double Redundancy(int[,] occurences)
        {
            int rows = occurences.GetLength(0),
                cols = occurences.GetLength(1);
            int[] rowOccurences = new int[rows],
                columnOccurences = new int[cols];

            #region Read the data
            int totalSum = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    int o = occurences[i, j];
                    totalSum += o;
                    rowOccurences[i] += o;
                    columnOccurences[j] += o;
                }
            #endregion


            double min = Math.Min(EntropyMetric(rowOccurences), EntropyMetric(columnOccurences));
            return min == 0 ? 0 : MutualInformation(occurences) / min;
        }

        /// <summary>
        /// The uncertainty coefficient measures the validity of a classification
        /// </summary>
        /// <param name="occurences">the occurences in each cross bucket</param>
        /// <param name="normalizerOccurences">the normalizing variable occurence</param>
        /// <returns>the fraction of additional information</returns>
        public static double Uncertainty(int[,] occurences, int[] normalizerOccurences)
        {
            double mutualInformation = MutualInformation(occurences),
                entropy = EntropyMetric(normalizerOccurences);

            return mutualInformation / entropy;
        }

        /// <summary>
        /// Mutual information measures the amount of information that can be obtained about one random variable by observing another
        /// </summary>
        /// <param name="occurences">the occurences in each bucket</param>
        /// <returns>the mutual information in natural bits of information</returns>
        public static double MutualInformation(int[,] occurences)
        {
            int rows = occurences.GetLength(0),
                cols = occurences.GetLength(1);
            int[] rowSums = new int[rows],
                columnSums = new int[cols];

            #region Read the data
            int totalSum = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    int o = occurences[i, j];
                    totalSum += o;
                    rowSums[i] += o;
                    columnSums[j] += o;
                }
            #endregion

            #region Result
            double result = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (occurences[i, j] != 0)
                        result += occurences[i, j] * Math.Log(occurences[i, j] * totalSum * 1.0 / (rowSums[i] * columnSums[j]));
            result = result / totalSum;
            #endregion

            return result;
        }
    }
}
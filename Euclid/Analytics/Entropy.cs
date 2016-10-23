using System;

namespace Euclid.Analytics
{
    public static class Entropy
    {
        public static double Redundancy(int[,] occurences)
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
            double information = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (occurences[i, j] != 0)
                        information += occurences[i, j] * Math.Log(occurences[i, j] * totalSum * 1.0 / (rowSums[i] * columnSums[j]));

            #region Columns Entropy
            double rowsEntropy = 0;
            for (int i = 0; i < rows; i++)
                if (rowSums[i] != 0)
                    rowsEntropy -= rowSums[i] * Math.Log(rowSums[i] * 1.0 / totalSum);
            #endregion

            #region Columns Entropy
            double columnsEntropy = 0;
            for (int j = 0; j < cols; j++)
                if (columnSums[j] != 0)
                    columnsEntropy -= columnSums[j] * Math.Log(columnSums[j] * 1.0 / totalSum);
            #endregion

            #endregion

            double min = Math.Min(rowsEntropy, columnsEntropy);
            return min == 0 ? 0 : information / min;
        }
        public static double Uncertainty(int[,] occurences)
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
            double information = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (occurences[i, j] != 0)
                        information += occurences[i, j] * Math.Log(occurences[i, j] * totalSum * 1.0 / (rowSums[i] * columnSums[j]));

            #region Columns Entropy
            double columnsEntropy = 0;
            for (int j = 0; j < cols; j++)
                if (columnSums[j] != 0)
                    columnsEntropy += columnSums[j] * Math.Log(columnSums[j] * 1.0 / totalSum);
            #endregion

            #endregion

            return -information / columnsEntropy;
        }
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

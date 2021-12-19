namespace Euclid.Analytics.Statistics
{
    /// <summary>
    /// Class which encapsulate a glossary of statistics
    /// </summary>
    public sealed class SummaryStatistics
    {
        #region vars
        /// <summary>Returns the minimum</summary>
        public double Min { get; private set; }

        /// <summary>Returns the average</summary>
        public double Average { get; private set; }

        /// <summary>Returns the maximum</summary>
        public double Max { get; private set; }
        /// <summary>Standard deviation (Pearson)</summary>
        public double Stdev { get; private set; }

        /// <summary>Distance between Min && Max</summary>
        public double Range { get; private set; }

        /// <summary>Median</summary>
        public double Median { get; private set; }

        /// <summary>Percentile 10%</summary>
        public double Percentile10 { get; private set; }

        /// <summary>Percentile 90%</summary>
        public double Percentile90 { get; private set; }

        /// <summary>Measure of asymmetry</summary>
        public double Skewness { get; private set; }

        /// <summary>Measure of the tailedness</summary>
        public double Kurtosis { get; private set; }

        /// <summary>Sum of element(s)</summary>
        public double Sum { get; private set; }

        /// <summary>Number of element(s)</summary>
        public int Count { get; private set; }
        #endregion

        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public SummaryStatistics() { }

        /// <summary>Builds a <c>SummaryStatistics</c></summary>
        /// <param name="min">Minimum</param>
        /// <param name="average">Average</param>
        /// <param name="max">Maximum</param>
        /// <param name="stdev">Standard deviation</param>
        /// <param name="range">Range</param>
        /// <param name="percentile10">Percentile 10%</param>
        /// <param name="median">Median</param>
        /// <param name="percentile90">Percentile 90%</param>
        /// <param name="count">Nb of elements</param>
        /// <param name="skewness">Skewness</param>
        /// <param name="kurtosis">Kurtosis</param>
        /// <param name="sum">Sum of elements</param>
        public SummaryStatistics(double min, double average, double max, double stdev, double range, double percentile10, double median, double percentile90, int count, double skewness, double kurtosis, double sum)
        {
            Min = min; Average = average; Max = max; Stdev = stdev; Range = range; Count = count; Percentile10 = percentile10; Percentile90 = percentile90; Median = median; Skewness = skewness; Kurtosis = kurtosis; Sum = sum;
        }
        #endregion

        #region methods
        /// <summary>
        /// Print main statistics from the summary
        /// </summary>
        /// <returns></returns>
        public override string ToString() { return string.Format("{0};{1};{2};{3};{4};{5}", Min, Average, Median, Max, Stdev, Count); }
        #endregion
    }
}

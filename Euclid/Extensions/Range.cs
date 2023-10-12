using System;
using System.Collections.Generic;

namespace Euclid.Extensions
{
    /// <summary>
    /// Handle different range types
    /// </summary>
    public static class Range
    {
        #region methods
        /// <summary>
        /// Create a range of data between bound according a step
        /// </summary>
        /// <param name="start">Starting value</param>
        /// <param name="end">Ending value</param>
        /// <param name="step">Step increment</param>
        /// <returns>Range of data</returns>
        public static IReadOnlyList<double> Create(double start, double end, double step) 
        {
            if (start >= end) throw new Exception($"start {start} >= end {end}");

            List<double> result = new List<double>();
            for (double val = start; val <= end; val = val + step) result.Add(val);
            return result;
        }
        #endregion
    }
}

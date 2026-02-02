using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Helpers
{
    /// <summary>Static helper for the quantilisation of time series</summary>
    public static class Quantilizer
    {
        /// <summary>Builds the empirical quantiles from a given set of datas</summary>
        /// <param name="buckets">the number of buckets (ie for quartiles, should be 4 though there are only 3 quartiles)</param>
        /// <param name="values">the set of data</param>
        /// <param name="open">specifies if the end intervals should be semi infinite</param>
        /// <returns>a set of intervals</returns>
        public static List<Interval> QuantileIntervals(int buckets, IList<double> values, bool open)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            List<double> orderedValues = values.OrderBy(d => d).ToList();
            List<Interval> result = new List<Interval>();

            double lowerBound = open ? double.NegativeInfinity : orderedValues[0],
                upperBound = open ? double.PositiveInfinity : orderedValues.Last();

            result.Add(new Interval(lowerBound, orderedValues[orderedValues.Count / buckets], !open, true));

            for (int i = 1; i < buckets - 1; i++)
                result.Add(new Interval(orderedValues[(orderedValues.Count * i) / buckets], orderedValues[(orderedValues.Count * (i + 1)) / buckets], false, true));

            result.Add(new Interval(orderedValues[(orderedValues.Count * (buckets - 1)) / buckets], upperBound, false, !open));

            return result;
        }
    }
}

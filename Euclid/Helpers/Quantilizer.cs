using Euclid.Histograms;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Helpers
{
    public static class Quantilizer
    {
        /// <summary>Builds the empirical quantiles from a given set of datas</summary>
        /// <param name="buckets">the number of buckets (ie for quartiles, should be 4 though there are only 3 quartiles)</param>
        /// <param name="values">the set of data</param>
        /// <param name="open">specifies if the end intervals should be semi infinite</param>
        /// <returns>a set of intervals</returns>
        public static List<Interval> QuantileIntervals(int buckets, IList<double> values, bool open)
        {
            List<double> orderedValues = values.OrderBy(d => d).ToList();
            List<Interval> result = new List<Interval>();

            double lowerBound = open ? double.NegativeInfinity : values[0],
                upperBound = open ? double.PositiveInfinity : values.Last();

            result.Add(new Interval(lowerBound, values[orderedValues.Count / buckets], !open, true));

            for (int i = 1; i < buckets - 1; i++)
                result.Add(new Interval(values[(orderedValues.Count * i) / buckets], values[(orderedValues.Count * (i + 1)) / buckets], false, true));

            result.Add(new Interval(values[(orderedValues.Count * (buckets - 1)) / buckets], upperBound, false, !open));

            return result;
        }
    }
}

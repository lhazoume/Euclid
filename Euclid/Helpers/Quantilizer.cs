using Euclid.Histograms;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Helpers
{
    public static class Quantilizer
    {
        public static List<Interval> QuantileIntervals(int buckets, IList<double> values)
        {
            List<double> orderedValues = values.OrderBy(d => d).ToList();
            List<Interval> result = new List<Interval>();

            result.Add(new Interval(double.NegativeInfinity, values[orderedValues.Count / buckets], false, true));

            for (int i = 1; i < buckets - 1; i++)
                result.Add(new Interval(values[(orderedValues.Count * i) / buckets], values[(orderedValues.Count * (i + 1)) / buckets], false, true));

            result.Add(new Interval(values[(orderedValues.Count * (buckets - 1)) / buckets], double.PositiveInfinity, false, false));

            return result;
        }
    }
}

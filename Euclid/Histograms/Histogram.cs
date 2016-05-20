using System.Collections.Generic;
using System.Linq;

namespace Euclid.Histograms
{
    /// <summary>
    /// Histogram class
    /// </summary>
    public class Histogram
    {
        #region Declarations
        private readonly Interval[] _intervals;
        private readonly int[] _items;
        #endregion

        #region Constructors
        /// <summary>
        /// Builds an <c>Histogram</c>
        /// </summary>
        /// <param name="intervals">the initial intervals</param>
        public Histogram(Interval[] intervals)
        {
            _intervals = new Interval[intervals.Length];
            _items = new int[intervals.Length];
            for (int i = 0; i < _intervals.Length; i++)
                _intervals[i] = new Interval(intervals[i].LowerBound, intervals[i].UpperBound);
        }
        #endregion

        #region Accessors
        /// <summary>Gets the number of intervals</summary>
        public int Count
        {
            get { return _intervals.Length; }
        }

        /// <summary>Gets the total number of items in the histogram</summary>
        public int TotalItems
        {
            get { return _items.Sum(); }
        }

        /// <summary>
        /// Gets the intervals
        /// </summary>
        public Interval[] Intervals
        {
            get { return _intervals; }
        }

        /// <summary>Gets the number of items in the i-th bucket</summary>
        /// <param name="bucket">the bucket's index</param>
        /// <returns>an int</returns>
        public int this[int bucket]
        {
            get { return _items[bucket]; }
        }
        #endregion

        #region Add
        /// <summary>Tabulates the value into to this instance</summary>
        /// <param name="value">the value</param>
        public void Tabulate(double value)
        {
            int i = 0;
            while (i < _intervals.Length)
            {
                if (_intervals[i].Contains(value))
                {
                    _items[i]++;
                    break;
                }
                i++;
            }
        }

        /// <summary>Tabulates these values into to this instance</summary>
        /// <param name="values">the value</param>
        public void Tabulate(IEnumerable<double> values)
        {
            foreach (double value in values)
                Tabulate(value);
        }
        #endregion

        #region Builders
        /// <summary>Creates an <c>Histogram</c> with regular buckets</summary>
        /// <param name="lowerBound">the support's lower bound</param>
        /// <param name="upperBound">the support's upper bound</param>
        /// <param name="numberOfIntervals">the number of intervals</param>
        /// <returns>an <c>Histogram</c></returns>
        public static Histogram Create(double lowerBound, double upperBound, int numberOfIntervals)
        {
            Interval[] intervals = new Interval[numberOfIntervals];
            double size = (upperBound - lowerBound) / numberOfIntervals;
            for (int i = 0; i < numberOfIntervals; i++)
                intervals[i] = new Interval(lowerBound + i * size, lowerBound + (i + 1) * size, true, false);
            return new Histogram(intervals);
        }
        #endregion
    }
}

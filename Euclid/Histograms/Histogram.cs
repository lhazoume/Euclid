using System.Collections.Generic;
using System;
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
        /// <summary>Builds an <c>Histogram</c></summary>
        /// <param name="intervals">the initial intervals</param>
        public Histogram(Interval[] intervals)
        {
            if (intervals == null) throw new ArgumentNullException(nameof(intervals));
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

        /// <summary>Gets the intervals</summary>
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
        /// <param name="occurences">the number of occurences of the value</param>
        public void Tabulate(double value, int occurences = 1)
        {
            int i = 0;
            while (i < _intervals.Length)
            {
                if (_intervals[i].Contains(value))
                {
                    _items[i] += occurences;
                    break;
                }
                i++;
            }
        }

        /// <summary>Tabulates these values into to this instance</summary>
        /// <param name="values">the value</param>
        public void Tabulate(IEnumerable<double> values)
        {
            IEnumerable<IGrouping<double, double>> groups = values.GroupBy(d => d);
            foreach (IGrouping<double, double> group in groups)
                Tabulate(group.Key, group.Count());
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

            if (double.IsInfinity(lowerBound) || double.IsInfinity(upperBound) || double.IsNaN(lowerBound) || double.IsNaN(upperBound))
                throw new NotSupportedException("The bounds are supposed to be finite nunmbers");
            if (upperBound <= lowerBound)
                throw new ArgumentException("The upper bound should be greater than the lower bound");
            if (numberOfIntervals <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfIntervals), "There can only be a positive number of buckets");

            Interval[] intervals = new Interval[numberOfIntervals];
            double size = (upperBound - lowerBound) / numberOfIntervals;
            for (int i = 0; i < numberOfIntervals; i++)
                intervals[i] = new Interval(lowerBound + i * size, lowerBound + (i + 1) * size, i == 0, true);
            return new Histogram(intervals);
        }
        #endregion
    }
}

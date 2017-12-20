using System;

namespace Euclid.Histograms
{
    /// <summary>
    /// Interval representation class
    /// </summary>
    public class Interval : IEquatable<Interval>
    {
        #region Declarations
        private Bound _lowerBound, _upperBound;
        #endregion

        #region Constructors
        /// <summary>
        /// Builds an interval
        /// </summary>
        /// <param name="lowerBound">the interval's lower bound</param>
        /// <param name="upperBound">the interval's upper bound</param>
        /// <param name="lowerIncluded">the lower bound's status </param>
        /// <param name="upperIncluded">the upper bound's status</param>
        public Interval(double lowerBound, double upperBound, bool lowerIncluded = true, bool upperIncluded = true)
        {
            _lowerBound = new Bound(lowerBound, lowerIncluded);
            _upperBound = new Bound(upperBound, upperIncluded);
        }

        /// <summary>
        /// Builds an interval
        /// </summary>
        /// <param name="lower">the lower bound</param>
        /// <param name="upper">the upper bound</param>
        public Interval(Bound lower, Bound upper)
        {
            _lowerBound = new Bound(lower.Value, lower.IsIncluded);
            _upperBound = new Bound(upper.Value, upper.IsIncluded);
        }

        /// <summary>
        /// Duplicates an <c>Interval</c>
        /// </summary>
        /// <param name="interval">the <c>Interval</c> to copy</param>
        private Interval(Interval interval)
            : this(interval.LowerBound, interval.UpperBound)
        { }
        #endregion

        #region Accessors
        /// <summary>Gets the interval's lower bound</summary>
        public Bound LowerBound
        {
            get { return _lowerBound; }
        }

        /// <summary>Gets the interval's upper bound</summary>
        public Bound UpperBound
        {
            get { return _upperBound; }
        }

        /// <summary>Gets a deep copy of the interval</summary>
        public Interval Clone
        {
            get { return new Interval(this); }
        }
        #endregion

        #region Methods
        /// <summary> Checks if the double is inside the interval </summary>
        /// <param name="x">the value</param>
        /// <returns><c>true</c> if x is inside the interval, <c>false</c> otherwise</returns>
        public bool Contains(double x)
        {
            return ((x > _lowerBound.Value || (_lowerBound.IsIncluded && x == _lowerBound.Value)) &&
                (x < _upperBound.Value || (_upperBound.IsIncluded && x == _upperBound.Value)));
        }

        /// <summary>
        /// Gives a string representation of the Interval
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString()
        {
            return string.Format("{0}{1}, {2}{3}", (_lowerBound.IsIncluded ? "[" : "]"),
                (_lowerBound.Value == double.NegativeInfinity ? "-∞" : _lowerBound.Value.ToString()),
                (_upperBound.Value == double.PositiveInfinity ? "+∞" : _upperBound.Value.ToString()),
                (_upperBound.IsIncluded ? "]" : "["));
        }
        #endregion

        private static Interval Inter(Interval i1, Interval i2)
        {
            if (i1._upperBound < i2._lowerBound || i2._upperBound < i1._lowerBound)
                return null;
            return new Interval(i1._lowerBound > i2._lowerBound ? i1._lowerBound : i2._lowerBound,
                i1._upperBound > i2._upperBound ? i2._upperBound : i1._upperBound);
        }

        /// <summary>Returns the intersection of a group of intervals</summary>
        /// <param name="intervals">the Intervals to intersect</param>
        /// <returns>an Interval</returns>
        public static Interval Intersection(params Interval[] intervals)
        {
            if (intervals.Length == 0) return null;
            if (intervals.Length == 1) return intervals[0].Clone;

            Interval intm = intervals[0].Clone;
            for (int i = 1; i < intervals.Length; i++)
            {
                intm = Inter(intm, intervals[i]);
                if (intm == null) return null;
            }
            return intm;
        }

        /// <summary>Equality comparer</summary>
        /// <param name="other">the other Interval</param>
        /// <returns>true if the bounds match, false otherwise</returns>
        public bool Equals(Interval other)
        {
            return other != null && other._lowerBound == _lowerBound && other._upperBound == _upperBound;
        }
    }
}

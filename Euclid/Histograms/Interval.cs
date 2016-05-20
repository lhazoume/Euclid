namespace Euclid.Histograms
{
    /// <summary>
    /// Interval representation class
    /// </summary>
    public class Interval
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
        public Interval(Interval interval)
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
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the double is inside the interval
        /// </summary>
        /// <param name="x">the value</param>
        /// <returns><c>true</c> if x is inside the interval, <c>false</c> otherwise</returns>
        public bool Contains(double x)
        {
            return (x >= _lowerBound && x <= _upperBound);
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
    }
}

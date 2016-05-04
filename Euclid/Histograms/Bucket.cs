namespace Euclid.Histograms
{
    public class Bucket
    {
        #region Declarations
        private Bound _lowerBound, _upperBound;
        #endregion

        public Bucket(double lowerBound, double upperBound, bool lowerIncluded = true, bool upperIncluded = true)
        {
            _lowerBound = new Bound(lowerBound, lowerIncluded);
            _upperBound = new Bound(upperBound, upperIncluded);
        }
        public Bucket(Bound lower, Bound upper)
        {
            _lowerBound = new Bound(lower.Value, lower.IsIncluded);
            _upperBound = new Bound(upper.Value, upper.IsIncluded);
        }
        public Bucket(Bucket bucket)
            : this(bucket.LowerBound, bucket.UpperBound)
        { }

        #region Accessors
        public Bound LowerBound
        {
            get { return _lowerBound; }
        }
        public Bound UpperBound
        {
            get { return _upperBound; }
        }
        #endregion

        public bool Contains(double x)
        {
            return (x >= _lowerBound && x <= _upperBound);
        }
        public override string ToString()
        {
            return string.Format("{0}{1}, {2}{3}", (_lowerBound.IsIncluded ? "[" : "]"),
                (_lowerBound.Value == double.NegativeInfinity ? "-" : _lowerBound.Value.ToString()),
                (_upperBound.Value == double.MaxValue ? "+" : _upperBound.Value.ToString()),
                (_upperBound.IsIncluded ? "]" : "["));
        }
    }
}

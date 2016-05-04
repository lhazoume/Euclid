using System.Collections.Generic;
using System.Linq;

namespace Euclid.Histograms
{
    public class Histogram
    {
        #region Declarations
        private Bucket[] _buckets;
        private int[] _items;
        #endregion

        #region Constructors
        public Histogram(Bucket[] buckets)
        {
            _buckets = new Bucket[buckets.Length];
            _items = new int[buckets.Length];
            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = new Bucket(buckets[i].LowerBound, buckets[i].UpperBound);
        }
        #endregion

        #region Accessors
        public int Count
        {
            get { return _buckets.Length; }
        }
        public int TotalItems
        {
            get { return _items.Sum(); }
        }
        public Bucket[] Buckets
        {
            get { return _buckets; }
        }
        public int this[int bucket]
        {
            get { return _items[bucket]; }
        }
        #endregion

        #region Add
        public void Tabulate(double value)
        {
            int i = 0;
            while (i < _buckets.Length)
            {
                if (_buckets[i].Contains(value))
                {
                    _items[i]++;
                    break;
                }
                i++;
            }
        }
        public void Tabulate(IEnumerable<double> values)
        {
            foreach (double value in values)
                Tabulate(value);
        }
        #endregion

        #region Builders
        public static Histogram Create(double lowerBound, double upperBound, int numberOfBuckets)
        {
            Bucket[] buckets = new Bucket[numberOfBuckets];
            double size = (upperBound - lowerBound) / numberOfBuckets;
            for (int i = 0; i < numberOfBuckets; i++)
                buckets[i] = new Bucket(lowerBound + i * size, lowerBound + (i + 1) * size, true, false);
            return new Histogram(buckets);
        }
        #endregion
    }
}

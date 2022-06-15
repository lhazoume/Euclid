using Euclid.Extensions;
using Euclid.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Euclid.DataStructures.IndexedSeries
{
#warning TimeDataFrame => Initialize mthd from abstraction IDataFrame could be avoid by using abstract static method but required C#9
    /// <summary>
    /// Class representing a DataFrame of synchronized data ordered by time 
    /// </summary>
    /// <typeparam name="DateTime">The legend type</typeparam>
    /// <typeparam name="TU">The data type</typeparam>
    /// <typeparam name="TV">The lable type</typeparam>
    public class TimeDataFrame<DateTime, TU, TV> : DataFrame<DateTime, TU, TV> where DateTime : IComparable<DateTime>, IEquatable<DateTime>, new() where TV : IEquatable<TV> where TU : new()
    {
        #region vars
        private readonly SortedDictionary<DateTime, Tuple<int, int>> _indexes = new SortedDictionary<DateTime, Tuple<int, int>>();
        #endregion

        #region constructor
        /// <summary>
        /// Protected constructor
        /// </summary>
        /// <param name="labels">Labels, serie of generic type</param>
        /// <param name="legends">Legends, serie of datetime type</param>
        /// <param name="data">Matric of generic type</param>
        protected TimeDataFrame(IHeader<TV> labels, IHeader<DateTime> legends, TU[,] data) : base(labels, legends, data) { }

        /// <summary>
        /// public constructor
        /// </summary>
        protected TimeDataFrame() { }

        /// <summary>
        /// Initialize TimeDataFrame instance
        /// </summary>
        /// <param name="labels">labels</param>
        /// <param name="legends">legends</param>
        /// <param name="data">data</param>
        protected override void Initialize(IList<TV> labels, IList<DateTime> legends, TU[,] data)
        {
            _data = Arrays.Clone(data);
            _labels = new Header<TV>(labels);
            _legends = new SortedHeader<DateTime>(legends);
        }
        #endregion

        #region accessors
        /// <summary>
        /// returns the chunk used for building index
        /// </summary>
        public IReadOnlyList<DateTime> Chunks => _indexes.Keys.ToArray();
        #endregion

        #region methods

        #region index time series
        public void BuildIndex(IEnumerable<DateTime> chunks, Predicate<DateTime> lowerBoundPredicate, Predicate<DateTime> upperBoundPredicate)
        {
            try
            {
                if (chunks == null) throw new ArgumentNullException(nameof(chunks));

                IReadOnlyList<DateTime> items = _legends.Values;

                foreach(DateTime chunk in chunks)
                {
                    //int start = items.FindFirstIndexOf(x => x.CompareTo(chunk) >= 0);
                    int start = items.FindFirstIndexOf(lowerBoundPredicate);
                    if (start == -1) throw new Exception($"Impossible to find the starting index of the chunk [{chunk}]");

                    int end = items.FindLastIndexOf(upperBoundPredicate, (uint)start);
                    if (start == -1) throw new Exception($"Impossible to find the ending index of the chunk [{chunk}]");

                    _indexes.Add(chunk, new Tuple<int, int>(start, end));
                }
            }
            catch(Exception ex) { throw ex; }
        }
        #endregion

        #endregion

    }
}

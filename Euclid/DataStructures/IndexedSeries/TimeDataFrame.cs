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
    public class TimeDataFrame<TU, TV> : IDataFrame<DateTime, TU, TV> where TV : IEquatable<TV>
    {
        #region vars
        private readonly SortedDictionary<DateTime, Tuple<uint, uint>> _indexes = new SortedDictionary<DateTime, Tuple<uint, uint>>();
        private readonly List<DateTime> _chunks = new List<DateTime>();
        private DateTime[] _timestamps;
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
            _timestamps = _legends.Values;
        }
        #endregion

        #region accessors
        /// <summary>
        /// returns the chunk used for building index
        /// </summary>
        public IReadOnlyList<DateTime> Chunks => _chunks;
        /// <summary>
        /// returns the values of legend
        /// </summary>
        public override DateTime[] Legends => _timestamps;
        /// <summary>
        /// Precise if the chunk has intraday granularity
        /// </summary>
        public bool IntradayChunk { get; private set; }
        #endregion

        #region methods

        #region index time series
        /// <summary>
        /// Build index for searching into the time series
        /// </summary>
        /// <param name="chunks">Chunk of time</param>
        /// <param name="intraday">Precise if index is intraday or extraday</param>
        public void BuildIndex(IEnumerable<DateTime> chunks, bool intraday = false)
        {
            try
            {
                if (chunks == null) throw new ArgumentNullException(nameof(chunks));
                IReadOnlyList<DateTime> items = _legends.Values;

                foreach (DateTime chunk in chunks)
                {
                    Func<DateTime, bool> lowerBoundPredicate = intraday ? (Func<DateTime, bool>)(ts => ts >= chunk) : (ts => ts.Date >= chunk);
                    Func<DateTime, bool> upperBoundPredicate = intraday ? (Func<DateTime, bool>)(ts => ts > chunk) : (ts => ts.Date > chunk);

                    int start = items.FindFirstIndexOf(lowerBoundPredicate);
                    if (start == -1) throw new Exception($"Impossible to find the starting index of the chunk [{chunk}]");

                    int end = items.FindFirstIndexOf(upperBoundPredicate, (uint)start);
                    if (end == -1) end = items.Count - 1;

                    _indexes.Add(chunk, new Tuple<uint, uint>((uint)start, (uint)end));
                    _chunks.Add(chunk);
                }

                IntradayChunk = intraday;
            }
            catch (Exception ex) { throw ex; }
        }
        #endregion

        #region search
        /// <summary>
        /// Find first index of a predicate by using binary search + indexes
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="chunk">Chunk value</param>
        /// <returns>index of the target else -1</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int FindFirstIndexOf(Func<DateTime, bool> predicate, DateTime? chunk = null)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            if(!chunk.HasValue) return _timestamps.FindFirstIndexOf(predicate); // chunk does not help

            int chunkIdx = _chunks.FindFirstIndexOf(IntradayChunk ? (Func<DateTime, bool>)(c => c >= chunk.Value) : (c => c >= chunk.Value));
            if (chunkIdx == -1) return _timestamps.FindFirstIndexOf(predicate); // chunk does not help

            Tuple<uint, uint> boundaries = _indexes[_chunks[chunkIdx]];
            return _timestamps.FindFirstIndexOf(predicate, boundaries.Item1, boundaries.Item2);
        }

        /// <summary>
        /// Find last index of a predicate by using binary search + indexes
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="chunk">Chunk value</param>
        /// <returns>index of the target else -1</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int FindLastIndexOf(Func<DateTime, bool> predicate, DateTime? chunk = null)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            if (!chunk.HasValue) return _timestamps.FindLastIndexOf(predicate); // chunk does not help

            int chunkIdx = _chunks.FindFirstIndexOf(IntradayChunk ? (Func<DateTime, bool>)(c => c >= chunk.Value) : (c => c >= chunk.Value));
            if (chunkIdx == -1) return _timestamps.FindLastIndexOf(predicate); // chunk does not help

            Tuple<uint, uint> boundaries = _indexes[_chunks[chunkIdx]];
            return _timestamps.FindLastIndexOf(predicate, boundaries.Item1, boundaries.Item2);
        }
        #endregion

        #endregion

    }
}

using Euclid.Extensions;
using Euclid.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.DataStructures.IndexedSeries
{
#warning TimeDataFrame => Initialize mthd from abstraction IDataFrame could be avoid by using abstract static method but required C#9
    /// <summary>Class representing a DataFrame of synchronized data ordered by time </summary>
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
        protected TimeDataFrame(IHeader<TV> labels, IHeader<DateTime> legends, TU[][] data) : base(labels, legends, data) { }

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
        protected override void Initialize(IList<TV> labels, IList<DateTime> legends, TU[][] data)
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

        #region extraction & getters
        /// <summary>Extracts the part of the time dataFrame whose legends between the starting & ending date (include)</summary>
        /// <param name="start">Starting date which includes the targeting legends</param>
        /// <param name="end">Ending date which includes the targeting legends</param>
        /// <returns>Matching time dataframe</returns>
        public TimeDataFrame<TU, TV> FastExtractByLegend(DateTime start, DateTime end)
        {
            #region requirements
            if(start > end) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the ending date [{end.ToShortDateString()}]!");

            if (start > Legends[Legends.Length - 1]) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the last legend [{Legends[Legends.Length - 1].ToShortDateString()}]!");
            if (end < Legends[0]) throw new Exception($"ExtractByLegend: Ending date [{end.ToShortDateString()}] is inferior to the first legend [{Legends[0].ToShortDateString()}]!");
            #endregion

            int startIdx = FindFirstIndexOf(l => l.Date >= start.Date, start.Date);
            if (startIdx == -1) throw new Exception($"Impossible to find the FindFirstIndexOf [{start.ToShortDateString()}] into the legends!");

            int endIdx = FindLastIndexOf(l => l.Date <= end.Date, end.Date);
            if (endIdx == -1) throw new Exception($"Impossible to find the FindLastIndexOf [{end.ToShortDateString()}] into the legends!");

            List<DateTime> legends = new List<DateTime>();
            int N = endIdx - startIdx, M = Columns;
            TU[][] data = Arrays.Build<TU>(N, M);

            for(int i = startIdx; i <= endIdx; i++)
            {
                legends.Add(Legends[i]);
                for (int j = 0; j < M; j++) data[i - startIdx][j] = this[i, j];
            }

            return Create<TimeDataFrame<TU, TV>>(Labels, legends, data);
        }

        /// <summary>Extracts the part of the values of the time dataFrame whose legends between the starting & ending date (include)</summary>
        /// <param name="start">Starting date which includes the targeting legends</param>
        /// <param name="end">Ending date which includes the targeting legends</param>
        /// <returns>Matching data</returns>
        public TU[][] FastExtractValuesByLegend(DateTime start, DateTime end)
        {
            #region requirements
            if (start > end) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the ending date [{end.ToShortDateString()}]!");

            if (start > Legends[Legends.Length - 1]) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the last legend [{Legends[Legends.Length - 1].ToShortDateString()}]!");
            if (end < Legends[0]) throw new Exception($"ExtractByLegend: Ending date [{end.ToShortDateString()}] is inferior to the first legend [{Legends[0].ToShortDateString()}]!");
            #endregion

            int startIdx = FindFirstIndexOf(l => l.Date >= start.Date, start.Date);
            if (startIdx == -1) throw new Exception($"Impossible to find the FindFirstIndexOf [{start.ToShortDateString()}] into the legends!");

            int endIdx = FindLastIndexOf(l => l.Date <= end.Date, end.Date);
            if (endIdx == -1) throw new Exception($"Impossible to find the FindLastIndexOf [{end.ToShortDateString()}] into the legends!");

            int N = endIdx - startIdx, M = Columns;
            TU[][] data = Arrays.Build<TU>(N, M);

            for (int i = startIdx; i <= endIdx; i++)
                for (int j = 0; j < M; j++) 
                    data[i - startIdx][j] = this[i, j];

            return data;
        }

        /// <summary>Extracts the part of the values of the time dataFrame whose legends between the starting & ending date (include) & label predicate</summary>
        /// <param name="start">Starting date which includes the targeting legends</param>
        /// <param name="end">Ending date which includes the targeting legends</param>
        /// <param name="predicate">The predicate on the labels</param>
        /// <returns>Matching data</returns>
        public TU[][] FastExtractValuesByLegendAndLabels(DateTime start, DateTime end, Func<TV, bool> predicate)
        {
            #region requirements
            if (start > end) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the ending date [{end.ToShortDateString()}]!");

            if (start > Legends[Legends.Length - 1]) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the last legend [{Legends[Legends.Length - 1].ToShortDateString()}]!");
            if (end < Legends[0]) throw new Exception($"ExtractByLegend: Ending date [{end.ToShortDateString()}] is inferior to the first legend [{Legends[0].ToShortDateString()}]!");
            #endregion

            int startIdx = FindFirstIndexOf(l => l.Date >= start.Date, start.Date);
            if (startIdx == -1) throw new Exception($"Impossible to find the FindFirstIndexOf [{start.ToShortDateString()}] into the legends!");

            int endIdx = FindLastIndexOf(l => l.Date <= end.Date, end.Date);
            if (endIdx == -1) throw new Exception($"Impossible to find the FindLastIndexOf [{end.ToShortDateString()}] into the legends!");

            List<TV> labels = _labels.Where(predicate).ToList();
            int N = endIdx - startIdx, M = Columns;
            TU[][] data = Arrays.Build<TU>(N, M);

            for (int j = 0; j < M; j++)
            {
                int k = _labels[labels[j]];
                for (int i = startIdx; i <= endIdx; i++) data[i - startIdx][j] = this[i, k];
            }

            return data;
        }

        /// <summary> Gets the data-point column of the given label</summary>
        /// <param name="label">the label</param>
        /// <returns> a <c>Time series</c></returns>
        public new TimeSeries<TU, TV> GetSeriesAt(TV label)
        {
            int index = _labels[label];
            if (index == -1) throw new ArgumentException($"Label [{label}] was not found");
            TU[] result = new TU[_legends.Count];
            for (int i = 0; i < _legends.Count; i++)
                result[i] = _data[i][index];
            return TimeSeries<TU, TV>.Create<TimeSeries<TU, TV>>(label, _legends, result);
        }
        #endregion

        #endregion

    }
}

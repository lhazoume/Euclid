using Euclid.Extensions;
using Euclid.Logging;
using Euclid.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

        #region create
        /// <summary>
        /// Builds a <c>TimeDataFrame</c> by synchronizing timeseries
        /// </summary>
        /// <typeparam name="TY">IDataFrame implementation</typeparam>
        /// <param name="series">Series</param>
        /// <returns>TimeDataFrame from synchronized timeseries</returns>
        public static TY CreateFromUnsynchronizedTimeseries<TY>(IEnumerable<TimeSeries<TU, TV>> series) where TY : TimeDataFrame<TU, TV>
        {
            if (series == null) throw new ArgumentNullException(nameof(series));

            #region build synchronized legend(s)
            int M = series.Count();
            IReadOnlyList<DateTime> legends = series.First().Legends;

            for (int i = 1; i < M; i++)
            {
                TimeSeries<TU,TV> candidat = series.ElementAt(i);

                ResultOutput<IReadOnlyList<DateTime>> qoIntersection = legends.Intersection(candidat.Legends);
                if (qoIntersection.Failed) throw new Exception(qoIntersection.Message);
                legends = qoIntersection.Result;
            }
            #endregion

            if (legends == null || legends.Count == 0) throw new Exception($"No matching legends between timeseries [{string.Join(";", series.Select(s => s.Label))}]");

            #region fill data
            int N = legends.Count;
            TU[][] data = Arrays.Build<TU>(N, M);

            for (int j = 0; j < M; j++)
            {
                TimeSeries<TU, TV> candidat = series.ElementAt(j);
                for (int i = 0; i < N; i++) data[i][j] = candidat[legends[i]];

            }
            #endregion

            return Create<TY>(series.Select(s => s.Label).ToList(), legends.ToArray(), data);
        }

        /// <summary>
        /// Create a synchronized <c>timedataframe</c> from a collection of sub unsynchronized <c>timedataframe</c>
        /// </summary>
        /// <typeparam name="TY">IDataFrame implementation</typeparam>
        /// <param name="tfs">Collection of sub unsynchronized <c>timedataframe</c></param>
        /// <returns>Synchronized <c>timedataframe</c></returns>
        public static ResultOutput<TY> CreateFromUnsynchronizedTF<TY>(IEnumerable<TimeDataFrame<TU, TV>> tfs) where TY : TimeDataFrame<TU, TV>
        {
            try
            {
                if (tfs == null) throw new ArgumentNullException(nameof(tfs));

                #region build synchronized legends
                int M = tfs.Count();
                IReadOnlyList<DateTime> legends = tfs.First().Legends;

                for (int i = 1; i < M; i++)
                {
                    DateTime[] candidats = tfs.ElementAt(i).Legends;

                    ResultOutput<IReadOnlyList<DateTime>> qoIntersection = legends.Intersection(candidats);
                    if (qoIntersection.Failed) throw new Exception(qoIntersection.Message);
                    legends = qoIntersection.Result;
                }
                #endregion

                if (legends == null || legends.Count == 0) throw new Exception($"No matching legends between timedataframe [{string.Join(";", tfs.Select(s => string.Join(";", s.Labels)))}]");

                #region fill data
                int N = legends.Count, k = 0; M = tfs.Sum(tf => tf.Labels.Length);
                TU[][] data = Arrays.Build<TU>(N, M);
                List<TV> labels = new List<TV>(M);

                foreach(TimeDataFrame<TU, TV> candidat in tfs)
                {
                    int K = candidat.Labels.Length;
                    for(int j = 0; j < K; j++)
                    {
                        for (int i = 0; i < N; i++) data[i][k] = candidat[i, j];
                        k++;
                    }
                    labels.AddRange(candidat.Labels);
                }
                #endregion

                return Create<TY>(labels, legends.ToArray(), data);
            }

            catch(Exception ex) { return ResultOutput < TY > .CreateFailed($"TimeDataFrame.CreateFromUnsynchronizedTF: {ex.Message}"); }            
        }
        #endregion

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

        #region adding
        /// <summary>
        /// Add a timeseries to the time dataframe
        /// </summary>
        /// <param name="series">Timeseries</param>
        public void AddSeries(TimeSeries<TU, TV> series)
        {
            if (_indexes.Count > 0) throw new Exception($"Cannot add a series to a dataframe if an index is present!");
            if (series == null) throw new ArgumentNullException(nameof(series));
            if (series.Rows < 1) throw new Exception($"None element into the series");

            ResultOutput<IReadOnlyList<DateTime>> qoIntersection =  _timestamps.Intersection(series.Legends);
            if (qoIntersection.Failed) throw new Exception(qoIntersection.Message);
            IReadOnlyList<DateTime> legends = qoIntersection.Result;

            TU[][] newData = Arrays.Build<TU>(legends.Count, _labels.Count + 1);
            Parallel.For(0, legends.Count, i =>
            {
                DateTime legend = legends[i];
                newData[i][_labels.Count] = series[legend];
                for (int j = 0; j < _labels.Count; j++)
                    newData[i][j] = this[legend,j];
            });

            _labels.Add(series.Label);
            _data = newData;
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
            int N = endIdx - startIdx + 1, M = Columns;
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

            int N = endIdx - startIdx + 1, M = Columns;
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
            if (start > end) throw new Exception($"FastExtractValuesByLegendAndLabels: Starting date [{start.ToShortDateString()}] is superior to the ending date [{end.ToShortDateString()}]!");

            if (start > Legends[Legends.Length - 1]) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the last legend [{Legends[Legends.Length - 1].ToShortDateString()}]!");
            if (end < Legends[0]) throw new Exception($"FastExtractValuesByLegendAndLabels: Ending date [{end.ToShortDateString()}] is inferior to the first legend [{Legends[0].ToShortDateString()}]!");
            #endregion

            int startIdx = FindFirstIndexOf(l => l.Date >= start.Date, start.Date);
            if (startIdx == -1) throw new Exception($"FastExtractValuesByLegendAndLabels: Impossible to find the FindFirstIndexOf [{start.ToShortDateString()}] into the legends!");

            int endIdx = FindLastIndexOf(l => l.Date <= end.Date, end.Date);
            if (endIdx == -1) throw new Exception($"FastExtractValuesByLegendAndLabels: Impossible to find the FindLastIndexOf [{end.ToShortDateString()}] into the legends!");

            List<TV> labels = _labels.Where(predicate).ToList();
            int N = endIdx - startIdx + 1, M = labels.Count;
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

        /// <summary>
        /// Get values between a range of date for a specified label
        /// </summary>
        /// <param name="start">Starting date</param>
        /// <param name="end">Ending date</param>
        /// <param name="label">Label</param>
        /// <returns>Values</returns>
        /// <exception cref="Exception"></exception>
        public TU[] GetValuesBetween(DateTime start, DateTime end, TV label)
        {
            #region requirements
            if (start > end) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the ending date [{end.ToShortDateString()}]!");

            if (start > Legends[Legends.Length - 1]) throw new Exception($"ExtractByLegend: Starting date [{start.ToShortDateString()}] is superior to the last legend [{Legends[Legends.Length - 1].ToShortDateString()}]!");
            if (end < Legends[0]) throw new Exception($"ExtractByLegend: Ending date [{end.ToShortDateString()}] is inferior to the first legend [{Legends[0].ToShortDateString()}]!");
            #endregion

            #region found period
            bool hasIndex = _indexes.Count > 0;
            int startIdx =  FindFirstIndexOf(l => l.Date >= start.Date, hasIndex ? start.Date : (DateTime?)null);
            if (startIdx == -1) throw new Exception($"FastExtractValuesByLegendAndLabels: Impossible to find the FindFirstIndexOf [{start.ToShortDateString()}] into the legends!");

            int endIdx = FindLastIndexOf(l => l.Date <= end.Date, hasIndex ? end.Date : (DateTime?)null);
            if (endIdx == -1) throw new Exception($"FastExtractValuesByLegendAndLabels: Impossible to find the FindLastIndexOf [{end.ToShortDateString()}] into the legends!");
            #endregion

            #region retrieve values
            int N = endIdx - startIdx + 1, k = _labels[label];
            TU[] data = new TU[N];

            for (int i = startIdx; i <= endIdx; i++) data[i - startIdx] = this[i, k];
            #endregion

            return data;
        }
        #endregion

        #endregion

    }
}

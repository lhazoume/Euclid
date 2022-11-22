using Euclid.Extensions;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>Class representing a template of DataFrame of synchronized data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="TU">the data type</typeparam>
    /// <typeparam name="TV">the label type</typeparam>
    public abstract class IDataFrame<T, TU, TV> : IIndexedSeries<T, TU, TV> where T : IComparable<T>, IEquatable<T> where TV : IEquatable<TV>
    {
        #region vars
        /// <summary>
        /// Columns variable
        /// </summary>
        protected IHeader<TV> _labels;
        /// <summary>
        /// Row variable
        /// </summary>
        protected IHeader<T> _legends;
        /// <summary>
        /// Matrix of data
        /// </summary>
        protected TU[][] _data;
        #endregion

        #region constructors
        /// <summary>
        /// Hidden constructor
        /// </summary>
        /// <param name="labels">header of labels</param>
        /// <param name="legends">header of legends</param>
        /// <param name="data">data</param>
        protected IDataFrame(IHeader<TV> labels, IHeader<T> legends, TU[][] data)
        {
            _data = Arrays.Clone(data);
            _labels = labels;
            _legends = legends;
        }

        /// <summary>
        /// public constructor
        /// </summary>
        public IDataFrame() { }

        /// <summary>
        /// Initialize an instance of IDataFrame
        /// </summary>
        /// <param name="labels"></param>
        /// <param name="legends"></param>
        /// <param name="data"></param>
        protected abstract void Initialize(IList<TV> labels, IList<T> legends, TU[][] data);

        /// <summary>
        /// Initialize the data matrix with the specified value.
        /// </summary>
        /// <param name="value">Value used for initialization</param>
        public void Initialize(TU value)
        {
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j <Columns; j++)
                    _data[i][j] = value;
        }

        /// <summary>
        /// Builds a <c>DataFrame</c> from its serialized form
        /// </summary>
        /// <typeparam name="TY">IDataFrame implementation</typeparam>
        /// <param name="node">the <c>XmlNode</c></param>
        /// <returns>Instance</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TY Create<TY>(XmlNode node) where TY : IDataFrame<T, TU, TV>
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            XmlNodeList labelNodes = node.SelectNodes("dataFrame/label"),
                legendNodes = node.SelectNodes("dataFrame/legend"),
                dataNodes = node.SelectNodes("dataFrame/point");

            #region Labels
            TV[] labels = new TV[labelNodes.Count];
            foreach (XmlNode label in labelNodes)
            {
                int index = int.Parse(label.Attributes["index"].Value);
                labels[index] = label.Attributes["value"].Value.Parse<TV>();
            }
            #endregion

            #region Legends
            T[] legends = new T[legendNodes.Count];
            foreach (XmlNode legend in legendNodes)
            {
                int index = int.Parse(legend.Attributes["index"].Value);
                legends[index] = legend.Attributes["value"].Value.Parse<T>();
            }
            #endregion

            #region Data
            TU[][] data = Arrays.Build<TU>(legends.Length, labels.Length);
            foreach (XmlNode point in dataNodes)
            {
                int row = int.Parse(point.Attributes["row"].Value),
                    col = int.Parse(point.Attributes["col"].Value);
                TU value = point.Attributes["value"].Value.Parse<TU>();
                data[row][col] = value;
            }
            #endregion

            return Create<TY>(labels, legends, data);
        }

        /// <summary>
        /// Create an instance of IDataFrame
        /// </summary>
        /// <typeparam name="TY">Generic type</typeparam>
        /// <param name="labels">labels</param>
        /// <param name="legends">legends</param>
        /// <param name="data">data</param>
        /// <returns>Instance</returns>
        public static TY Create<TY>(IList<TV> labels, IList<T> legends, TU[][] data) where TY : IDataFrame<T, TU, TV>
        {
            ConstructorInfo constructor = typeof(TY).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(ct => ct.GetParameters().Length == 0);
            if (constructor == null) throw new Exception("Impossible to find the matching constructor (protected, without parameters)!");
            TY dataframe = (TY)constructor.Invoke(null);
            dataframe.Initialize(labels, legends, data);
            return dataframe;
        }

        /// <summary>Builds a <c>DataFrame</c></summary>
        /// <typeparam name="TY">IDataFrame implementation</typeparam>
        /// <param name="labels">the labels</param>
        /// <param name="legends">the legends</param>
        /// <returns>a <c>DataFrame</c></returns>
        public static TY Create<TY>(IList<TV> labels, IList<T> legends) where TY : IDataFrame<T, TU, TV>
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            if (legends == null) throw new ArgumentNullException(nameof(legends));

            return Create<TY>(labels, legends, Arrays.Build<TU>(legends.Count, labels.Count));
        }

        /// <summary>
        /// Builds a <c>DataFrame</c>
        /// </summary>
        /// <typeparam name="TY">IDataFrame implementation</typeparam>
        /// <param name="slices">Slices</param>
        /// <returns>Instance</returns>
        public static TY Create<TY>(IEnumerable<Slice<T, TU, TV>> slices) where TY : IDataFrame<T, TU, TV>
        {
            TU[][] data = Arrays.Build<TU>(slices.Count(), slices.ElementAt(0).Columns);
            for (int i = 0; i < data.Length; i++)
                for (int j = 0; j < data[i].Length; j++)
                    data[i][j] = slices.ElementAt(i)[j];

            return Create<TY>(slices.ElementAt(0).Labels, slices.Select(s => s.Legend).ToList(), data);
        }

        /// <summary>
        /// Builds a <c>DataFrame</c>
        /// </summary>
        /// <typeparam name="TY">IDataFrame implementation</typeparam>
        /// <param name="series">Series</param>
        /// <returns>Instance</returns>
        public static TY Create<TY>(IEnumerable<Series<T, TU, TV>> series) where TY : IDataFrame<T, TU, TV>
        {
            TU[][] data = Arrays.Build<TU>(series.ElementAt(0).Rows, series.Count());
            for (int i = 0; i < data.Length; i++)
                for (int j = 0; j < data[i].Length; j++)
                    data[i][j] = series.ElementAt(j)[i];
            return Create<TY>(series.Select(s => s.Label).ToList(), series.ElementAt(0).Legends, data);
        }

        /// <summary>Builds a <c>DataFrame</c> from a CSV string</summary>
        /// <typeparam name="TY">IDataFrame implementation</typeparam>
        /// <param name="text">the serialized version of the data</param>
        /// <returns>Instance</returns>
        /// <exception cref="ArgumentNullException">Text empty</exception>
        public static TY Create<TY>(string text) where TY : IDataFrame<T, TU, TV>
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            char[] separator = CsvHelper.Separator.ToCharArray();

            #region Labels
            string[] labelStrings = lines[0].Split(separator, StringSplitOptions.None);
            labelStrings = labelStrings.SubArray(1, labelStrings.Length - 1);
            int cols = labelStrings.Length;

            TV[] labels = new TV[cols];
            for (int j = 0; j < cols; j++)
                labels[j] = labelStrings[j].Parse<TV>();
            #endregion

            #region Legends and data
            int rows = lines.Length - 1;
            T[] legends = new T[rows];
            TU[][] data = Arrays.Build<TU>(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                string[] lineSplit = lines[1 + i].Split(separator, StringSplitOptions.None);

                legends[i] = lineSplit[0].Parse<T>();

                for (int j = 0; j < cols; j++)
                    data[i][j] = lineSplit[1 + j].Parse<TU>();
            }
            #endregion

            return Create<TY>(labels, legends, data);
        }
        #endregion

        #region Accessors
        /// <summary>Returns the legends </summary>
        public virtual T[] Legends => _legends.Values;

        /// <summary>Returns the labels </summary>
        public TV[] Labels => _labels.Values;

        /// <summary>Returns the number of columns</summary>
        public int Columns => _labels.Count;

        /// <summary>Returns the number of rows</summary>
        public int Rows => _legends.Count;

        /// <summary>Gets the data</summary>
        public TU[][] Data => _data;


        /// <summary>Gets and sets the data for the i-th row and j-th column of the <c>DataFrame</c></summary>
        /// <param name="i">the row index</param>
        /// <param name="j">the column index</param>
        /// <returns>a data point</returns>
        public TU this[int i, int j]
        {
            get { return _data[i][j]; }
            set { _data[i][j] = value; }
        }

        /// <summary>Gets and sets the data for a given legend and a given label</summary>
        /// <param name="t">the legend</param>
        /// <param name="v">the label</param>
        /// <returns>a data point</returns>
        public TU this[T t, TV v]
        {
            get { return _data[_legends[t]][_labels[v]]; }
            set { _data[_legends[t]][_labels[v]] = value; }
        }
        #endregion

        #region methods

        #region Label
        /// <summary>Returns the label rank</summary>
        /// <param name="label">the target label</param>
        /// <returns>an <c>Integer</c></returns>
        public int GetLabelRank(TV label) { return _labels.Contains(label) ? _labels[label] : -1; }
        #endregion

        #region legend
        /// <summary>Returns the legend rank</summary>
        /// <param name="legend">the target legend</param>
        /// <returns>an <c>Integer</c></returns>
        public int GetLegendRank(T legend) { return _legends.Contains(legend) ? _legends[legend] : -1; }
        #endregion

        #region Series

        #region Get
        /// <summary> Gets the data-point column of the given label</summary>
        /// <param name="label">the label</param>
        /// <returns> a <c>Series</c></returns>
        public Series<T, TU, TV> GetSeriesAt(TV label)
        {
            int index = _labels[label];
            if (index == -1) throw new ArgumentException($"Label [{label}] was not found");
            TU[] result = new TU[_legends.Count];
            for (int i = 0; i < _legends.Count; i++)
                result[i] = _data[i][ index];
            return Series<T, TU, TV>.Create<Series<T, TU, TV>>(label, _legends, result);
        }

        /// <summary> Gets the data-point column of the given label</summary>
        /// <param name="label">the label</param>
        /// <param name="predicate">legend predicate</param>
        /// <returns> a <c>Series</c></returns>
        public Series<T, TU, TV> GetSeriesAt(TV label, Func<T, bool> predicate)
        {
            int indexLabel = _labels[label];
            if (indexLabel == -1) throw new ArgumentException($"Label [{label}] was not found");


            IEnumerable<int> matchingIndices = _legends.FindIndices(predicate);
            IList<TU> data = new List<TU>();
            Header<T> legends = new Header<T>();

            foreach (int indice in matchingIndices)
            {
                legends.Add(_legends.ElementAt(indice));
                data.Add(_data[indice][indexLabel]);
            }
            return Series<T, TU, TV>.Create<Series<T, TU, TV>>(label, legends.Values, data.ToArray());
        }

        /// <summary> Gets all the data as an array of <c>Series</c></summary>
        /// <returns>an array of <c>Series</c></returns>
        public Series<T, TU, TV>[] GetSeries()
        {
            Series<T, TU, TV>[] result = new Series<T, TU, TV>[_labels.Count];
            for (int j = 0; j < _labels.Count; j++)
            {
                TU[] data = new TU[_legends.Count];
                for (int i = 0; i < _legends.Count; i++)
                    data[i] = _data[i][j];

                result[j] = Series<T, TU, TV>.Create<Series<T, TU, TV>>(_labels.ElementAt(j), _legends, data);
            }
            return result;
        }
        #endregion

        #region Add

        /// <summary> Adds a column to the <c>DataFrame</c></summary>
        /// <param name="label">the new column's label</param>
        /// <param name="column">the new column's data</param>
        public void AddSeries(TV label, TU[] column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));

            TU[][] newData = Arrays.Build<TU>(_legends.Count, _labels.Count + 1);
            for (int i = 0; i < _legends.Count; i++)
            {
                newData[i][_labels.Count] = column[i];
                for (int j = 0; j < _labels.Count; j++)
                    newData[i][j] = _data[i][j];
            }

            _labels.Add(label);
            _data = newData;
        }

        /// <summary>Adds an empty column to the <c>DataFrame</c></summary>
        /// <param name="label">the new column's label</param>
        public void AddSeries(TV label)
        {
            TU[] newColumn = Enumerable.Range(0, _legends.Count).Select(u => default(TU)).ToArray();
            AddSeries(label, newColumn);
        }

        #endregion

        #region Take

        /// <summary>Takes a <c>Series</c> from the DataFrame thereby removing it from the DataFrame</summary>
        /// <param name="label">the label of the <c>Series</c> to take</param>
        /// <returns> a <c>Series</c></returns>
        public Series<T, TU, TV> TakeSeries(TV label)
        {
            int indexToRemove = _labels[label];
            if (indexToRemove == -1 || _labels.Count == 1) return null;

            TU[] takenData = new TU[_legends.Count];
            TU[][] newData = Arrays.Build<TU>(_legends.Count, _data.Length - 1);
            for (int j = 0; j < _labels.Count; j++)
            {
                if (j == indexToRemove)
                    for (int i = 0; i < _legends.Count; i++)
                        takenData[i] = _data[i][j];
                else
                {
                    for (int i = 0; i < _legends.Count; i++)
                        newData[i][j - (j < indexToRemove ? 0 : 1)] = _data[i][j];
                }
            }
            _labels.Remove(label);
            _data = newData;

            return Series<T, TU, TV>.Create<Series<T, TU, TV>>(label, _legends, takenData);
        }

        #endregion

        #region extract

        #endregion

        #region Remove

        /// <summary>Remove the data for a given label</summary>
        /// <param name="label">the label</param>
        public bool RemoveSeriesAt(TV label)
        {
            int indexToRemove = _labels[label];
            if (indexToRemove == -1 || _labels.Count == 1) return false;
            TU[][] newData = Arrays.Build<TU>(_legends.Count, _data.Length - 1);

            for (int j = 0; j < _labels.Count; j++)
            {
                if (j == indexToRemove) continue;
                for (int i = 0; i < _legends.Count; i++)
                    newData[i][j - (j < indexToRemove ? 0 : 1)] = _data[i][j];
            }

            _labels.Remove(label);
            _data = newData;
            return true;
        }

        #endregion

        #region Extract
        /// <summary>Extracts the part of the DataFrame whose labels obeys the predicate</summary>
        /// <param name="predicate">the predicate on the labels</param>
        /// <returns>a DataFrame</returns>
        public TY ExtractByLabels<TY>(Func<TV, bool> predicate) where TY : IDataFrame<T, TU, TV>
        {
            return Create<TY>(_labels.Where(predicate).Select(l => GetSeriesAt(l)));
        }
        #endregion

        #endregion

        #region Slices

        #region Get

        /// <summary>Gets the data-point row of the given legend</summary>
        /// <param name="legend">the legend</param>
        /// <returns>a <c>Slice</c></returns>
        public Slice<T, TU, TV> GetSliceAt(T legend)
        {
            int index = _legends[legend];
            if (index == -1) throw new ArgumentException($"Legend [{legend}] was not found");
            TU[] result = new TU[_labels.Count];
            for (int j = 0; j < _labels.Count; j++)
                result[j] = _data[index][j];
            return Slice<T, TU, TV>.Create(_labels, legend, result);
        }

        /// <summary>Gets all the data as an array of <c>Slice</c></summary>
        /// <returns>an array of <c>Slice</c></returns>
        public Slice<T, TU, TV>[] GetSlices()
        {
            Slice<T, TU, TV>[] result = new Slice<T, TU, TV>[_legends.Count];
            for (int i = 0; i < _legends.Count; i++)
            {
                TU[] data = new TU[_labels.Count];
                for (int j = 0; j < _labels.Count; j++)
                    data[j] = _data[i][j];

                result[i] = Slice<T, TU, TV>.Create(_labels, _legends.ElementAt(i), data);
            }
            return result;
        }

        #endregion

        #region Add

        /// <summary> Adds a slice to the <c>DataFrame</c></summary>
        /// <param name="legend">the new slice's legend</param>
        /// <param name="slice">the new slice's data</param>
        public void AddSlice(T legend, TU[] slice)
        {
            if (slice == null) throw new ArgumentNullException(nameof(slice));

            TU[][] newData = Arrays.Build<TU>(_legends.Count + 1, _labels.Count);
            for (int j = 0; j < _labels.Count; j++)
            {
                newData[_legends.Count][j] = slice[j];
                for (int i = 0; i < _legends.Count; i++)
                    newData[i][j] = _data[i][j];
            }

            _legends.Add(legend);
            _data = newData;
        }

        /// <summary>Adds an empty slice to the <c>DataFrame</c></summary>
        /// <param name="legend">the new slice's legend</param>
        public void AddSlice(T legend)
        {
            TU[] newSlice = Enumerable.Range(0, _labels.Count).Select(u => default(TU)).ToArray();
            AddSlice(legend, newSlice);
        }

        #endregion

        #region Take

        /// <summary>Takes a <c>Slice</c> from the DataFrame thereby removing it from the DataFrame</summary>
        /// <param name="legend">the legend of the <c>Slice</c> to take</param>
        /// <returns> a <c>Slice</c></returns>
        public Slice<T, TU, TV> TakeSlice(T legend)
        {
            int indexToRemove = _legends[legend];
            if (indexToRemove == -1 || _legends.Count == 1) return null;

            TU[] takenData = new TU[_labels.Count];
            TU[][] newData = Arrays.Build<TU>(_legends.Count - 1, _labels.Count);

            for (int i = 0; i < _legends.Count; i++)
            {
                if (i == indexToRemove)
                    for (int j = 0; j < _labels.Count; j++)
                        takenData[j] = _data[i][j];
                else
                    for (int j = 0; j < _labels.Count; j++)
                        newData[i - (i < indexToRemove ? 0 : 1)][j] = _data[i][j];
            }
            _legends.Remove(legend);
            _data = newData;
            return Slice<T, TU, TV>.Create(_labels, legend, takenData);
        }

        #endregion

        #region Remove
        /// <summary>Removes the row for a given legend</summary>
        /// <param name="legend">the legend</param>
        public bool RemoveSliceAt(T legend)
        {
            int indexToRemove = _legends[legend];
            if (indexToRemove == -1 || _legends.Count == 1) return false;

            TU[][] newData = Arrays.Build<TU>(_legends.Count - 1, _labels.Count);
            for (int i = 0; i < _legends.Count; i++)
            {
                if (i == indexToRemove) continue;
                for (int j = 0; j < _labels.Count; j++)
                    newData[i - (i < indexToRemove ? 0 : 1)][j] = _data[i][j];
            }
            _legends.Remove(legend);
            _data = newData;
            return true;
        }
        #endregion

        #region Extract
        /// <summary>Extracts the part of the DataFrame whose legends obeys the predicate</summary>
        /// <param name="predicate">the predicate on the legends</param>
        /// <returns>a DataFrame</returns>
        public TY ExtractByLegend<TY>(Func<T, bool> predicate) where TY : IDataFrame<T, TU, TV>
        {
            return Create<TY>(_legends.Where(predicate).Select(l => GetSliceAt(l)));
        }
        #endregion

        #endregion

        /// <summary>Clones the <c>DataFrame</c></summary>
        /// <returns>a <c>DataFrame</c></returns>
        public TY Clone<TY>() where TY : IDataFrame<T, TU, TV>
        {
            return Create<TY>(_labels.Values, _legends.Values, Arrays.FastClone(_data));
        }

        #region Apply
        /// <summary>Applies a function to all the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<TU, TU> function)
        {
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    _data[i][j] = function(_data[i][j]);
        }

        /// <summary>Applies a function to all the legends</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLegends(Func<T, T> function)
        {
            for (int i = 0; i < _legends.Count; i++)
                _legends.Rename(_legends.ElementAt(i), function(_legends.ElementAt(i)));
        }

        /// <summary>Applies a function to all the labels</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLabels(Func<TV, TV> function)
        {
            for (int i = 0; i < _labels.Count; i++)
                _labels.Rename(_labels.ElementAt(i), function(_labels.ElementAt(i)));
        }
        #endregion

        #region Access the legends and labels

        /// <summary>Gets the i-th legend value</summary>
        /// <param name="index">the index</param>
        /// <returns>a legend value</returns>
        public T GetLegend(int index) { return _legends.ElementAt(index); }

        /// <summary>Renames a legend</summary>
        /// <param name="oldValue">the old legend value</param>
        /// <param name="newValue">the new legend value</param>
        public void RenameLegend(T oldValue, T newValue) { _legends.Rename(oldValue, newValue); }

        /// <summary>Gets the i-th label's value</summary>
        /// <param name="index">the index</param>
        /// <returns>a label</returns>
        public TV GetLabel(int index) { return _labels.ElementAt(index); }

        /// <summary>Renames a label</summary>
        /// <param name="oldValue">the old value</param>
        /// <param name="newValue">the new value</param>
        public void RenameLabel(TV oldValue, TV newValue) { _labels.Rename(oldValue, newValue); }
        #endregion

        #region IXmlable
        /// <summary>Serializes the <c>DataFrame</c> to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        public void ToXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartElement("dataFrame");

            #region Labels
            for (int j = 0; j < _labels.Count; j++)
            {
                writer.WriteStartElement("label");
                writer.WriteAttributeString("value", _labels.ElementAt(j).ToString());
                writer.WriteAttributeString("index", j.ToString());
                writer.WriteEndElement();
            }
            #endregion

            #region Legends
            for (int i = 0; i < _legends.Count; i++)
            {
                writer.WriteStartElement("legend");
                writer.WriteAttributeString("value", _legends.ElementAt(i).ToString());
                writer.WriteAttributeString("index", i.ToString());
                writer.WriteEndElement();
            }
            #endregion

            #region Data
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                {
                    writer.WriteStartElement("point");
                    writer.WriteAttributeString("row", i.ToString());
                    writer.WriteAttributeString("col", j.ToString());
                    writer.WriteAttributeString("value", _data[i][j].ToString());
                    writer.WriteEndElement();
                }
            #endregion

            writer.WriteEndElement();
        }

        /// <summary>Buils a <c>DataFrame</c></summary>
        /// <param name="node">the Xml node</param>
        /// <returns>a DataFrame</returns>
        public static TY FromXml<TY>(XmlNode node) where TY : IDataFrame<T, TU, TV>, new()
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            #region Labels and legends
            XmlNodeList labelNodes = node.SelectNodes("label"),
                legendNodes = node.SelectNodes("legend");

            List<TV> labels = new List<TV>();
            List<T> legends = new List<T>();

            for (int j = 0; j < labelNodes.Count; j++)
            {
                TV label = labelNodes[j].Attributes["value"].Value.Parse<TV>();
                labels.Add(label);
            }

            for (int i = 0; i < legendNodes.Count; i++)
            {
                T legend = legendNodes[i].Attributes["value"].Value.Parse<T>();
                legends.Add(legend);
            }
            #endregion

            #region Values
            XmlNodeList pointNodes = node.SelectNodes("point");
            TU[][] data = Arrays.Build<TU>(legends.Count, labels.Count);

            foreach (XmlNode pointNode in pointNodes)
            {
                int i = int.Parse(pointNode.Attributes["row"].Value),
                    j = int.Parse(pointNode.Attributes["col"].Value);
                TU value = pointNode.Attributes["value"].Value.Parse<TU>();
                data[i][j] = value;
            }

            return Create<TY>(labels, legends, data);
            #endregion
        }
        #endregion

        #region ICSVable
        /// <summary>Builds a string representation of the content of the <c>DataFrame</c> (default use ',' as separator) </summary>
        /// <returns>a <c>String</c></returns>
        public string ToCSV()
        {

            string[] lines = new string[1 + _legends.Count];
            lines[0] = $"x{CsvHelper.Separator}{string.Join(CsvHelper.Separator.ToString(), _labels)}";

            for (int i = 0; i < _legends.Count; i++)
            {
                TU[] row = new TU[_labels.Count];
                for (int j = 0; j < _labels.Count; j++) row[j] = _data[i][j];
                lines[i + 1] = $"{_legends.ElementAt(i)}{CsvHelper.Separator}{string.Join(CsvHelper.Separator.ToString(), row)}";
            }
            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>Builds a string representation of the content of the <c>DataFrame</c> </summary>
        /// <param name="separator">CSV separator</param>
        /// <returns>a <c>String</c></returns>
        public string ToCSV(string separator)
        {
            string[] lines = new string[1 + _legends.Count];
            lines[0] = $"x{separator}{string.Join(separator, _labels)}";

            for (int i = 0; i < _legends.Count; i++)
            {
                TU[] row = new TU[_labels.Count];
                for (int j = 0; j < _labels.Count; j++) row[j] = _data[i][j];
                lines[i + 1] = $"{_legends.ElementAt(i)}{separator}{string.Join(separator, row)}";
            }
            return string.Join(Environment.NewLine, lines);
        }
        #endregion

        /// <summary>Equality comparer</summary>
        /// <param name="other">the other DataFrame</param>
        /// <returns>true if the data, legends and labels match, false otherwise</returns>
        public bool Equals(IDataFrame<T, TU, TV> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (other._labels.Count == _labels.Count && other._legends.Count == _legends.Count &&
                !other._labels.Except(_labels).Any() && !other._legends.Except(_legends).Any())
            {
                for (int i = 0; i < other.Rows; i++)
                {
                    T t = other._legends.ElementAt(i);
                    for (int j = 0; j < other.Columns; j++)
                    {
                        TV v = other._labels.ElementAt(j);
                        if (!other[t, v].Equals(this[t, v]))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}

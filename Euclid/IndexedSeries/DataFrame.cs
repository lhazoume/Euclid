using Euclid.Helpers;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries
{
    /// <summary>Class representing a DataFrame of synchronized data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="U">the data type</typeparam>
    /// <typeparam name="V">the label type</typeparam>
    public class DataFrame<T, U, V> : IIndexedSeries<T, U, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private Header<V> _labels;
        private Header<T> _legends;
        private U[,] _data;
        #endregion

        #region Constructors
        private DataFrame(IList<V> labels, IList<T> legends, U[,] data)
        {
            _data = Arrays.Clone(data);
            _labels = new Header<V>(labels);
            _legends = new Header<T>(legends);
        }

        /// <summary>Builds a <c>DataFrame</c> from its serialized form</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        public static DataFrame<T, U, V> Create(XmlNode node)
        {

            XmlNodeList labelNodes = node.SelectNodes("dataFrame/label"),
                legendNodes = node.SelectNodes("dataFrame/legend"),
                dataNodes = node.SelectNodes("dataFrame/point");

            #region Labels
            V[] labels = new V[labelNodes.Count];
            foreach (XmlNode label in labelNodes)
            {
                int index = int.Parse(label.Attributes["index"].Value);
                labels[index] = label.Attributes["value"].Value.Parse<V>();
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
            U[,] data = new U[legends.Length, labels.Length];
            foreach (XmlNode point in dataNodes)
            {
                int row = int.Parse(point.Attributes["row"].Value),
                    col = int.Parse(point.Attributes["col"].Value);
                U value = point.Attributes["value"].Value.Parse<U>();
                data[row, col] = value;
            }
            #endregion

            return new DataFrame<T, U, V>(labels, legends, data);
        }

        /// <summary>
        /// Builds a <c>DataFrame</c>
        /// </summary>
        /// <param name="labels">the labels</param>
        /// <param name="legends">the legends</param>
        /// <param name="data">the data</param>
        public static DataFrame<T, U, V> Create(IList<V> labels, IList<T> legends, U[,] data)
        {
            return new DataFrame<T, U, V>(labels, legends, data);
        }

        /// <summary> Builds a <c>DataFrame</c></summary>
        /// <param name="slices">the slices</param>
        /// <returns>a DataFrame</returns>
        public static DataFrame<T, U, V> Create(IEnumerable<Slice<T, U, V>> slices)
        {
            U[,] data = new U[slices.Count(), slices.ElementAt(0).Columns];
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                    data[i, j] = slices.ElementAt(i)[j];
            return new DataFrame<T, U, V>(slices.ElementAt(0).Labels, slices.Select(s => s.Legend).ToList(), data);
        }

        /// <summary>Builds a <c>DataFrame</c> </summary>
        /// <param name="series">the series</param>
        /// <returns>a DataFrame</returns>
        public static DataFrame<T, U, V> Create(IEnumerable<Series<T, U, V>> series)
        {
            U[,] data = new U[series.ElementAt(0).Rows, series.Count()];
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                    data[i, j] = series.ElementAt(j)[i];
            return new DataFrame<T, U, V>(series.Select(s => s.Label).ToList(), series.ElementAt(0).Legends, data);
        }

        /// <summary>Builds a <c>DataFrame</c> from a CSV string</summary>
        /// <param name="text">the <c>String</c> content</param>
        public static DataFrame<T, U, V> Create(string text)
        {
            string[] lines = text.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            char[] separator = CSVHelper.Separator.ToCharArray();

            #region Labels
            string[] labelStrings = lines[0].Split(separator, StringSplitOptions.RemoveEmptyEntries);
            labelStrings = labelStrings.SubArray(1, labelStrings.Length - 1);
            int cols = labelStrings.Length;

            V[] labels = new V[cols];
            for (int j = 0; j < cols; j++)
                labels[j] = labelStrings[j].Parse<V>();
            #endregion

            #region Legends and data
            int rows = lines.Length - 1;
            T[] legends = new T[rows];
            U[,] data = new U[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] lineSplit = lines[1 + i].Split(separator, StringSplitOptions.RemoveEmptyEntries);

                legends[i] = lineSplit[0].Parse<T>();

                for (int j = 0; j < cols; j++)
                    data[i, j] = lineSplit[1 + j].Parse<U>();
            }
            #endregion

            return new DataFrame<T, U, V>(labels, legends, data);
        }

        #endregion

        #region Accessors
        /// <summary>
        /// Gets the legends
        /// </summary>
        public T[] Legends
        {
            get { return _legends.Values; }
        }

        /// <summary>Gets the labels </summary>
        public V[] Labels
        {
            get { return _labels.Values; }
        }

        /// <summary>Gets the number of columns</summary>
        public int Columns
        {
            get { return _labels.Count; }
        }

        /// <summary>Returns the number of rows</summary>
        public int Rows
        {
            get { return _legends.Count; }
        }

        /// <summary>Gets the data</summary>
        public U[,] Data
        {
            get { return _data; }
        }

        /// <summary>Gets and sets the data for the i-th row and j-th column of the <c>DataFrame</c></summary>
        /// <param name="i">the row index</param>
        /// <param name="j">the column index</param>
        /// <returns>a data point</returns>
        public U this[int i, int j]
        {
            get { return _data[i, j]; }
            set { _data[i, j] = value; }
        }

        /// <summary>
        /// Gets and sets the data for a given legend and a given label
        /// </summary>
        /// <param name="t">the legend</param>
        /// <param name="v">the label</param>
        /// <returns>a data point</returns>
        public U this[T t, V v]
        {
            get { return _data[_legends[t], _labels[v]]; }
            set { _data[_legends[t], _labels[v]] = value; }
        }
        #endregion

        #region Methods

        #region Series

        #region Get
        /// <summary> Gets the data-point column of the given label</summary>
        /// <param name="label">the label</param>
        /// <returns> a <c>Series</c></returns>
        public Series<T, U, V> GetSeriesAt(V label)
        {
            int index = _labels[label];
            if (index == -1) throw new ArgumentException("Label [" + label.ToString() + "] was not found");
            U[] result = new U[_legends.Count];
            for (int i = 0; i < _legends.Count; i++)
                result[i] = _data[i, index];
            return Series<T, U, V>.Create(label, _legends, result);
        }

        /// <summary> Gets all the data as an array of <c>Series</c></summary>
        /// <returns>an array of <c>Series</c></returns>
        public Series<T, U, V>[] GetSeries()
        {
            Series<T, U, V>[] result = new Series<T, U, V>[_labels.Count];
            for (int j = 0; j < _labels.Count; j++)
            {
                U[] data = new U[_legends.Count];
                for (int i = 0; i < _legends.Count; i++)
                    data[i] = _data[i, j];

                result[j] = Series<T, U, V>.Create(_labels.ElementAt(j), _legends, data);
            }
            return result;
        }
        #endregion

        #region Add

        /// <summary> Adds a column to the <c>DataFrame</c></summary>
        /// <param name="label">the new column's label</param>
        /// <param name="column">the new column's data</param>
        public void AddSeries(V label, U[] column)
        {
            U[,] newData = new U[_legends.Count, _labels.Count + 1];
            for (int i = 0; i < _legends.Count; i++)
            {
                newData[i, _labels.Count] = column[i];
                for (int j = 0; j < _labels.Count; j++)
                    newData[i, j] = _data[i, j];
            }

            _labels.Add(label);
            _data = newData;
        }

        /// <summary>Adds an empty column to the <c>DataFrame</c></summary>
        /// <param name="label">the new column's label</param>
        public void AddSeries(V label)
        {
            U[] newColumn = Enumerable.Range(0, _legends.Count).Select(u => default(U)).ToArray();
            AddSeries(label, newColumn);
        }

        #endregion

        #region Take

        /// <summary>Takes a <c>Series</c> from the DataFrame thereby removing it from the DataFrame</summary>
        /// <param name="label">the label of the <c>Series</c> to take</param>
        /// <returns> a <c>Series</c></returns>
        public Series<T, U, V> TakeSeries(V label)
        {
            int indexToRemove = _labels[label];
            if (indexToRemove == -1 || _labels.Count == 1) return null;

            U[] takenData = new U[_legends.Count];
            U[,] newData = new U[_legends.Count, _data.Length - 1];
            for (int j = 0; j < _labels.Count; j++)
            {
                if (j == indexToRemove)
                    for (int i = 0; i < _legends.Count; i++)
                        takenData[i] = _data[i, j];
                else
                {
                    for (int i = 0; i < _legends.Count; i++)
                        newData[i, j - (j < indexToRemove ? 0 : 1)] = _data[i, j];
                }
            }
            _labels.Remove(label);
            _data = newData;

            return Series<T, U, V>.Create(label, _legends, takenData);
        }

        #endregion

        #region Remove

        /// <summary>Remove the data for a given label</summary>
        /// <param name="label">the label</param>
        public bool RemoveSeriesAt(V label)
        {
            int indexToRemove = _labels[label];
            if (indexToRemove == -1 || _labels.Count == 1) return false;
            U[,] newData = new U[_legends.Count, _data.Length - 1];
            V[] newLabels = new V[_labels.Count - 1];

            for (int j = 0; j < _labels.Count; j++)
            {
                if (j == indexToRemove) continue;
                for (int i = 0; i < _legends.Count; i++)
                    newData[i, j - (j < indexToRemove ? 0 : 1)] = _data[i, j];
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
        public DataFrame<T, U, V> ExtractByLabels(Func<V, bool> predicate)
        {
            return Create(_labels.Where(predicate).Select(l => GetSeriesAt(l)));
        }
        #endregion

        #endregion

        #region Slices

        #region Get

        /// <summary>Gets the data-point row of the given legend</summary>
        /// <param name="legend">the legend</param>
        /// <returns>a <c>Slice</c></returns>
        public Slice<T, U, V> GetSliceAt(T legend)
        {
            int index = _legends[legend];
            if (index == -1) throw new ArgumentException(string.Format("Legend [{0}] was not found", legend.ToString()));
            U[] result = new U[_labels.Count];
            for (int j = 0; j < _labels.Count; j++)
                result[j] = _data[index, j];
            return Slice<T, U, V>.Create(_labels, legend, result);
        }

        /// <summary>Gets all the data as an array of <c>Slice</c></summary>
        /// <returns>an array of <c>Slice</c></returns>
        public Slice<T, U, V>[] GetSlices()
        {
            Slice<T, U, V>[] result = new Slice<T, U, V>[_legends.Count];
            for (int i = 0; i < _legends.Count; i++)
            {
                U[] data = new U[_labels.Count];
                for (int j = 0; j < _labels.Count; j++)
                    data[j] = _data[i, j];

                result[i] = Slice<T, U, V>.Create(_labels, _legends.ElementAt(i), data);
            }
            return result;
        }

        #endregion

        #region Add

        /// <summary> Adds a slice to the <c>DataFrame</c></summary>
        /// <param name="legend">the new slice's legend</param>
        /// <param name="slice">the new slice's data</param>
        public void AddSlice(T legend, U[] slice)
        {
            U[,] newData = new U[_legends.Count + 1, _labels.Count];
            for (int j = 0; j < _labels.Count; j++)
            {
                newData[_legends.Count, j] = slice[j];
                for (int i = 0; i < _legends.Count; i++)
                    newData[i, j] = _data[i, j];
            }

            _legends.Add(legend);
            _data = newData;
        }

        /// <summary>Adds an empty slice to the <c>DataFrame</c></summary>
        /// <param name="legend">the new slice's legend</param>
        public void AddSlice(T legend)
        {
            U[] newSlice = Enumerable.Range(0, _labels.Count).Select(u => default(U)).ToArray();
            AddSlice(legend, newSlice);
        }

        #endregion

        #region Take

        /// <summary>Takes a <c>Slice</c> from the DataFrame thereby removing it from the DataFrame</summary>
        /// <param name="legend">the legend of the <c>Slice</c> to take</param>
        /// <returns> a <c>Slice</c></returns>
        public Slice<T, U, V> TakeSlice(T legend)
        {
            int indexToRemove = _legends[legend];
            if (indexToRemove == -1 || _legends.Count == 1) return null;

            U[] takenData = new U[_labels.Count];
            U[,] newData = new U[_legends.Count - 1, _labels.Count];

            for (int i = 0; i < _legends.Count; i++)
            {
                if (i == indexToRemove)
                    for (int j = 0; j < _labels.Count; j++)
                        takenData[j] = _data[i, j];
                else
                    for (int j = 0; j < _labels.Count; j++)
                        newData[i - (i < indexToRemove ? 0 : 1), j] = _data[i, j];
            }
            _legends.Remove(legend);
            _data = newData;
            return Slice<T, U, V>.Create(_labels, legend, takenData);
        }

        #endregion

        #region Remove
        /// <summary>Removes the row for a given legend</summary>
        /// <param name="legend">the legend</param>
        public bool RemoveSliceAt(T legend)
        {
            int indexToRemove = _legends[legend];
            if (indexToRemove == -1 || _legends.Count == 1) return false;

            U[,] newData = new U[_legends.Count - 1, _labels.Count];
            for (int i = 0; i < _legends.Count; i++)
            {
                if (i == indexToRemove) continue;
                for (int j = 0; j < _labels.Count; j++)
                    newData[i - (i < indexToRemove ? 0 : 1), j] = _data[i, j];
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
        public DataFrame<T, U, V> ExtractByLegend(Func<T, bool> predicate)
        {
            return Create(_legends.Where(predicate).Select(l => GetSliceAt(l)));
        }
        #endregion

        #endregion

        /// <summary>Clones the <c>DataFrame</c></summary>
        /// <returns>a <c>DataFrame</c></returns>
        public DataFrame<T, U, V> Clone()
        {
            return new DataFrame<T, U, V>(_labels.Values, _legends.Values, Arrays.Clone(_data));
        }

        #region Apply

        /// <summary>Applies a function to all the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<U, U> function)
        {
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    _data[i, j] = function(_data[i, j]);
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
        public void ApplyOnLabels(Func<V, V> function)
        {
            for (int i = 0; i < _labels.Count; i++)
                _labels.Rename(_labels.ElementAt(i), function(_labels.ElementAt(i)));
        }

        #endregion

        #region Access the legends and labels

        /// <summary>Gets the i-th legend value</summary>
        /// <param name="index">the index</param>
        /// <returns>a legend value</returns>
        public T GetLegend(int index)
        {
            return _legends.ElementAt(index);
        }

        /// <summary>Renames a legend</summary>
        /// <param name="oldValue">the old legend value</param>
        /// <param name="newValue">the new legend value</param>
        public void RenameLegend(T oldValue, T newValue)
        {
            _legends.Rename(oldValue, newValue);
        }

        /// <summary>Gets the i-th label's value</summary>
        /// <param name="index">the index</param>
        /// <returns>a label</returns>
        public V GetLabel(int index)
        {
            return _labels.ElementAt(index);
        }

        /// <summary>Renames a label</summary>
        /// <param name="oldValue">the old value</param>
        /// <param name="newValue">the new value</param>
        public void RenameLabel(V oldValue, V newValue)
        {
            _labels.Rename(oldValue, newValue);
        }

        #endregion

        #endregion

        #region IXmlable
        /// <summary>Serializes the <c>DataFrame</c> to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        public void ToXml(XmlWriter writer)
        {
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
                    writer.WriteAttributeString("value", _data[i, j].ToString());
                    writer.WriteEndElement();
                }
            #endregion

            writer.WriteEndElement();
        }
        #endregion

        #region ICSVable
        /// <summary>Builds a string representation of the content of the <c>DataFrame</c> </summary>
        /// <returns>a <c>String</c></returns>
        public string ToCSV()
        {

            string[] lines = new string[1 + _legends.Count];
            lines[0] = string.Format("x{0}{1}", CSVHelper.Separator, string.Join(CSVHelper.Separator.ToString(), _labels));

            for (int i = 0; i < _legends.Count; i++)
            {
                U[] row = new U[_labels.Count];
                for (int j = 0; j < _labels.Count; j++) row[j] = _data[i, j];
                lines[i + 1] = string.Format("{0}{1}{2}", _legends.ElementAt(i).ToString(), CSVHelper.Separator, string.Join(CSVHelper.Separator.ToString(), row));
            }
            return string.Join(Environment.NewLine, lines);
        }
        #endregion

        /// <summary>Equality comparer</summary>
        /// <param name="other">the other DataFrame</param>
        /// <returns>true if the data, legends and labels match, false otherwise</returns>
        public bool Equals(DataFrame<T, U, V> other)
        {
            if (other._labels.Count == _labels.Count && other._legends.Count == _legends.Count &&
                other._labels.Except(_labels).Count() == 0 && other._legends.Except(_legends).Count()==0)
            {
                for (int i = 0; i < other.Rows; i++)
                {
                    T t = other._legends.ElementAt(i);
                    for (int j = 0; j < other.Columns; j++)
                    {
                        V v = other._labels.ElementAt(j);
                        if (!other[t, v].Equals(this[t, v]))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}

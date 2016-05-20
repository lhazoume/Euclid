using Euclid.Helpers;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries
{
    /// <summary>Class representing a Series of data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="U">the data type</typeparam>
    /// <typeparam name="V">the label type</typeparam>
    public class Series<T, U, V> : IIndexedSeries<T, U, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>
    {
        #region Declarations
        private V _label;
        private U[] _data;
        private T[] _legends;
        #endregion

        /// <summary>Builds an empty <c>Series</c></summary>
        /// <param name="rows">the size of the <c>Series</c></param>
        public Series(int rows)
        {
            _label = default(V);
            _legends = new T[rows];
            _data = new U[rows];
        }

        /// <summary>Builds a <c>Series</c></summary>
        /// <param name="label">the label</param>
        /// <param name="legends">the legends</param>
        /// <param name="data">the data</param>
        public Series(V label, IEnumerable<T> legends, IEnumerable<U> data)
        {
            _data = data.ToArray();
            _label = label;
            _legends = legends.ToArray();
        }

        /// <summary>Builds a <c>Series</c> from its serialized form</summary>
        /// <param name="seriesNode">the <c>XmlNode</c></param>
        public Series(XmlNode seriesNode)
        {
            FromXml(seriesNode);
        }

        #region Accessors
        /// <summary>Returns the legends of the <c>Series</c></summary>
        public T[] Legends
        {
            get { return Arrays.Clone(_legends); }
        }

        /// <summary>Returns the labels of the <c>Series</c> (in this case, it is the only label)</summary>
        public V[] Labels
        {
            get { return new V[] { _label }; }
        }

        /// <summary>Returns the data of the <c>Series</c></summary>
        public U[] Data
        {
            get { return Arrays.Clone(_data); }
        }

        /// <summary>Gets and sets the label</summary>
        public V Label
        {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>Returns the number of columns of the <c>Series</c> (in this case, it is one)</summary>
        public int Columns
        {
            get { return 1; }
        }

        /// <summary>Returns the number of rows of the <c>Series</c></summary>
        public int Rows
        {
            get { return _legends.Length; }
        }
        #endregion

        #region Methods
        /// <summary>Clones the <c>Series</c></summary>
        /// <returns>a <c>Series</c></returns>
        public Series<T, U, V> Clone()
        {
            return new Series<T, U, V>(_label, Arrays.Clone(_legends), Arrays.Clone(_data));
        }

        /// <summary>Gets and sets the i-th data of the <c>Series</c></summary>
        /// <param name="index">the index</param>
        /// <returns>a data point</returns>
        public U this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        /// <summary>Gets and sets the data for a given legend</summary>
        /// <param name="t">the legend</param>
        /// <returns>a data point</returns>
        public U this[T t]
        {
            get
            {
                int index = Array.IndexOf<T>(_legends, t);
                if (index == -1) throw new ArgumentException("Legend [" + t.ToString() + "] was not found");
                return _data[index];
            }
            set
            {
                int index = Array.IndexOf<T>(_legends, t);
                if (index == -1) throw new ArgumentException("Legend [" + t.ToString() + "] was not found");
                _data[index] = value;
            }
        }

        /// <summary>Removes the row for a given legend</summary>
        /// <param name="t">the legend</param>
        public void RemoveRowAt(T t)
        {
            int indexToRemove = Array.IndexOf<T>(_legends, t);
            if (indexToRemove == -1 || _legends.Length == 1) return;
            U[] newData = new U[_legends.Length - 1];
            T[] newLegends = new T[_legends.Length - 1];
            for (int i = 0; i < _legends.Length; i++)
            {
                if (i == indexToRemove) continue;
                int k = i - (i < indexToRemove ? 0 : 1);
                newLegends[k] = _legends[i];
                newData[k] = _data[i];
            }
            _legends = newLegends;
            _data = newData;
        }

        /// <summary>Adds a line to the <c>Series</c></summary>
        /// <param name="legend">the new legend</param>
        /// <param name="value">the new data</param>
        public void Add(T legend, U value)
        {
            if (_legends.Contains(legend))
                throw new ArgumentException("The legend is already in the series");
            U[] newData = new U[_data.Length + 1];
            T[] newLegends = new T[_legends.Length + 1];
            for (int j = 0; j < _legends.Length; j++)
            {
                newLegends[j] = _legends[j];
                newData[j] = _data[j];
            }

            newLegends[_legends.Length] = legend;
            newData[_data.Length] = value;

            _legends = newLegends;
            _data = newData;
        }

        /// <summary>Removes all the data-points that fit a predicate</summary>
        /// <param name="predicate">the predicate</param>
        public void Remove(Func<T, U, bool> predicate)
        {
            #region Kept Indices
            List<int> keptIndices = new List<int>();
            for (int i = 0; i < _legends.Length; i++)
                if (!predicate(_legends[i], _data[i]))
                    keptIndices.Add(i);
            #endregion

            #region Extraction
            U[] newData = new U[keptIndices.Count];
            T[] newLegends = new T[keptIndices.Count];
            for (int i = 0; i < keptIndices.Count; i++)
            {
                newLegends[i] = _legends[keptIndices[i]];
                newData[i] = _data[keptIndices[i]];
            }
            _legends = newLegends;
            _data = newData;
            #endregion
        }

        /// <summary>Applies a function to all the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<U, U> function)
        {
            for (int i = 0; i < _legends.Length; i++)
                _data[i] = function(_data[i]);
        }

        /// <summary>Applies a function to all the legends</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLegends(Func<T, T> function)
        {
            for (int i = 0; i < _legends.Length; i++)
                _legends[i] = function(_legends[i]);
        }

        /// <summary>Returns the sum of the data passed through a function</summary>
        /// <param name="function">the function</param>
        /// <returns>a scalar</returns>
        public U Sum(Func<U, U> function)
        {
            dynamic sum = default(U);

            for (int i = 0; i < _legends.Length; i++)
                sum += function(_data[i]);
            return (U)sum;
        }

        /// <summary>Gets the i-th legend value</summary>
        /// <param name="index">the index</param>
        /// <returns>a legend value</returns>
        public T GetLegend(int index)
        {
            return _legends[index];
        }

        /// <summary>Sets the i-th legend value </summary>
        /// <param name="index">the index</param>
        /// <param name="value">the new legend value</param>
        public void SetLegend(int index, T value)
        {
            _legends[index] = value;
        }
        #endregion

        #region IXmlable
        /// <summary>Serializes the <c>Series</c> to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("series");

            writer.WriteStartElement("label");
            writer.WriteAttributeString("value", _label.ToString());
            writer.WriteEndElement();

            for (int i = 0; i < _legends.Length; i++)
            {
                writer.WriteStartElement("point");
                writer.WriteAttributeString("legend", _legends[i].ToString());
                writer.WriteAttributeString("value", _data[i].ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>De-serializes the <c>Series</c> from a Xml node</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        public void FromXml(XmlNode node)
        {
            _label = node.SelectSingleNode("label").Attributes["value"].Value.Parse<V>();
            List<T> legends = new List<T>();
            List<U> data = new List<U>();

            XmlNodeList points = node.SelectNodes("point");
            foreach (XmlNode pointNode in points)
            {
                U value = pointNode.Attributes["value"].Value.Parse<U>();
                T legend = pointNode.Attributes["legend"].Value.Parse<T>();
                legends.Add(legend);
                data.Add(value);
            }

            _legends = legends.ToArray();
            _data = data.ToArray();
        }
        #endregion

        #region ICSVable
        /// <summary>Builds a string representing the content of the <c>Series</c></summary>
        /// <returns>a <c>String</c></returns>
        public string ToCSV()
        {
            string[] lines = new string[1 + this.Rows];
            lines[0] = string.Join(CSVHelper.Separator, "x", _label.ToString());
            for (int i = 0; i < this.Rows; i++)
                lines[1 + i] = string.Join(CSVHelper.Separator, _legends[i].ToString(), _data[i].ToString());
            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>Fills a <c>Series</c> from a string</summary>
        /// <param name="text">the <c>String</c> content</param>
        public void FromCSV(string text)
        {
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;
            _data = new U[lines.Length - 1];
            _legends = new T[lines.Length - 1];

            for (int i = 0; i < lines.Length; i++)
            {
                string[] content = lines[i].Split(CSVHelper.Separator.ToCharArray());
                if (content.Length == 2)
                    if (i == 0)
                        _label = content[i].Parse<V>();
                    else
                    {
                        _data[i - 1] = content[1].Parse<U>();
                        _legends[i - 1] = content[0].Parse<T>();
                    }
            }
        }
        #endregion

        #region Operators
        /// <summary>Adds two <c>Series</c></summary>
        /// <param name="ts1">the left hand side <c>Series</c></param>
        /// <param name="ts2">the right hand side <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator +(Series<T, U, V> ts1, Series<T, U, V> ts2)
        {
            if (ts1.Rows != ts2.Rows) throw new Exception("Series length do not match");
            for (int i = 0; i < ts1.Rows; i++)
                if (!ts1._legends[i].Equals(ts2._legends[i]))
                    throw new Exception("Series values do not match at #" + i + " : " + ts1._label + "=" + ts1._legends[i] + " while " + ts2._label + "=" + ts2._legends[i]);

            U[] data = new U[ts1._data.Length];
            for (int i = 0; i < ts1._data.Length; i++)
                data[i] = (dynamic)ts1._data[i] + (dynamic)ts2._data[i];
            return new Series<T, U, V>(default(V), ts1._legends, data);
        }

        /// <summary>Substracts one <c>Series</c> to another</summary>
        /// <param name="ts1">the left hand side <c>Series</c></param>
        /// <param name="ts2">the right hand side <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator -(Series<T, U, V> ts1, Series<T, U, V> ts2)
        {
            if (ts1.Rows != ts2.Rows) throw new Exception("Series length do not match");
            for (int i = 0; i < ts1.Rows; i++)
                if (!ts1._legends[i].Equals(ts2._legends[i]))
                    throw new Exception("Series values do not match at #" + i + " : " + ts1._label + "=" + ts1._legends[i] + " while " + ts2._label + "=" + ts2._legends[i]);

            U[] data = new U[ts1._data.Length];
            for (int i = 0; i < ts1._data.Length; i++)
                data[i] = (dynamic)ts1._data[i] - ts2._data[i];
            return new Series<T, U, V>(default(V), ts1._legends, data);
        }

        /// <summary>Multiplies the <c>Series</c> by a factor</summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="factor">the factor </param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator *(Series<T, U, V> ts, U factor)
        {
            U[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] * factor;
            return new Series<T, U, V>(ts._label, ts._legends, data);
        }

        /// <summary>Multiplies the <c>Series</c> by a factor</summary>
        /// <param name="factor">the factor</param>
        /// <param name="ts">the <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator *(U factor, Series<T, U, V> ts)
        {
            return ts * factor;
        }

        /// <summary>Divides the <c>Series</c> by a factor</summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="factor">the factor</param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator /(Series<T, U, V> ts, U factor)
        {
            U[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] / factor;
            return new Series<T, U, V>(ts._label, ts._legends, data);
        }

        /// <summary>Adds a scalar to a <c>Series</c></summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="amount">the number</param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator +(Series<T, U, V> ts, U amount)
        {
            U[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] + amount;
            return new Series<T, U, V>(ts._label, ts._legends, data);
        }

        /// <summary>Adds a scalar to a <c>Series</c></summary>
        /// <param name="amount">the scalar</param>
        /// <param name="ts">the <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator +(U amount, Series<T, U, V> ts)
        {
            return ts + amount;
        }

        /// <summary>Substracts a scalar to a <c>Series</c></summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="amount">the scalar</param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, U, V> operator -(Series<T, U, V> ts, U amount)
        {
            U[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] - amount;
            return new Series<T, U, V>(ts._label, ts._legends, data);
        }
        #endregion
    }
}

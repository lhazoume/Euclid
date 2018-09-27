using Euclid.Extensions;
using Euclid.Helpers;
using Euclid.Objects;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries
{
    /// <summary>Class representing a Slice of synchronized data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="U">the data type</typeparam>
    /// <typeparam name="V">the label type</typeparam>
    public class Slice<T, U, V> : IIndexedSeries<T, U, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private Header<V> _labels;
        private U[] _data;
        private T _legend;
        #endregion

        #region Constructors
        private Slice(Header<V> labels, T legend, IEnumerable<U> data)
        {
            _data = data.ToArray();
            _labels = labels.Clone();
            _legend = legend;
        }
        #endregion

        #region Accessors
        /// <summary>Gets the legends. Inherited (in this case, the legend is packaged into an array)</summary>
        public T[] Legends
        {
            get { return new T[] { _legend }; }
        }

        /// <summary>Gets and sets the legend</summary>
        public T Legend
        {
            get { return _legend; }
            set { _legend = value; }
        }

        /// <summary>Returns the labels</summary>
        public V[] Labels
        {
            get { return _labels.Values; }
        }

        /// <summary>Returns the number of columns</summary>
        public int Columns
        {
            get { return _labels.Count; }
        }

        /// <summary>Returns the number of rows</summary>
        public int Rows
        {
            get { return 1; }
        }

        /// <summary> Gets a deep copy of the data</summary>
        public U[] Data { get { return Arrays.Clone<U>(_data); } }
        #endregion

        #region Methods
        /// <summary>Clones the slice</summary>
        /// <returns>a <c>Slice</c></returns>
        public Slice<T, U, V> Clone()
        {
            return new Slice<T, U, V>(_labels, _legend, _data);
        }

        /// <summary>Remove the data for a given label</summary>
        /// <param name="label">the label</param>
        public void RemoveColumnAt(V label)
        {
            int indexToRemove = _labels[label];
            if (indexToRemove == -1 || _labels.Count == 1) return;
            U[] newData = new U[_data.Length - 1];

            for (int j = 0; j < _data.Length; j++)
            {
                if (j == indexToRemove) continue;
                newData[j - (j < indexToRemove ? 0 : 1)] = _data[j];
            }

            _labels.Remove(label);
            _data = newData;
        }

        /// <summary>Gets and sets the i-th data </summary>
        /// <param name="i">the index</param>
        /// <returns>a data point</returns>
        public U this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        /// <summary>Gets and sets the data for a given label</summary>
        /// <param name="v">the target label</param>
        /// <returns>a data point</returns>
        public U this[V v]
        {
            get { return _data[_labels[v]]; }
            set { _data[_labels[v]] = value; }
        }

        /// <summary>Adds a data to the slice</summary>
        /// <param name="label">the new label</param>
        /// <param name="value">the new value</param>
        public void Add(V label, U value)
        {
            if (_labels.Contains(label)) throw new ArgumentException("The label is already in the slice");

            U[] newData = new U[_data.Length + 1];
            _labels.Add(label);
            for (int j = 0; j < _labels.Count; j++)
                newData[j] = _data[j];
            newData[_data.Length] = value;

            _data = newData;
        }

        /// <summary>Applies a function to the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<U, U> function)
        {
            for (int j = 0; j < _data.Length; j++)
                _data[j] = function(_data[j]);
        }

        /// <summary>Gets the i-th label's value</summary>
        /// <param name="i">the index</param>
        /// <returns>a label</returns>
        public V GetLabel(int i)
        {
            return _labels.ElementAt(i);
        }

        /// <summary>Sets the i-th label's value</summary>
        /// <param name="oldValue">the old value</param>
        /// <param name="newValue">the new value</param>
        public void RenameLabel(V oldValue, V newValue)
        {
            _labels.Rename(oldValue, newValue);
        }
        #endregion

        #region IXmlable
        /// <summary>Serializes the slice to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("slice");

            #region Legend
            writer.WriteStartElement("legend");
            writer.WriteAttributeString("value", _legend.ToString());
            writer.WriteEndElement();
            #endregion

            #region Data
            foreach (V v in _labels)
            {
                writer.WriteStartElement("point");
                writer.WriteAttributeString("label", v.ToString());
                writer.WriteAttributeString("value", _data[_labels[v]].ToString());
                writer.WriteEndElement();
            }
            #endregion

            writer.WriteEndElement();
        }
        #endregion

        #region ICSVable
        /// <summary>Builds a string representation the content of the slice </summary>
        /// <returns>a <c>String</c></returns>
        public string ToCSV()
        {
            string[] lines = new string[2];
            lines[0] = "x" + CSVHelper.Separator + string.Join(CSVHelper.Separator.ToString(), _labels);
            lines[1] = _legend.ToString() + CSVHelper.Separator + string.Join(CSVHelper.Separator.ToString(), _data);
            return string.Join(Environment.NewLine, lines);
        }
        #endregion

        #region Creators
        /// <summary>De-serializes the slice from a Xml node</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        public static Slice<T, U, V> Create(XmlNode node)
        {
            XmlNodeList dataNodes = node.SelectNodes("point");
            XmlNode legendNode = node.SelectSingleNode("legend");

            T legend = legendNode.Attributes["value"].Value.Parse<T>();

            #region Data
            U[] data = new U[dataNodes.Count];
            Header<V> labels = new Header<V>();
            for (int i = 0; i < dataNodes.Count; i++)
            {
                V label = dataNodes[i].Attributes["label"].Value.Parse<V>();
                U value = dataNodes[i].Attributes["value"].Value.Parse<U>();
                data[i] = value;
                labels.Add(label);
            }
            #endregion

            return new Slice<T, U, V>(labels, legend, data);
        }

        /// <summary>Builds a slice from generic enumerable labels and data</summary>
        /// <param name="labels">the labels</param>
        /// <param name="legend">the legend</param>
        /// <param name="data">the data</param>
        /// <returns>a <c>Slice</c></returns>
        public static Slice<T, U, V> Create(Header<V> labels, T legend, IEnumerable<U> data)
        {
            return new Slice<T, U, V>(labels, legend, data);
        }

        /// <summary>Builds a <c>Slice</c> from its CSV string</summary>
        /// <param name="text">the <c>String</c> content</param>
        public static Slice<T, U, V> Create(string text)
        {
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != 2) return null;
            string[] header = lines[0].Split(new string[] { CSVHelper.Separator }, StringSplitOptions.RemoveEmptyEntries),
                content = lines[1].Split(new string[] { CSVHelper.Separator }, StringSplitOptions.RemoveEmptyEntries);
            if ((header.Length != content.Length) || (header.Length <= 1)) return null;
            int count = header.Length - 1;

            U[] data = new U[count];
            Header<V> labels = new Header<V>();
            T legend = content[0].Parse<T>();

            for (int i = 0; i < count; i++)
            {
                labels.Add(header[1 + i].Parse<V>());
                data[i] = content[1 + i].Parse<U>();
            }

            return new Slice<T, U, V>(labels, legend, data);
        }
        #endregion
    }
}

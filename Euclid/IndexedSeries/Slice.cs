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
        private Map<V, int> _labels;
        private U[] _data;
        private T _legend;
        #endregion

        #region Constructors
        private Slice(Map<V, int> labels, T legend, IEnumerable<U> data)
        {
            _data = data.ToArray();
            _labels = labels.Clone;
        }

        /// <summary>Builds a <c>Slice</c></summary>
        /// <param name="labels">the labels</param>
        /// <param name="legend">the legend</param>
        /// <param name="data">the data</param>
        public Slice(IEnumerable<V> labels, T legend, IEnumerable<U> data)
        {
            _data = data.ToArray();
            _labels = new Map<V, int>(Enumerable.Range(0, labels.Count()).Select(i => new Tuple<V, int>(labels.ElementAt(i), i)));
            _legend = legend;
        }

        /// <summary>Builds a <c>Slice</c> from its serialized form</summary>
        /// <param name="dataFrameNode">the <c>XmlNode</c></param>
        public Slice(XmlNode dataFrameNode)
        {
            FromXml(dataFrameNode);
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
            get { return _labels.Lefts; }
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
            return new Slice<T, U, V>(_labels.Clone, _legend, Arrays.Clone(_data));
        }

        /// <summary>Remove the data for a given label</summary>
        /// <param name="label">the label</param>
        public void RemoveColumnAt(V label)
        {
            int indexToRemove = _labels.Forward(label);
            if (indexToRemove == -1 || _labels.Count == 1) return;
            U[] newData = new U[_data.Length - 1];
            //V[] newLabels = new V[_labels.Count - 1];
            List<Tuple<V, int>> tuples = new List<Tuple<V, int>>();
            for (int j = 0; j < _labels.Count; j++)
            {
                if (j == indexToRemove) continue;
                int k = j - (j < indexToRemove ? 0 : 1);
                tuples.Add(new Tuple<V, int>(_labels.Backward(j), k));
                //newLabels[k] = _labels[j];
                newData[k] = _data[j];
            }
            _labels = new Map<V, int>(tuples); // newLabels;
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
            get
            {
                return _data[_labels.Forward(v)];
                /*int j = Array.IndexOf<V>(_labels, v);
                if (j == -1) throw new ArgumentException(string.Format("Point [{0}] was not found", v.ToString()));
                return _data[j];*/
            }
            set
            {
                _data[_labels.Forward(v)] = value;
                /*int j = Array.IndexOf<V>(_labels, v);
                if (j == -1) throw new ArgumentException(string.Format("Point [{0}] was not found", v.ToString()));
                _data[j] = value;*/
            }
        }

        /// <summary>Adds a data to the slice</summary>
        /// <param name="label">the new label</param>
        /// <param name="value">the new value</param>
        public void Add(V label, U value)
        {
            if (_labels.ContainsKey(label)) throw new ArgumentException("The label is already in the slice");

            U[] newData = new U[_data.Length + 1];
            _labels.Add(label, _data.Length);
            //V[] newLabels = new V[_labels.Count + 1];
            for (int j = 0; j < _labels.Count; j++)
            {
                //newLabels[j] = _labels[j];
                newData[j] = _data[j];
            }

            //newLabels[_labels.Count] = label;
            newData[_data.Length] = value;

            //_labels = newLabels;
            _data = newData;
        }

        /// <summary>Applies a function to the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<U, U> function)
        {
            for (int j = 0; j < _labels.Count; j++)
                _data[j] = function(_data[j]);
        }

        /// <summary>Gets the i-th label's value</summary>
        /// <param name="i">the index</param>
        /// <returns>a label</returns>
        public V GetLabel(int i)
        {
            return _labels.Backward(i);
        }

        /// <summary>Sets the i-th label's value</summary>
        /// <param name="i">the index</param>
        /// <param name="value">the new value</param>
        public void SetLabel(int i, V value)
        {
            _labels.SetBackward(i, value);
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
            for (int j = 0; j < _labels.Count; j++)
            {
                writer.WriteStartElement("point");
                writer.WriteAttributeString("label", _labels.Backward(j).ToString());
                writer.WriteAttributeString("value", _data[j].ToString());
                writer.WriteEndElement();
            }
            #endregion

            writer.WriteEndElement();
        }

        /// <summary>De-serializes the slice from a Xml node</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        public void FromXml(XmlNode node)
        {
            XmlNodeList dataNodes = node.SelectNodes("point");
            XmlNode legendNode = node.SelectSingleNode("legend");

            _legend = legendNode.Attributes["value"].Value.Parse<T>();

            #region Data
            _data = new U[dataNodes.Count];
            _labels = new Map<V, int>();
            for (int i = 0; i < dataNodes.Count; i++)
            {
                V label = dataNodes[i].Attributes["label"].Value.Parse<V>();
                U value = dataNodes[i].Attributes["value"].Value.Parse<U>();
                _data[i] = value;
                _labels.Add(label, i);
            }
            #endregion

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

        /// <summary>Fills a <c>Slice</c> from a string</summary>
        /// <param name="text">the <c>String</c> content</param>
        public void FromCSV(string text)
        {
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length != 2) return;
            string[] header = lines[0].Split(new string[] { CSVHelper.Separator }, StringSplitOptions.RemoveEmptyEntries),
                data = lines[1].Split(new string[] { CSVHelper.Separator }, StringSplitOptions.RemoveEmptyEntries);
            if ((header.Length != data.Length) || (header.Length <= 1)) return;
            int count = header.Length - 1;

            _data = new U[count];
            _labels = new Map<V, int>();
            _legend = data[0].Parse<T>();

            for (int i = 0; i < count; i++)
            {
                _labels.Add(header[1 + i].Parse<V>(), i);
                _data[i] = data[1 + i].Parse<U>();
            }
        }
        #endregion

        //TODO : operators
    }
}

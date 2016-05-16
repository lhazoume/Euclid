using Euclid.Helpers;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries
{
    public class Slice<T, U, V> : IIndexedSeries<T, U, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private V[] _labels;
        private U[] _data;
        private T _legend;
        #endregion

        #region Constructors
        public Slice(int columns)
        {
            _data = new U[columns];
            _labels = new V[columns];
            _legend = default(T);
        }
        public Slice(IEnumerable<V> labels, T legend, U[] data)
        {
            _data = Arrays.Clone(data);
            _labels = labels.ToArray();
            _legend = legend;
        }
        public Slice(XmlNode dataFrameNode)
        {
            FromXml(dataFrameNode);
        }
        #endregion

        #region Accessors
        public T[] Legends
        {
            get { return new T[] { _legend }; }
        }
        public T Legend
        {
            get { return _legend; }
            set { _legend = value; }
        }
        public V[] Labels
        {
            get { return _labels; }
        }

        public int Columns
        {
            get { return _labels.Length; }
        }
        public int Rows
        {
            get { return 1; }
        }
        #endregion

        #region Methods

        public Slice<T, U, V> Clone()
        {
            return new Slice<T, U, V>(Arrays.Clone(_labels), _legend, Arrays.Clone(_data));
        }
        public void RemoveColumnAt(V label)
        {
            int indexToRemove = Array.IndexOf<V>(_labels, label);
            if (indexToRemove == -1 || _labels.Length == 1) return;
            U[] newData = new U[_data.Length - 1];
            V[] newLabels = new V[_labels.Length - 1];
            for (int j = 0; j < _labels.Length; j++)
            {
                if (j == indexToRemove) continue;
                int k = j - (j < indexToRemove ? 0 : 1);
                newLabels[k] = _labels[j];
                newData[k] = _data[j];
            }
            _labels = newLabels;
            _data = newData;
        }
        public U this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }
        public U this[V v]
        {
            get
            {
                int j = Array.IndexOf<V>(_labels, v);
                if (j == -1) throw new ArgumentException(string.Format("Point [{0}] was not found", v.ToString()));
                return _data[j];
            }
        }
        public void Add(V label, U value)
        {
            U[] newData = new U[_data.Length + 1];
            V[] newLabels = new V[_labels.Length + 1];
            for (int j = 0; j < _labels.Length; j++)
            {
                newLabels[j] = _labels[j];
                newData[j] = _data[j];
            }

            newLabels[_labels.Length] = label;
            newData[_labels.Length] = value;

            _labels = newLabels;
            _data = newData;
        }
        public void ApplyOnData(Func<U, U> function)
        {
            for (int j = 0; j < _labels.Length; j++)
                _data[j] = function(_data[j]);
        }

        public V GetLabel(int index)
        {
            return _labels[index];
        }
        public void SetLabel(int index, V value)
        {
            _labels[index] = value;
        }
        #endregion

        #region IXmlable
        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("slice");

            #region Labels
            for (int j = 0; j < _labels.Length; j++)
            {
                writer.WriteStartElement("label");
                writer.WriteAttributeString("value", _labels[j].ToString());
                writer.WriteAttributeString("index", j.ToString());
                writer.WriteEndElement();
            }
            #endregion

            #region Legend
            writer.WriteStartElement("legend");
            writer.WriteAttributeString("value", _legend.ToString());
            writer.WriteEndElement();
            #endregion

            #region Data
            for (int j = 0; j < _labels.Length; j++)
            {
                writer.WriteStartElement("point");
                writer.WriteAttributeString("col", j.ToString());
                writer.WriteAttributeString("value", _data[j].ToString());
                writer.WriteEndElement();
            }
            #endregion

            writer.WriteEndElement();
        }
        public void FromXml(XmlNode node)
        {
            {
                XmlNodeList labelNodes = node.SelectNodes("label"),
                    dataNodes = node.SelectNodes("point");
                XmlNode legendNode = node.SelectSingleNode("legend");

                #region Labels
                _labels = new V[labelNodes.Count];
                foreach (XmlNode label in labelNodes)
                {
                    int index = int.Parse(label.Attributes["index"].Value);
                    _labels[index] = label.Attributes["value"].Value.Parse<V>();
                }
                #endregion

                #region Legend
                _legend = legendNode.Attributes["value"].Value.Parse<T>();
                #endregion

                #region Data
                _data = new U[_labels.Length];
                foreach (XmlNode point in dataNodes)
                {
                    int col = int.Parse(point.Attributes["col"].Value);
                    U value = point.Attributes["value"].Value.Parse<U>();
                    _data[col] = value;
                }
                #endregion
            }
        }
        #endregion

        #region ICSVable
        public string ToCSV()
        {
            string[] lines = new string[2];
            lines[0] = "x" + CSVHelper.Separator + string.Join(CSVHelper.Separator.ToString(), _labels);
            lines[1] = _legend.ToString() + CSVHelper.Separator + string.Join(CSVHelper.Separator.ToString(), _data);
            return string.Join(Environment.NewLine, lines);
        }
        public void FromCSV(string text)
        {
            //TODO
            throw new NotImplementedException();
        }
        #endregion
    }
}

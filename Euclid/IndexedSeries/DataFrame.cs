using Euclid.Helpers;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries
{
    public class DataFrame<T, U, V> : IIndexedSeries<T, U, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private V[] _labels;
        private U[,] _data;
        private T[] _legends;
        #endregion

        #region Constructors
        public DataFrame(int rows, int columns)
        {
            _data = new U[rows, columns];
            _labels = new V[columns];
            _legends = new T[rows];
        }
        public DataFrame(IEnumerable<V> labels, IEnumerable<T> legends, U[,] data)
        {
            _data = Arrays.Clone(data);
            _labels = labels.ToArray();
            _legends = legends.ToArray();
        }
        public DataFrame(XmlNode dataFrameNode)
        {
            FromXml(dataFrameNode);
        }
        #endregion

        #region Accessors
        public T[] Legends
        {
            get { return _legends; }
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
            get { return _legends.Length; }
        }
        #endregion

        #region Methods

        public DataFrame<T, U, V> Clone()
        {
            return new DataFrame<T, U, V>(Arrays.Clone(_labels), Arrays.Clone(_legends), Arrays.Clone(_data));
        }
        public void RemoveColumnAt(V label)
        {
            int indexToRemove = Array.IndexOf<V>(_labels, label);
            if (indexToRemove == -1 || _labels.Length == 1) return;
            U[,] newData = new U[_legends.Length, _data.Length - 1];
            V[] newLabels = new V[_labels.Length - 1];
            for (int j = 0; j < _labels.Length; j++)
            {
                if (j == indexToRemove) continue;
                int k = j - (j < indexToRemove ? 0 : 1);
                newLabels[k] = _labels[j];

                for (int i = 0; i < _legends.Length; i++)
                    newData[i, k] = _data[i, j];
            }
            _labels = newLabels;
            _data = newData;
        }
        public void RemoveRowAt(T t)
        {
            int indexToRemove = Array.IndexOf<T>(_legends, t);
            if (indexToRemove == -1 || _legends.Length == 1) return;
            U[,] newData = new U[_legends.Length - 1, _labels.Length];
            T[] newLegends = new T[_legends.Length - 1];
            for (int i = 0; i < _legends.Length; i++)
            {
                if (i == indexToRemove) continue;
                int k = i - (i < indexToRemove ? 0 : 1);
                newLegends[k] = _legends[i];

                for (int j = 0; j < _labels.Length; j++)
                    newData[k, j] = _data[i, j];
            }
            _legends = newLegends;
            _data = newData;
        }
        public void Remove(Func<T, U, V, bool> predicate)
        {
            #region Kept Indices
            List<int> keptIndices = new List<int>();
            for (int i = 0; i < _legends.Length; i++)
            {
                bool lineShouldBeRemoved = false;
                for (int j = 0; j < _labels.Length; j++)
                    if (predicate(_legends[i], _data[i, j], _labels[j]))
                    {
                        lineShouldBeRemoved = true;
                        break;
                    }
                if (!lineShouldBeRemoved)
                    keptIndices.Add(i);
            }
            #endregion

            #region Extraction
            U[,] newData = new U[keptIndices.Count, _labels.Length];
            T[] newLegends = new T[keptIndices.Count];
            for (int i = 0; i < keptIndices.Count; i++)
            {
                newLegends[i] = _legends[keptIndices[i]];

                for (int j = 0; j < _labels.Length; j++)
                    newData[i, j] = _data[keptIndices[i], j];
            }
            _legends = newLegends;
            _data = newData;
            #endregion
        }
        public U this[int i, int j]
        {
            get { return _data[i, j]; }
        }
        public U this[T t, V v]
        {
            get
            {
                int i = Array.IndexOf<T>(_legends, t),
                    j = Array.IndexOf<V>(_labels, v);
                if (i == -1 || j == -1) throw new ArgumentException(string.Format("Point [{0}, {1}] was not found", t.ToString(), v.ToString()));
                return _data[i, j];
            }
        }
        public U[] GetRowAt(T t)
        {
            int index = Array.IndexOf<T>(_legends, t);
            if (index == -1) throw new ArgumentException(string.Format("Legend [{0}] was not found", t.ToString()));
            U[] result = new U[_labels.Length];
            for (int j = 0; j < _labels.Length; j++)
                result[j] = _data[index, j];
            return result;
        }
        public U[] GetColumnAt(V v)
        {
            int index = Array.IndexOf<V>(_labels, v);
            if (index == -1) throw new ArgumentException("Label [" + v.ToString() + "] was not found");
            U[] result = new U[_legends.Length];
            for (int i = 0; i < _legends.Length; i++)
                result[i] = _data[i, index];
            return result;
        }

        public U[][] GetRows()
        {
            U[][] result = new U[_legends.Length][];
            for (int i = 0; i < _legends.Length; i++)
            {
                result[i] = new U[_labels.Length];
                for (int j = 0; j < _labels.Length; j++)
                    result[i][j] = _data[i, j];
            }
            return result;
        }
        public void Add(V label, U[] column)
        {
            U[,] newData = new U[_legends.Length, _data.Length + 1];
            V[] newLabels = new V[_labels.Length + 1];
            for (int j = 0; j < _labels.Length; j++)
            {
                newLabels[j] = _labels[j];
                for (int i = 0; i < _legends.Length; i++)
                    newData[i, j] = _data[i, j];
            }

            newLabels[_labels.Length] = label;
            for (int i = 0; i < _legends.Length; i++)
                newData[i, _labels.Length] = column[i];

            _labels = newLabels;
            _data = newData;
        }
        public void ApplyOnData(Func<U, U> function)
        {
            for (int i = 0; i < _legends.Length; i++)
                for (int j = 0; j < _labels.Length; j++)
                    _data[i, j] = function(_data[i, j]);
        }
        public void ApplyOnLegends(Func<T, T> function)
        {
            for (int i = 0; i < _legends.Length; i++)
                _legends[i] = function(_legends[i]);
        }

        public T GetLegend(int index)
        {
            return _legends[index];
        }
        public void SetLegend(int index, T value)
        {
            _legends[index] = value;
        }

        public V GetLabel(int index)
        {
            return _labels[index];
        }
        public void SetLabel(int index, V value)
        {
            _labels[index] = value;
        }

        public Series<T,U,V> GetSeries(V label)
        public Series<T, U, V> GetSeries(V label)
        {
            U[] data = this.GetColumnAt(label);
            return new Series<T, U, V>(label, _legends, data);
        }
        #endregion

        #region IXmlable
        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("dataFrame");

            #region Labels
            for (int j = 0; j < _labels.Length; j++)
            {
                writer.WriteStartElement("label");
                writer.WriteAttributeString("value", _labels[j].ToString());
                writer.WriteAttributeString("index", j.ToString());
                writer.WriteEndElement();
            }
            #endregion

            #region Legends
            for (int i = 0; i < _legends.Length; i++)
            {
                writer.WriteStartElement("legend");
                writer.WriteAttributeString("value", _legends[i].ToString());
                writer.WriteAttributeString("index", i.ToString());
                writer.WriteEndElement();
            }
            #endregion

            #region Data
            for (int i = 0; i < _legends.Length; i++)
                for (int j = 0; j < _labels.Length; j++)
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
        public void FromXml(XmlNode node)
        {
            {
                XmlNodeList labelNodes = node.SelectNodes("timeDataFrame/label"),
                    legendNodes = node.SelectNodes("timeDataFrame/legend"),
                    dataNodes = node.SelectNodes("timeDataFrame/point");

                #region Labels
                _labels = new V[labelNodes.Count];
                foreach (XmlNode label in labelNodes)
                {
                    int index = int.Parse(label.Attributes["index"].Value);
                    _labels[index] = label.Attributes["value"].Value.Parse<V>();
                }
                #endregion

                #region Legends
                _legends = new T[legendNodes.Count];
                foreach (XmlNode legend in legendNodes)
                {
                    int index = int.Parse(legend.Attributes["index"].Value);
                    _legends[index] = legend.Attributes["value"].Value.Parse<T>();
                }
                #endregion

                #region Data
                _data = new U[_legends.Length, _labels.Length];
                foreach (XmlNode point in dataNodes)
                {
                    int row = int.Parse(point.Attributes["row"].Value),
                        col = int.Parse(point.Attributes["col"].Value);
                    U value = point.Attributes["value"].Value.Parse<U>();
                    _data[row, col] = value;
                }
                #endregion
            }
        }
        #endregion

        #region ICSVable
        public string ToCSV()
        {
            U[][] rows = this.GetRows();

            string[] lines = new string[1 + _legends.Length];
            lines[0] = "x" + CSVHelper.Separator + string.Join(CSVHelper.Separator.ToString(), _labels);
            for (int i = 0; i < _legends.Length; i++)
                lines[i + 1] = _legends[i].ToString() + CSVHelper.Separator + string.Join(CSVHelper.Separator.ToString(), rows[i]);
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

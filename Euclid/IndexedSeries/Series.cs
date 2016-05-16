using Euclid.Helpers;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries
{
    public class Series<T, U, V> : IIndexedSeries<T, U, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>
    {
        #region Declarations
        private V _label;
        private U[] _data;
        private T[] _legends;
        #endregion

        public Series(int rows)
        {
            _label = default(V);
            _legends = new T[rows];
            _data = new U[rows];
        }
        public Series(V label, IEnumerable<T> legends, IEnumerable<U> data)
        {
            _data = data.ToArray();
            _label = label;
            _legends = legends.ToArray();
        }
        public Series(XmlNode seriesNode)
        {
            FromXml(seriesNode);
        }

        #region Accessors
        public T[] Legends
        {
            get { return Arrays.Clone(_legends); }
        }
        public V[] Labels
        {
            get { return new V[] { _label }; }
        }
        public U[] Data
        {
            get { return Arrays.Clone(_data); }
        }
        public V Label
        {
            get { return _label; }
            set { _label = value; }
        }
        public int Columns
        {
            get { return 1; }
        }
        public int Rows
        {
            get { return _legends.Length; }
        }
        #endregion

        #region Methods
        public Series<T, U, V> Clone()
        {
            return new Series<T, U, V>(_label, Arrays.Clone(_legends), Arrays.Clone(_data));
        }
        public U GetValue(T t)
        {
            int index = Array.IndexOf<T>(_legends, t);
            if (index == -1) throw new ArgumentException("Legend [" + t.ToString() + "] was not found");
            return _data[index];
        }
        public U this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }
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
        public void ApplyOnData(Func<U, U> function)
        {
            for (int i = 0; i < _legends.Length; i++)
                _data[i] = function(_data[i]);
        }
        public void ApplyOnLegends(Func<T, T> function)
        {
            for (int i = 0; i < _legends.Length; i++)
                _legends[i] = function(_legends[i]);
        }
        public U Sum(Func<U, U> function)
        {
            dynamic sum = default(U);

            for (int i = 0; i < _legends.Length; i++)
                sum += function(_data[i]);
            return (U)sum;
        }
        public T GetLegend(int index)
        {
            return _legends[index];
        }
        public void SetLegend(int index, T value)
        {
            _legends[index] = value;
        }
        #endregion

        #region IXmlable
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
        public string ToCSV()
        {
            string[] lines = new string[1 + this.Rows];
            lines[0] = string.Join(CSVHelper.Separator, "x", _label.ToString());
            for (int i = 0; i < this.Rows; i++)
                lines[1 + i] = string.Join(CSVHelper.Separator, _legends[i].ToString(), _data[i].ToString());
            return string.Join(Environment.NewLine, lines);
        }
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
        public static Series<T, U, V> operator *(Series<T, U, V> ts, U factor)
        {
            U[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] * factor;
            return new Series<T, U, V>(ts._label, ts._legends, data);
        }
        public static Series<T, U, V> operator *(U factor, Series<T, U, V> ts)
        {
            return ts * factor;
        }
        public static Series<T, U, V> operator /(Series<T, U, V> ts, U factor)
        {
            U[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] / factor;
            return new Series<T, U, V>(ts._label, ts._legends, data);
        }
        public static Series<T, U, V> operator +(Series<T, U, V> ts, U amount)
        {
            U[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] + amount;
            return new Series<T, U, V>(ts._label, ts._legends, data);
        }
        public static Series<T, U, V> operator +(U amount, Series<T, U, V> ts)
        {
            return ts + amount;
        }
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

using Euclid.Extensions;
using Euclid.Helpers;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>Class representing a Series of data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="TU">the data type</typeparam>
    /// <typeparam name="TV">the label type</typeparam>
    public class Series<T, TU, TV> : IIndexedSeries<T, TU, TV> where T : IComparable<T>, IEquatable<T> where TV : IEquatable<TV>
    {
        #region Declarations
        /// <summary>
        /// Label variable
        /// </summary>
        protected TV _label;
        /// <summary>
        /// Data variable
        /// </summary>
        protected TU[] _data;
        /// <summary>
        /// Legend variable
        /// </summary>
        protected IHeader<T> _legends;
        #endregion

        #region constructor
        /// <summary>
        /// Parameterless constructor
        /// </summary>
        protected Series() { }

        /// <summary>Builds a <c>Series</c></summary>
        /// <param name="label">the label</param>
        /// <param name="legends">the legends</param>
        /// <param name="data">the data</param>
        protected Series(TV label, IHeader<T> legends, TU[] data)
        {
            _data = Arrays.Clone(data);
            _label = label;
            _legends = legends.Clone();
        }
        #endregion

        #region Accessors
        /// <summary>Returns the legends of the <c>Series</c></summary>
        public virtual T[] Legends => _legends.Values;

        /// <summary>Returns the labels of the <c>Series</c> (in this case, it is the only label)</summary>
        public TV[] Labels => new TV[] { _label };

        /// <summary>Returns the data of the <c>Series</c></summary>
        public TU[] Data => Arrays.Clone(_data);

        /// <summary>Gets and sets the label</summary>
        public TV Label
        {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>Returns the number of columns of the <c>Series</c> (in this case, it is one)</summary>
        public int Columns => 1;

        /// <summary>Returns the number of rows of the <c>Series</c></summary>
        public int Rows => _data.Length;
        #endregion

        #region Methods
        /// <summary>Clones the <c>Series</c></summary>
        /// <returns>a <c>Series</c></returns>
        public TY Clone<TY>() where TY : Series<T, TU, TV>
        {
            return Create<TY>(_label, _legends.Values, _data);
        }

        /// <summary>Gets and sets the i-th data of the <c>Series</c></summary>
        /// <param name="index">the index</param>
        /// <returns>a data point</returns>
        public TU this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        /// <summary>Gets and sets the data for a given legend</summary>
        /// <param name="t">the legend</param>
        /// <returns>a data point</returns>
        public TU this[T t]
        {
            get { return _data[_legends[t]]; }
            set { _data[_legends[t]] = value; }
        }

        /// <summary>Removes the row for a given legend</summary>
        /// <param name="t">the legend</param>
        public void RemoveRowAt(T t)
        {
            int indexToRemove = _legends[t];
            if (indexToRemove == -1 || _legends.Count == 1) return;
            TU[] newData = new TU[_data.Length - 1];

            for (int i = 0; i < _data.Length; i++)
            {
                if (i == indexToRemove) continue;
                newData[i - (i < indexToRemove ? 0 : 1)] = _data[i];
            }

            _legends.Remove(t);
            _data = newData;
        }

        /// <summary>Adds a line to the <c>Series</c></summary>
        /// <param name="legend">the new legend</param>
        /// <param name="value">the new data</param>
        public void Add(T legend, TU value)
        {
            if (_legends.Contains(legend))
                throw new ArgumentException("The legend is already in the series");
            TU[] newData = new TU[_data.Length + 1];
            for (int j = 0; j < _data.Length; j++)
                newData[j] = _data[j];

            _legends.Add(legend);
            newData[_data.Length] = value;

            _data = newData;
        }

        /// <summary>Removes all the data-points that fit a predicate</summary>
        /// <param name="predicate">the predicate</param>
        public int Remove(Func<T, TU, bool> predicate)
        {
            int linesRemoved = 0,
                i = 0;
            while (i < _legends.Count)
            {
                if (predicate(_legends.ElementAt(i), _data[i]))
                {
                    RemoveRowAt(_legends.ElementAt(i));
                    linesRemoved++;
                }
                else
                    i++;
            }

            return linesRemoved;
        }

        /// <summary>Applies a function to all the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<TU, TU> function)
        {
            for (int i = 0; i < _data.Length; i++)
                _data[i] = function(_data[i]);
        }

        /// <summary>Applies a function to all the legends</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLegends(Func<T, T> function)
        {
            _legends = new Header<T>(_legends.Select(t => function(t)).ToList());
        }

        /// <summary>Returns the sum of the data passed through a function</summary>
        /// <param name="function">the function</param>
        /// <returns>a scalar</returns>
        public TU Sum(Func<TU, TU> function)
        {
            dynamic sum = default(TU);

            for (int i = 0; i < _data.Length; i++)
                sum += function(_data[i]);
            return (TU)sum;
        }

        /// <summary>Gets the i-th legend value</summary>
        /// <param name="index">the index</param>
        /// <returns>a legend value</returns>
        public T GetLegend(int index)
        {
            return _legends.ElementAt(index);
        }

        /// <summary>Sets the i-th legend value </summary>
        /// <param name="oldValue">the old legend</param>
        /// <param name="newValue">the new legend value</param>
        public void Rename(T oldValue, T newValue)
        {
            _legends.Rename(oldValue, newValue);
        }

        /// <summary>Equality comparer for the Series</summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Series<T, TU, TV> other)
        {
            if (other != null && other._label.Equals(_label) && other._legends.Count == _legends.Count &&
                !other._legends.Except(_legends).Any())
            {
                for (int i = 0; i < _data.Length; i++)
                    if (!other._data[i].Equals(_data[i]))
                        return false;
                return true;
            }
            return false;
        }
        #endregion

        #region IXmlable
        /// <summary>Serializes the <c>Series</c> to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        public void ToXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteStartElement("series");

            writer.WriteStartElement("label");
            writer.WriteAttributeString("value", _label.ToString());
            writer.WriteEndElement();

            foreach (T t in _legends)
            {
                writer.WriteStartElement("point");
                writer.WriteAttributeString("legend", t.ToString());
                writer.WriteAttributeString("value", _data[_legends[t]].ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        #endregion

        #region ICSVable
        /// <summary>Builds a string representing the content of the <c>Series</c></summary>
        /// <param name="separator">column separator, by default ';' is used</param>
        /// <returns>a <c>String</c></returns>
        public string ToCSV(string separator = ";")
        {
            List<string> lines = new List<string> { string.Join(separator, "x", _label.ToString()) };
            foreach (T t in _legends) lines.Add(string.Join(separator, t.ToString(), _data[_legends[t]].ToString()));
            return string.Join(Environment.NewLine, lines);
        }
        #endregion

        #region Operators
        /// <summary>Adds two <c>Series</c></summary>
        /// <param name="ts1">the left hand side <c>Series</c></param>
        /// <param name="ts2">the right hand side <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator +(Series<T, TU, TV> ts1, Series<T, TU, TV> ts2)
        {
            if (ts1 == null) throw new ArgumentNullException(nameof(ts1));
            if (ts2 == null) throw new ArgumentNullException(nameof(ts2));
            if (ts1.Rows != ts2.Rows) throw new Exception("Series length do not match");
            if (ts1._legends != ts2._legends) throw new DataMisalignedException("The series legends do not match");

            TU[] data = new TU[ts1._data.Length];
            for (int i = 0; i < ts1._data.Length; i++)
                data[i] = (dynamic)ts1._data[i] + (dynamic)ts2._data[i];
            return new Series<T, TU, TV>(default, ts1._legends, data);
        }

        /// <summary>Substracts one <c>Series</c> to another</summary>
        /// <param name="ts1">the left hand side <c>Series</c></param>
        /// <param name="ts2">the right hand side <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator -(Series<T, TU, TV> ts1, Series<T, TU, TV> ts2)
        {
            if (ts1 == null) throw new ArgumentNullException(nameof(ts1));
            if (ts2 == null) throw new ArgumentNullException(nameof(ts2));

            if (ts1.Rows != ts2.Rows) throw new Exception("Series length do not match");
            if (ts1._legends != ts2._legends) throw new DataMisalignedException("The series legends do not match");

            TU[] data = new TU[ts1._data.Length];
            for (int i = 0; i < ts1._data.Length; i++)
                data[i] = (dynamic)ts1._data[i] - ts2._data[i];
            return new Series<T, TU, TV>(default, ts1._legends, data);
        }

        /// <summary>Multiplies the <c>Series</c> by a factor</summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="factor">the factor </param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator *(Series<T, TU, TV> ts, TU factor)
        {
            if (ts == null) throw new ArgumentNullException(nameof(ts));

            TU[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] * factor;
            return new Series<T, TU, TV>(ts._label, ts._legends, data);
        }

        /// <summary>Multiplies the <c>Series</c> by a factor</summary>
        /// <param name="factor">the factor</param>
        /// <param name="ts">the <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator *(TU factor, Series<T, TU, TV> ts)
        {
            return ts * factor;
        }

        /// <summary>Divides the <c>Series</c> by a factor</summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="factor">the factor</param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator /(Series<T, TU, TV> ts, TU factor)
        {
            if (ts == null) throw new ArgumentNullException(nameof(ts));

            TU[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] / factor;
            return new Series<T, TU, TV>(ts._label, ts._legends, data);
        }

        /// <summary>Adds a scalar to a <c>Series</c></summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="amount">the number</param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator +(Series<T, TU, TV> ts, TU amount)
        {
            if (ts == null) throw new ArgumentNullException(nameof(ts));

            TU[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] + amount;
            return new Series<T, TU, TV>(ts._label, ts._legends, data);
        }

        /// <summary>Adds a scalar to a <c>Series</c></summary>
        /// <param name="amount">the scalar</param>
        /// <param name="ts">the <c>Series</c></param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator +(TU amount, Series<T, TU, TV> ts)
        {
            return ts + amount;
        }

        /// <summary>Substracts a scalar to a <c>Series</c></summary>
        /// <param name="ts">the <c>Series</c></param>
        /// <param name="amount">the scalar</param>
        /// <returns>a <c>Series</c></returns>
        public static Series<T, TU, TV> operator -(Series<T, TU, TV> ts, TU amount)
        {
            if (ts == null) throw new ArgumentNullException(nameof(ts));

            TU[] data = ts._data;
            for (int i = 0; i < ts._data.Length; i++)
                data[i] = (dynamic)ts._data[i] - amount;
            return new Series<T, TU, TV>(ts._label, ts._legends, data);
        }
        #endregion

        #region Creators
        /// <summary>
        /// Initialize serie instance
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="legends">Legends</param>
        /// <param name="data">Data</param>
        protected virtual void Initialize(TV label, IList<T> legends, TU[] data)
        {
            _data = Arrays.Clone(data);
            _label = label;
            _legends = new Header<T>(legends);
        }

        /// <summary>
        /// Initialize the data vector with the specified value.
        /// </summary>
        /// <param name="value">Value used for initialization</param>
        public void Initialize(TU value)
        {
            for (int i = 0; i < Rows; i++)
                    _data[i] = value;
        }

        /// <summary>
        /// Builds a <c>Series</c> from generic enumerables of legends and data
        /// </summary>
        /// <typeparam name="TY">Generic type</typeparam>
        /// <param name="label">label</param>
        /// <param name="legends">legends</param>
        /// <param name="data">data</param>
        /// <returns>Instance</returns>
        public static TY Create<TY>(TV label, IList<T> legends, TU[] data) where TY : Series<T, TU, TV>
        {
            ConstructorInfo constructor = typeof(TY).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(ct => ct.GetParameters().Length == 0);
            if (constructor == null) throw new Exception("Impossible to find the matching constructor (protected, without parameters)!");
            TY serie = (TY)constructor.Invoke(null);
            serie.Initialize(label, legends, data);
            return serie;
        }

        /// <summary>
        /// Create a <c>Series</c> from generic enumerables of legends and data
        /// </summary>
        /// <typeparam name="TY">Typed series</typeparam>
        /// <param name="label">Label</param>
        /// <param name="legends">Legends</param>
        /// <param name="data">Data</param>
        /// <returns>Series</returns>
        public static TY Create<TY>(TV label, IHeader<T> legends, TU[] data) where TY : Series<T, TU, TV>
        {
            return Create<TY>(label, legends.Values, data);
        }

        /// <summary>Builds a <c>Series</c> from generic enumerables of legends</summary>
        /// <param name="label">the label</param>
        /// <param name="legends">the legends</param>
        /// <returns>a <c>Series</c></returns>
        public static TY Create<TY>(TV label, IList<T> legends) where TY : Series<T, TU, TV>
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            if (legends == null) throw new ArgumentNullException(nameof(legends));

            return Create<TY>(label, legends, new TU[legends.Count()]);
        }

        /// <summary>Builds a <c>Series</c> from its serialized form</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        public static TY Create<TY>(XmlNode node) where TY : Series<T, TU, TV>
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            TV label = node.SelectSingleNode("series/label").Attributes["value"].Value.Parse<TV>();
            List<T> legends = new List<T>();
            List<TU> data = new List<TU>();

            XmlNodeList points = node.SelectNodes("series/point");
            foreach (XmlNode pointNode in points)
            {
                TU value = pointNode.Attributes["value"].Value.Parse<TU>();
                T legend = pointNode.Attributes["legend"].Value.Parse<T>();
                legends.Add(legend);
                data.Add(value);
            }

            return Create<TY>(label, legends, data.ToArray());
        }

        /// <summary>Builds a <c>Series</c> from a string</summary>
        /// <param name="text">the <c>String</c> content</param>
        public static TY Create<TY>(string text) where TY : Series<T, TU, TV>
        {
            if (text == null) throw new ArgumentOutOfRangeException(nameof(text));

            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return null;

            TU[] data = new TU[lines.Length - 1];
            List<T> legends = new List<T>();
            TV label = lines[0].Split(CsvHelper.Separator.ToCharArray())[1].Parse<TV>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] content = lines[i].Split(CsvHelper.Separator.ToCharArray());
                data[i - 1] = content[1].Parse<TU>();
                legends.Add(content[0].Parse<T>());
            }

            return Create<TY>(label, legends, data);
        }
        #endregion
    }
}

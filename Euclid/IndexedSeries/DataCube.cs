using Euclid.Extensions;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.IndexedSeries
{
    /// <summary>Class representing a cube of synchronized data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="U">the label type</typeparam>
    /// <typeparam name="V">the layer type</typeparam>
    /// <typeparam name="W">the data type</typeparam>
    public class DataCube<T, U, V, W> : IXmlable, ICSVable
        where T : IComparable<T>, IEquatable<T>
        where U : IEquatable<U>, IConvertible
        where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private Header<T> _legends;
        private Header<U> _labels;
        private Header<V> _layers;
        private W[,,] _data;
        #endregion

        #region Constructors

        private DataCube(IList<T> legends, IList<U> labels, IList<V> layers, W[,,] data)
        {
            _data = Arrays.Clone(data);
            _legends = new Header<T>(legends);
            _labels = new Header<U>(labels);
            _layers = new Header<V>(layers);
        }

        /// <summary>Builds a <c>DataCube</c> from its serialized form</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        /// <returns>a <c>DataCube</c></returns>
        public static DataCube<T, U, V, W> Create(XmlNode node)
        {
            XmlNodeList legendNodes = node.SelectNodes("dataFrame/legend"),
                labelNodes = node.SelectNodes("dataFrame/label"),
                layerNodes = node.SelectNodes("dataFrame/layer"),
                dataNodes = node.SelectNodes("dataFrame/point");

            #region Legends
            T[] legends = new T[legendNodes.Count];
            foreach (XmlNode legend in legendNodes)
            {
                int index = int.Parse(legend.Attributes["index"].Value);
                legends[index] = legend.Attributes["value"].Value.Parse<T>();
            }
            #endregion

            #region Labels
            U[] labels = new U[labelNodes.Count];
            foreach (XmlNode label in labelNodes)
            {
                int index = int.Parse(label.Attributes["index"].Value);
                labels[index] = label.Attributes["value"].Value.Parse<U>();
            }
            #endregion

            #region Layers
            V[] layers = new V[layerNodes.Count];
            foreach (XmlNode layer in layerNodes)
            {
                int index = int.Parse(layer.Attributes["index"].Value);
                layers[index] = layer.Attributes["value"].Value.Parse<V>();
            }
            #endregion

            #region Data
            W[,,] data = new W[legends.Length, labels.Length, layers.Length];
            foreach (XmlNode point in dataNodes)
            {
                int row = int.Parse(point.Attributes["row"].Value),
                    col = int.Parse(point.Attributes["col"].Value),
                    layer = int.Parse(point.Attributes["layer"].Value);
                W value = point.Attributes["value"].Value.Parse<W>();
                data[row, col, layer] = value;
            }
            #endregion

            return new DataCube<T, U, V, W>(legends, labels, layers, data);
        }

        /// <summary>Builds a <c>DataCube</c> </summary>
        /// <param name="labels">the labels</param>
        /// <param name="layers">the layers</param>
        /// <param name="legends">the legends</param>
        /// <param name="data">the data</param>
        /// <returns>a <c>DataCube</c></returns>
        public static DataCube<T, U, V, W> Create(IList<T> legends, IList<U> labels, IList<V> layers, W[,,] data)
        {
            return new DataCube<T, U, V, W>(legends, labels, layers, data);
        }

        /// <summary>Builds a <c>DataCube</c></summary>
        /// <param name="labels">the labels</param>
        /// <param name="layers">the layers</param>
        /// <param name="legends">the legends</param>
        /// <returns>a <c>DataCube</c></returns>
        public static DataCube<T, U, V, W> Create(IList<T> legends, IList<U> labels, IList<V> layers)
        {
            return new DataCube<T, U, V, W>(legends, labels, layers, new W[legends.Count, labels.Count, layers.Count]);
        }

        #endregion

        #region Accessors
        /// <summary>Returns the legends</summary>
        public T[] Legends
        {
            get { return _legends.Values; }
        }

        /// <summary>Returns the labels</summary>
        public U[] Labels
        {
            get { return _labels.Values; }
        }

        /// <summary>Returns the layers</summary>
        public V[] Layers
        {
            get { return _layers.Values; }
        }

        /// <summary>Returns the number of columns</summary>
        public int Columns
        {
            get { return _labels.Count; }
        }

        /// <summary>Returns the number of rows</summary>
        public int Rows
        {
            get { return _legends.Count; }
        }

        /// <summary>Returns the number of layers</summary>
        public int Depth
        {
            get { return _layers.Count; }
        }

        /// <summary>Gets the data</summary>
        public W[,,] Data
        {
            get { return _data; }
        }

        /// <summary>Gets and sets the data for the i-th row and j-th column of the <c>DataFrame</c></summary>
        /// <param name="i">the row index</param>
        /// <param name="j">the column index</param>
        /// <param name="k">the layer index</param>
        /// <returns>a data point</returns>
        public W this[int i, int j, int k]
        {
            get { return _data[i, j, k]; }
            set { _data[i, j, k] = value; }
        }

        /// <summary>Gets and sets the data for a given legend and a given label</summary>
        /// <param name="t">the legend</param>
        /// <param name="u">the label</param>
        /// <param name="v">the layer</param>
        /// <returns>a data point</returns>
        public W this[T t, U u, V v]
        {
            get { return _data[_legends[t], _labels[u], _layers[v]]; }
            set { _data[_legends[t], _labels[u], _layers[v]] = value; }
        }
        #endregion

        #region Methods

        #region Label

        /// <summary>Returns the label rank</summary>
        /// <param name="label">the target label</param>
        /// <returns>an <c>Integer</c></returns>
        public int GetLabelRank(U label) { return _labels.Contains(label) ? _labels[label] : -1; }

        #endregion

        #region Series

        /// <summary> Gets the data-point column of the given label and layer</summary>
        /// <param name="label">the label</param>
        /// <param name="layer">the layer</param>
        /// <returns> a <c>Series</c></returns>
        public Series<T, W, string> GetSeriesAt(U label, V layer)
        {
            int labelIndex = _labels[label];
            if (labelIndex == -1) throw new ArgumentException(string.Format("Label [{0}] was not found", label.ToString()));

            int layerIndex = _layers[layer];
            if (layerIndex == -1) throw new ArgumentException(string.Format("Layer [{0}] was not found", layer.ToString()));

            W[] result = new W[_legends.Count];
            for (int i = 0; i < _legends.Count; i++)
                result[i] = _data[i, labelIndex, layerIndex];
            return Series<T, W, string>.Create(string.Format("{0}{1}", label.ToString(), layer.ToString()), _legends, result);
        }

        /// <summary> Gets all the data as an array of <c>Series</c></summary>
        /// <returns>an array of <c>Series</c></returns>
        public Series<T, W, string>[,] GetSeries()
        {
            Series<T, W, string>[,] result = new Series<T, W, string>[_labels.Count, _layers.Count];
            for (int k = 0; k < _layers.Count; k++)
                for (int j = 0; j < _labels.Count; j++)
                {
                    W[] data = new W[_legends.Count];
                    for (int i = 0; i < _legends.Count; i++)
                        data[i] = _data[i, j, k];

                    result[j, k] = Series<T, W, string>.Create(string.Format("{0}{1}", _labels.ElementAt(j), _layers.ElementAt(k)), _legends, data);
                }
            return result;
        }

        #endregion

        #region DataFrame

        #region Get

        /// <summary>Gets the dataframe for a given label</summary>
        /// <param name="label">the label</param>
        /// <returns>a <c>DataFrame</c></returns>
        public DataFrame<T, W, V> GetDataFrameForLabel(U label)
        {
            int labelIndex = _labels[label];
            if (labelIndex == -1) throw new ArgumentException(string.Format("Label [{0}] was not found", label.ToString()));
            W[,] result = new W[_legends.Count, _layers.Count];
            for (int i = 0; i < _legends.Count; i++)
                for (int k = 0; k < _layers.Count; k++)
                    result[i, k] = _data[i, labelIndex, k];
            return DataFrame<T, W, V>.Create(_layers.ToList(), _legends.ToList(), result);
        }

        /// <summary>Gets the dataframe for a given layer</summary>
        /// <param name="layer">the layer</param>
        /// <returns>a <c>DataFrame</c></returns>
        public DataFrame<T, W, U> GetDataFrameForLayer(V layer)
        {
            int layerIndex = _layers[layer];
            if (layerIndex == -1) throw new ArgumentException(string.Format("Layer [{0}] was not found", layer.ToString()));
            W[,] result = new W[_legends.Count, _layers.Count];
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    result[i, j] = _data[i, j, layerIndex];
            return DataFrame<T, W, U>.Create(_labels.ToList(), _legends.ToList(), result);
        }

        #endregion

        #endregion

        /// <summary>Clones the <c>DataCube</c></summary>
        /// <returns>a <c>DataCube</c></returns>
        public DataCube<T, U, V, W> Clone()
        {
            return new DataCube<T, U, V, W>(_legends.Values, _labels.Values, _layers.Values, Arrays.Clone(_data));
        }

        #region Apply

        /// <summary>Applies a function to all the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<W, W> function)
        {
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    for (int k = 0; k < _layers.Count; k++)
                        _data[i, j, k] = function(_data[i, j, k]);
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
        public void ApplyOnLabels(Func<U, U> function)
        {
            for (int i = 0; i < _labels.Count; i++)
                _labels.Rename(_labels.ElementAt(i), function(_labels.ElementAt(i)));
        }

        /// <summary>Applies a function to all the layers</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLayers(Func<V, V> function)
        {
            for (int i = 0; i < _layers.Count; i++)
                _layers.Rename(_layers.ElementAt(i), function(_layers.ElementAt(i)));
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
        public U GetLabel(int index)
        {
            return _labels.ElementAt(index);
        }

        /// <summary>Renames a label</summary>
        /// <param name="oldValue">the old value</param>
        /// <param name="newValue">the new value</param>
        public void RenameLabel(U oldValue, U newValue)
        {
            _labels.Rename(oldValue, newValue);
        }

        /// <summary>Gets the i-th layer's value</summary>
        /// <param name="index">the index</param>
        /// <returns>a layer</returns>
        public V GetLayer(int index)
        {
            return _layers.ElementAt(index);
        }

        /// <summary>Renames a layer</summary>
        /// <param name="oldValue">the old value</param>
        /// <param name="newValue">the new value</param>
        public void RenameLabel(V oldValue, V newValue)
        {
            _layers.Rename(oldValue, newValue);
        }

        #endregion

        #endregion

        #region IXmlable
        /// <summary>Serializes the <c>DataCube</c> to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("dataCube");

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

            #region Layers
            for (int i = 0; i < _layers.Count; i++)
            {
                writer.WriteStartElement("layer");
                writer.WriteAttributeString("value", _layers.ElementAt(i).ToString());
                writer.WriteAttributeString("index", i.ToString());
                writer.WriteEndElement();
            }
            #endregion


            #region Data
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    for (int k = 0; k < _layers.Count; k++)
                    {
                        writer.WriteStartElement("point");
                        writer.WriteAttributeString("row", i.ToString());
                        writer.WriteAttributeString("col", j.ToString());
                        writer.WriteAttributeString("layer", k.ToString());
                        writer.WriteAttributeString("value", _data[i, j, k].ToString());
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
            List<string> lines = new List<string>();
            lines.Add(string.Join(CSVHelper.Separator.ToString(), "Legend", "Label", "Layer", "Value"));

            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    for (int k = 0; k < _layers.Count; k++)
                        lines.Add(string.Join(CSVHelper.Separator.ToString(), _legends.ElementAt(i).ToString(), _labels.ElementAt(j).ToString(), _layers.ElementAt(k).ToString(), _data[i, j, k].ToString()));
            return string.Join(Environment.NewLine, lines);
        }
        #endregion

        /// <summary>Equality comparer</summary>
        /// <param name="other">the other DataFrame</param>
        /// <returns>true if the data, legends and labels match, false otherwise</returns>
        public bool Equals(DataCube<T, U, V, W> other)
        {
            if (other._labels.Count == _labels.Count && other._labels.Except(_labels).Count() == 0 &&
                other._legends.Count == _legends.Count && other._legends.Except(_legends).Count() == 0 &&
                other._layers.Count == _layers.Count && other._layers.Except(_layers).Count() == 0)
            {
                for (int i = 0; i < other.Rows; i++)
                {
                    T t = other._legends.ElementAt(i);
                    for (int j = 0; j < other.Columns; j++)
                    {
                        U u = other._labels.ElementAt(j);
                        for (int k = 0; k < other.Depth; k++)
                        {
                            V v = other._layers.ElementAt(k);
                            if (!other[t, u, v].Equals(this[t, u, v]))
                                return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}

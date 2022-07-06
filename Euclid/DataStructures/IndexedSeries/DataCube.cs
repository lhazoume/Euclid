using Euclid.Extensions;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>Class representing a cube of synchronized data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="TU">the label type</typeparam>
    /// <typeparam name="TV">the layer type</typeparam>
    /// <typeparam name="TW">the data type</typeparam>
    public class DataCube<T, TU, TV, TW> : IXmlable, ICSVable
        where T : IComparable<T>, IEquatable<T>
        where TU : IEquatable<TU>
        where TV : IEquatable<TV>
    {
        #region Declarations
        private readonly Header<T> _legends;
        private readonly Header<TU> _labels;
        private readonly Header<TV> _layers;
        private readonly TW[,,] _data;
        #endregion

        #region Constructors

        private DataCube(IList<T> legends, IList<TU> labels, IList<TV> layers, TW[,,] data)
        {
            _data = Arrays.Clone(data);
            _legends = new Header<T>(legends);
            _labels = new Header<TU>(labels);
            _layers = new Header<TV>(layers);
        }

        /// <summary>Builds a <c>DataCube</c> from its serialized form</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        /// <returns>a <c>DataCube</c></returns>
        public static DataCube<T, TU, TV, TW> Create(XmlNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

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
            TU[] labels = new TU[labelNodes.Count];
            foreach (XmlNode label in labelNodes)
            {
                int index = int.Parse(label.Attributes["index"].Value);
                labels[index] = label.Attributes["value"].Value.Parse<TU>();
            }
            #endregion

            #region Layers
            TV[] layers = new TV[layerNodes.Count];
            foreach (XmlNode layer in layerNodes)
            {
                int index = int.Parse(layer.Attributes["index"].Value);
                layers[index] = layer.Attributes["value"].Value.Parse<TV>();
            }
            #endregion

            #region Data
            TW[,,] data = new TW[legends.Length, labels.Length, layers.Length];
            foreach (XmlNode point in dataNodes)
            {
                int row = int.Parse(point.Attributes["row"].Value),
                    col = int.Parse(point.Attributes["col"].Value),
                    layer = int.Parse(point.Attributes["layer"].Value);
                TW value = point.Attributes["value"].Value.Parse<TW>();
                data[row, col, layer] = value;
            }
            #endregion

            return new DataCube<T, TU, TV, TW>(legends, labels, layers, data);
        }

        /// <summary>Builds a <c>DataCube</c> </summary>
        /// <param name="labels">the labels</param>
        /// <param name="layers">the layers</param>
        /// <param name="legends">the legends</param>
        /// <param name="data">the data</param>
        /// <returns>a <c>DataCube</c></returns>
        public static DataCube<T, TU, TV, TW> Create(IList<T> legends, IList<TU> labels, IList<TV> layers, TW[,,] data)
        {
            return new DataCube<T, TU, TV, TW>(legends, labels, layers, data);
        }

        /// <summary>Builds a <c>DataCube</c></summary>
        /// <param name="labels">the labels</param>
        /// <param name="layers">the layers</param>
        /// <param name="legends">the legends</param>
        /// <returns>a <c>DataCube</c></returns>
        public static DataCube<T, TU, TV, TW> Create(IList<T> legends, IList<TU> labels, IList<TV> layers)
        {
            if (legends == null) throw new ArgumentNullException(nameof(legends));
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            if (layers == null) throw new ArgumentNullException(nameof(layers));

            return new DataCube<T, TU, TV, TW>(legends, labels, layers, new TW[legends.Count, labels.Count, layers.Count]);
        }

        #endregion

        #region Accessors
        /// <summary>Returns the legends</summary>
        public T[] Legends => _legends.Values;

        /// <summary>Returns the labels</summary>
        public TU[] Labels => _labels.Values;

        /// <summary>Returns the layers</summary>
        public TV[] Layers => _layers.Values;

        /// <summary>Returns the number of columns</summary>
        public int Columns => _labels.Count;

        /// <summary>Returns the number of rows</summary>
        public int Rows => _legends.Count;

        /// <summary>Returns the number of layers</summary>
        public int Depth => _layers.Count;

        /// <summary>Gets the data</summary>
        public TW[,,] Data => _data;

        /// <summary>Gets and sets the data for the i-th row and j-th column of the <c>DataFrame</c></summary>
        /// <param name="i">the row index</param>
        /// <param name="j">the column index</param>
        /// <param name="k">the layer index</param>
        /// <returns>a data point</returns>
        public TW this[int i, int j, int k]
        {
            get { return _data[i, j, k]; }
            set { _data[i, j, k] = value; }
        }

        /// <summary>Gets and sets the data for a given legend and a given label</summary>
        /// <param name="t">the legend</param>
        /// <param name="u">the label</param>
        /// <param name="v">the layer</param>
        /// <returns>a data point</returns>
        public TW this[T t, TU u, TV v]
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
        public int GetLabelRank(TU label) { return _labels.Contains(label) ? _labels[label] : -1; }

        #endregion

        #region Series

        /// <summary> Gets the data-point column of the given label and layer</summary>
        /// <param name="label">the label</param>
        /// <param name="layer">the layer</param>
        /// <returns> a <c>Series</c></returns>
        public Series<T, TW, string> GetSeriesAt(TU label, TV layer)
        {
            int labelIndex = _labels[label];
            if (labelIndex == -1) throw new ArgumentException($"Label [{label}] was not found");

            int layerIndex = _layers[layer];
            if (layerIndex == -1) throw new ArgumentException($"Layer [{layer}] was not found");

            TW[] result = new TW[_legends.Count];
            for (int i = 0; i < _legends.Count; i++)
                result[i] = _data[i, labelIndex, layerIndex];
            return Series<T, TW, string>.Create<Series<T, TW, string>>($"{label}{layer}", _legends, result);
        }

        /// <summary> Gets all the data as an array of <c>Series</c></summary>
        /// <returns>an array of <c>Series</c></returns>
        public Series<T, TW, string>[,] GetSeries()
        {
            Series<T, TW, string>[,] result = new Series<T, TW, string>[_labels.Count, _layers.Count];
            for (int k = 0; k < _layers.Count; k++)
                for (int j = 0; j < _labels.Count; j++)
                {
                    TW[] data = new TW[_legends.Count];
                    for (int i = 0; i < _legends.Count; i++)
                        data[i] = _data[i, j, k];

                    result[j, k] = Series<T, TW, string>.Create<Series<T, TW, string>>($"{_labels.ElementAt(j)}{_layers.ElementAt(k)}", _legends, data);
                }
            return result;
        }

        #endregion

        #region DataFrame

        #region Get

        /// <summary>Gets the dataframe for a given label</summary>
        /// <param name="label">the label</param>
        /// <returns>a <c>DataFrame</c></returns>
        public DataFrame<T, TW, TV> GetDataFrameForLabel(TU label)
        {
            int labelIndex = _labels[label];
            if (labelIndex == -1) throw new ArgumentException($"Label [{label}] was not found");
            TW[][] result = Arrays.Build<TW>(_legends.Count, _layers.Count);
            for (int i = 0; i < _legends.Count; i++)
                for (int k = 0; k < _layers.Count; k++)
                    result[i][k] = _data[i, labelIndex, k];
            return DataFrame<T, TW, TV>.Create<DataFrame<T, TW, TV>>(_layers.ToList(), _legends.ToList(), result);
        }

        /// <summary>Gets the dataframe for a given layer</summary>
        /// <param name="layer">the layer</param>
        /// <returns>a <c>DataFrame</c></returns>
        public DataFrame<T, TW, TU> GetDataFrameForLayer(TV layer)
        {
            int layerIndex = _layers[layer];
            if (layerIndex == -1) throw new ArgumentException($"Layer [{layer}] was not found");
            TW[][] result = Arrays.Build<TW>(_legends.Count, _labels.Count);
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    result[i][j] = _data[i, j, layerIndex];
            return DataFrame<T, TW, TU>.Create<DataFrame<T, TW, TU>>(_labels.ToList(), _legends.ToList(), result);
        }

        #endregion

        #endregion

        /// <summary>Clones the <c>DataCube</c></summary>
        /// <returns>a <c>DataCube</c></returns>
        public DataCube<T, TU, TV, TW> Clone()
        {
            return new DataCube<T, TU, TV, TW>(_legends.Values, _labels.Values, _layers.Values, Arrays.Clone(_data));
        }

        #region Apply

        /// <summary>Applies a function to all the data</summary>
        /// <param name="function">the function</param>
        public void ApplyOnData(Func<TW, TW> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function), "the function should not be null");
            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    for (int k = 0; k < _layers.Count; k++)
                        _data[i, j, k] = function(_data[i, j, k]);
        }

        /// <summary>Applies a function to all the legends</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLegends(Func<T, T> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function), "the function should not be null");
            for (int i = 0; i < _legends.Count; i++)
                _legends.Rename(_legends.ElementAt(i), function(_legends.ElementAt(i)));
        }

        /// <summary>Applies a function to all the labels</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLabels(Func<TU, TU> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function), "the function should not be null");
            for (int i = 0; i < _labels.Count; i++)
                _labels.Rename(_labels.ElementAt(i), function(_labels.ElementAt(i)));
        }

        /// <summary>Applies a function to all the layers</summary>
        /// <param name="function">the function</param>
        public void ApplyOnLayers(Func<TV, TV> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function), "the function should not be null");
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
        public TU GetLabel(int index)
        {
            return _labels.ElementAt(index);
        }

        /// <summary>Renames a label</summary>
        /// <param name="oldValue">the old value</param>
        /// <param name="newValue">the new value</param>
        public void RenameLabel(TU oldValue, TU newValue)
        {
            _labels.Rename(oldValue, newValue);
        }

        /// <summary>Gets the i-th layer's value</summary>
        /// <param name="index">the index</param>
        /// <returns>a layer</returns>
        public TV GetLayer(int index)
        {
            return _layers.ElementAt(index);
        }

        /// <summary>Renames a layer</summary>
        /// <param name="oldValue">the old value</param>
        /// <param name="newValue">the new value</param>
        public void RenameLabel(TV oldValue, TV newValue)
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
            if (writer == null) throw new ArgumentNullException(nameof(writer));

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
            List<string> lines = new List<string>
            {
                string.Join(CsvHelper.Separator.ToString(), "Legend", "Label", "Layer", "Value")
            };

            for (int i = 0; i < _legends.Count; i++)
                for (int j = 0; j < _labels.Count; j++)
                    for (int k = 0; k < _layers.Count; k++)
                        lines.Add(string.Join(CsvHelper.Separator.ToString(), _legends.ElementAt(i).ToString(), _labels.ElementAt(j).ToString(), _layers.ElementAt(k).ToString(), _data[i, j, k].ToString()));
            return string.Join(Environment.NewLine, lines);
        }
        #endregion

        /// <summary>Equality comparer</summary>
        /// <param name="other">the other DataFrame</param>
        /// <returns>true if the data, legends and labels match, false otherwise</returns>
        public bool Equals(DataCube<T, TU, TV, TW> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (other._labels.Count == _labels.Count && !other._labels.Except(_labels).Any() &&
                other._legends.Count == _legends.Count && !other._legends.Except(_legends).Any() &&
                other._layers.Count == _layers.Count && !other._layers.Except(_layers).Any())
            {
                for (int i = 0; i < other.Rows; i++)
                {
                    T t = other._legends.ElementAt(i);
                    for (int j = 0; j < other.Columns; j++)
                    {
                        TU u = other._labels.ElementAt(j);
                        for (int k = 0; k < other.Depth; k++)
                        {
                            TV v = other._layers.ElementAt(k);
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

using Euclid.Analytics.NeuralNetworks.ActivationFunctions;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.Analytics.NeuralNetworks.FeedForward
{
    /// <summary>Perceptron feed-forward neural network class</summary>
    public class Perceptron : IXmlable
    {
        private readonly Layer[] _layers;

        private Perceptron(IEnumerable<Layer> layers)
        {
            _layers = layers.ToArray();
        }

        #region Accessors
        /// <summary>Gets a deep copy of the perceptron network</summary>
        public Perceptron Clone
        {
            get
            {
                Layer[] layers = new Layer[_layers.Length];
                for (int i = 0; i < _layers.Length; i++)
                    layers[i] = _layers[i].Clone;
                return new Perceptron(layers);
            }
        }

        /// <summary>Gets a layer of the perceptron</summary>
        /// <param name="index">the layer's index</param>
        /// <returns>a Layer</returns>
        public Layer this[int index]
        {
            get { return _layers[index]; }
        }

        /// <summary>Gets the number of layers in the network</summary>
        public int LayerCount
        {
            get { return _layers.Length; }
        }

        /// <summary>Gets the number of parameters (i.e. freedom degrees) of the network</summary>
        public int Parameters
        {
            get { return _layers.Sum(l => l.Parameters); }
        }

        /// <summary>Gets the size of the input data expected by the network</summary>
        public int InputSize
        {
            get { return _layers[0].InputSize; }
        }
        #endregion

        #region Methods
        /// <summary>Writes the network's characteristics to an XML</summary>
        /// <param name="writer">the XML writer</param>
        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("neuralNetwork");
            for (int i = 0; i < _layers.Length; i++)
            {
                writer.WriteStartElement("layer");
                writer.WriteAttributeString("index", i.ToString());
                _layers[i].ToXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>Calculates the network's response to an input</summary>
        /// <param name="input">the input data</param>
        /// <returns>a Vector</returns>
        public Vector Process(Vector input)
        {
            Vector tmp = _layers[0].Process(input);
            for (int i = 1; i < _layers.Length; i++)
                tmp = _layers[i].Process(tmp);
            return tmp;
        }

        /// <summary>Calculates the network's responses to a set of inputs</summary>
        /// <param name="inputs">the set of input data</param>
        /// <returns>a List of Vectors</returns>
        public List<Vector> Process(List<Vector> inputs)
        {
            List<Vector> tmp = _layers[0].Process(inputs);
            for (int i = 1; i < _layers.Length; i++)
                tmp = _layers[i].Process(tmp);
            return tmp;
        }
        #endregion

        #region Creators
        /// <summary>Creates an empty network</summary>
        /// <param name="function">the activation function</param>
        /// <param name="inputSize">the input size</param>
        /// <param name="layerSizes">the layers' sizes</param>
        /// <returns>a Perceptron network</returns>
        public static Perceptron Create(IActivationFunction function, int inputSize, params int[] layerSizes)
        {
            if (layerSizes.Length == 0) throw new ArgumentOutOfRangeException("there should be at least one layer");
            if (inputSize <= 0) throw new ArgumentOutOfRangeException("the input size should be > 0");

            List<Layer> layers = new List<Layer>();

            layers.Add(Layer.Create(layerSizes[0], inputSize, function));
            for (int i = 1; i < layerSizes.Length; i++)
                layers.Add(Layer.Create(layerSizes[i], layerSizes[i - 1], function));
            
            return new Perceptron(layers);
        }

        /// <summary>Builds ans fills a Perceptron network from its XML representation</summary>
        /// <param name="node">the XML node</param>
        /// <returns>a Perceptron network</returns>
        public static Perceptron Create(XmlNode node)
        {
            XmlNodeList layerNodes = node.SelectNodes("neuralNetwork/layer");
            Layer[] layers = new Layer[layerNodes.Count];

            foreach (XmlNode layerNode in layerNodes)
            {
                int index = int.Parse(layerNode.Attributes["index"].Value);
                layers[index] = Layer.Create(layerNode);
            }
            return new Perceptron(layers);
        }
        #endregion
    }
}

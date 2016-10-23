using Euclid.Analytics.NeuralNetworks.ActivationFunctions;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.Analytics.NeuralNetworks.FeedForward
{
    public class Perceptron : IXmlable
    {
        private readonly Layer[] _layers;

        private Perceptron(IEnumerable<Layer> layers)
        {
            _layers = layers.ToArray();
        }

        #region Accessors
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
        public Layer this[int index]
        {
            get { return _layers[index]; }
        }
        public int LayerCount
        {
            get { return _layers.Length; }
        }
        public int Parameters
        {
            get { return _layers.Sum(l => l.Parameters); }
        }
        public int InputSize
        {
            get { return _layers[0].InputSize; }
        }
        #endregion

        #region Methods
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
        public Vector Process(Vector input)
        {
            Vector tmp = _layers[0].Process(input);
            for (int i = 1; i < _layers.Length; i++)
                tmp = _layers[i].Process(tmp);
            return tmp;
        }
        public List<Vector> Process(List<Vector> inputs)
        {
            List<Vector> tmp = _layers[0].Process(inputs);
            for (int i = 1; i < _layers.Length; i++)
                tmp = _layers[i].Process(tmp);
            return tmp;
        }
        #endregion

        #region Creators
        public static Perceptron Create(IActivationFunction function, int inputSize, params int[] layerSizes)
        {
            if (layerSizes.Length == 0) throw new ArgumentNullException("there should be at least one layer");

            List<Layer> layers = new List<Layer>();

            layers.Add(Layer.Create(layerSizes[0], inputSize, function));
            for (int i = 1; i < layerSizes.Length; i++)
            {
                Layer l = Layer.Create(layerSizes[i], layerSizes[i - 1], function);
                layers.Add(l);
            }
            return new Perceptron(layers);
        }
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

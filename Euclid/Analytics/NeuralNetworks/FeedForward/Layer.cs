using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.Analytics.NeuralNetworks.FeedForward
{
    /// <summary>A Layer class for feed forward perceptrons</summary>
    public class Layer
    {
        #region Declarations
        private readonly int _layerSize, _inputSize;
        private readonly Matrix _weights;
        private readonly Vector _biases;
        private Vector _z, _a;
        private readonly IActivationFunction _activation;
        #endregion

        private Layer(int layerSize, int inputSize, IActivationFunction function)
        {
            _layerSize = layerSize;
            _inputSize = inputSize;
            _weights = Matrix.Create(_layerSize, _inputSize);
            _biases = Vector.Create(_layerSize);
            _activation = function;
        }

        public Vector Process(Vector input)
        {
            if (input.Size != _inputSize) throw new ArgumentOutOfRangeException("the inputs' size does not match the layer's characteristics");
            _z = (_weights * input) + _biases;
            _a = Vector.Apply(_z, _activation.Function);
            return _a.Clone;
        }
        public List<Vector> Process(List<Vector> inputs)
        {
            _z = Vector.Create(_layerSize);
            _a = Vector.Create(_layerSize);

            List<Vector> outputs = new List<Vector>();
            for (int i = 0; i < inputs.Count; i++)
            {
                Vector z = (_weights * inputs[i]) + _biases,
                    a = Vector.Apply(z, _activation.Function);
                _z += z;
                _a += a;
                outputs.Add(a);
            }
            return outputs;
        }
        public void Increment(Matrix weightIncrements, Vector biasIncrements)
        {
            for (int i = 0; i < _weights.Size; i++) _weights[i] += weightIncrements[i];
            for (int i = 0; i < _biases.Size; i++) _biases[i] += biasIncrements[i];
        }
        public void Set(Matrix weights, Vector biases)
        {
            for (int i = 0; i < _weights.Size; i++) _weights[i] = weights[i];
            for (int i = 0; i < _biases.Size; i++) _biases[i] = biases[i];
        }
        public void SetBiases(Vector biases)
        {
            for (int i = 0; i < _biases.Size; i++) _biases[i] = biases[i];
        }
        public void SetWeights(Matrix weights)
        {
            for (int i = 0; i < _weights.Size; i++) _weights[i] = weights[i];
        }
        public double Weight(int neuronIndex, int inputIndex)
        {
            return _weights[neuronIndex, inputIndex];
        }
        public double Bias(int neuronIndex)
        {
            return _biases[neuronIndex];
        }
        public Vector Biases
        {
            get { return _biases; }
        }
        public Matrix Weights
        {
            get { return _weights; }
        }


        #region Accessors
        /// <summary>Gets a deep copy of the Layer</summary>
        public Layer Clone
        {
            get
            {
                Layer layer = new Layer(_layerSize, _inputSize, _activation);
                for (int i = 0; i < _weights.Size; i++) layer._weights[i] = _weights[i];
                for (int i = 0; i < _biases.Size; i++) layer._biases[i] = _biases[i];
                return layer;
            }
        }

        /// <summary>Gets the number of parameters of the Layer</summary>
        public int Parameters
        {
            get { return _layerSize * (_inputSize + 1); }
        }

        public int LayerSize
        {
            get { return _layerSize; }
        }

        public int InputSize
        {
            get { return _inputSize; }
        }

        public IActivationFunction Function
        {
            get { return _activation; }
        }

        public Vector A
        {
            get { return _a; }
        }
        public Vector Z
        {
            get { return _z; }
        }
        #endregion

        public void ToXml(XmlWriter writer)
        {
            writer.WriteElementString("layerSize", _layerSize.ToString());
            writer.WriteElementString("inputSize", _inputSize.ToString());
            writer.WriteElementString("activation", _activation.GetType().Name);

            for (int i = 0; i < _layerSize; i++)
            {
                writer.WriteStartElement("neuron");
                writer.WriteAttributeString("index", i.ToString());

                for (int j = 0; j < _inputSize; j++)
                {
                    writer.WriteStartElement("weight");
                    writer.WriteAttributeString("index", j.ToString());
                    writer.WriteAttributeString("value", _weights[i, j].ToString("#0.0###########################"));
                    writer.WriteEndElement();
                }

                writer.WriteElementString("bias", _biases[i].ToString("#0.0###########################"));
                writer.WriteEndElement();
            }
        }

        #region Creators
        public static Layer Create(Matrix weights, Vector biases, IActivationFunction function)
        {
            if (weights.Rows != biases.Size) throw new ArgumentException("the matrix and biases' sizes do not match");
            Layer layer = new Layer(weights.Rows, weights.Columns, function);

            for (int i = 0; i < weights.Size; i++) layer._weights[i] = weights[i];
            for (int i = 0; i < biases.Size; i++) layer._biases[i] = biases[i];

            return layer;
        }
        public static Layer Create(int layerSize, int inputSize, IActivationFunction function)
        {
            Layer layer = new Layer(layerSize, inputSize, function);
            return layer;
        }
        public static Layer Create(XmlNode node)
        {
            int layerSize = int.Parse(node.SelectSingleNode("layerSize").InnerText),
                inputSize = int.Parse(node.SelectSingleNode("inputSize").InnerText);
            string activation = node.SelectSingleNode("activation").InnerText;
            Layer layer = new Layer(layerSize, inputSize, activation.ActivationFunction());

            XmlNodeList neurons = node.SelectNodes("neuron");
            foreach (XmlNode neuron in neurons)
            {
                int i = int.Parse(neuron.Attributes["index"].Value);
                layer._biases[i] = double.Parse(neuron.SelectSingleNode("bias").InnerText);

                XmlNodeList weights = neuron.SelectNodes("weight");
                foreach (XmlNode weight in weights)
                {
                    int j = int.Parse(weight.Attributes["index"].Value);
                    layer._weights[i, j] = double.Parse(weight.Attributes["value"].Value);
                }
            }

            return layer;
        }
        #endregion
    }
}

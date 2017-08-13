using Euclid.Analytics.NeuralNetworks.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Euclid.Analytics.NeuralNetworks.FeedForward
{
    /// <summary>A Layer class for feed-forward perceptron</summary>
    public class Layer
    {
        #region Declarations
        private readonly int _layerSize, _inputSize;
        private Matrix _weights;
        private Vector _biases,
            _z, _a;
        private readonly IActivationFunction _activation;
        #endregion

        #region Constructors
        private Layer(int layerSize, int inputSize, IActivationFunction function)
        {
            _layerSize = layerSize;
            _inputSize = inputSize;
            _weights = Matrix.Create(_layerSize, _inputSize);
            _biases = Vector.Create(_layerSize);
            _activation = function;
        }
        private Layer(Matrix weights, Vector biases, IActivationFunction function)
        {
            if (weights.Rows != biases.Size) throw new RankException("The size of the biases should fit the number of rows of the wieght matrix");
            _layerSize = biases.Size;
            _inputSize = weights.Columns;
            _weights = weights.Clone;
            _biases = biases.Clone;
            _activation = function;
        }
        #endregion

        #region Methods

        #region Process inputs

        /// <summary>Returns the output of a layer for a given input set</summary>
        /// <param name="input">the input vector</param>
        /// <returns>a <c>Vector</c></returns>
        public Vector Process(Vector input)
        {
            if (input.Size != _inputSize) throw new ArgumentOutOfRangeException("the inputs' size does not match the layer's characteristics");
            _z = (_weights * input) + _biases;
            _a = _z.Apply(_activation.Function);
            return _a.Clone;
        }

        /// <summary>Returns the list of the outputs of a layer for a given list of input sets</summary>
        /// <param name="inputs">the list of input vectors</param>
        /// <returns>a list of <c>Vector</c></returns>
        public List<Vector> Process(List<Vector> inputs)
        {
            _z = Vector.Create(_layerSize);
            _a = Vector.Create(_layerSize);

            List<Vector> outputs = new List<Vector>();
            for (int i = 0; i < inputs.Count; i++)
            {
                Vector z = (_weights * inputs[i]) + _biases,
                    a = z.Apply(_activation.Function);
                _z += z;
                _a += a;
                outputs.Add(a);
            }
            return outputs;
        }

        #endregion

        #region Get and set weights and biases

        /// <summary>Gets and sets the biases of the layer</summary>
        public Vector Biases
        {
            get { return _biases; }
            set
            {
                if (value.Size == _biases.Size)
                    _biases = value.Clone;
            }
        }

        /// <summary>Gets and sets the weights of the layer</summary>
        public Matrix Weights
        {
            get { return _weights; }
            set
            {
                if (value.Rows == _weights.Rows && value.Columns == _weights.Columns)
                    _weights = value.Clone;
            }
        }

        /// <summary>Gets the weight of a given neuron for a given input</summary>
        /// <param name="neuronIndex">the index of the neuron</param>
        /// <param name="inputIndex">the index of the input</param>
        /// <returns>a double</returns>
        public double Weight(int neuronIndex, int inputIndex)
        {
            return _weights[neuronIndex, inputIndex];
        }

        /// <summary>Gets the bias of a given neuron</summary>
        /// <param name="neuronIndex">the index of the neuron</param>
        /// <returns>a double</returns>
        public double Bias(int neuronIndex)
        {
            return _biases[neuronIndex];
        }

        #endregion

        #endregion

        #region Accessors
        /// <summary>Gets a deep copy of the Layer</summary>
        public Layer Clone
        {
            get { return new Layer(_weights, _biases, _activation); }
        }

        /// <summary>Gets the number of parameters of the Layer</summary>
        public int Parameters
        {
            get { return _layerSize * (_inputSize + 1); }
        }

        /// <summary>Gets the number of neurons in the Layer</summary>
        public int LayerSize
        {
            get { return _layerSize; }
        }

        /// <summary>Gets the input size of the Layer</summary>
        public int InputSize
        {
            get { return _inputSize; }
        }

        /// <summary>Gets the activation function of the Layer</summary>
        public IActivationFunction Function
        {
            get { return _activation; }
        }


        /// <summary>Gets the linear output of the layer before activation</summary>
        public Vector A
        {
            get { return _a; }
        }

        /// <summary>Gets the outputs of the layer's processing</summary>
        public Vector Z
        {
            get { return _z; }
        }
        #endregion

        #region IXmlable
        /// <summary>Serializes the <c>Layer</c> to a XML </summary>
        /// <param name="writer">the XMlWriter</param>
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
        #endregion

        #region Creators

        /// <summary>Builds a <c>Layer</c></summary>
        /// <param name="weights">the weghts of the layer</param>
        /// <param name="biases">the biases of the layer</param>
        /// <param name="function">the activation function</param>
        /// <returns>a <c>Layer</c></returns>
        public static Layer Create(Matrix weights, Vector biases, IActivationFunction function)
        {
            if (weights.Rows != biases.Size) throw new ArgumentException("the matrix and biases' sizes do not match");
            Layer layer = new Layer(weights.Rows, weights.Columns, function);

            layer._weights = weights.Clone;
            layer._biases = biases.Clone;

            return layer;
        }

        /// <summary>Builds an empty <c>Layer</c></summary>
        /// <param name="layerSize">the number of neurons in the layer</param>
        /// <param name="inputSize">the number of inputs of the layer</param>
        /// <param name="function">the activation function</param>
        /// <returns>a <c>Layer</c></returns>
        public static Layer Create(int layerSize, int inputSize, IActivationFunction function)
        {
            Layer layer = new Layer(layerSize, inputSize, function);
            return layer;
        }

        /// <summary>Builds a <c>Layer</c> from a XML node</summary>
        /// <param name="node">the node</param>
        /// <returns>a <c>Layer</c></returns>
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

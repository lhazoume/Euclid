using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Analytics.NeuralNetworks.FeedForward
{
    public class Trainer
    {
        private Perceptron _network;
        private IErrorFunction _errorFunction;
        private List<Tuple<int, double, double>> _convergence;
        private List<Tuple<Matrix, Vector>[]> _descentDirections = new List<Tuple<Matrix, Vector>[]>();

        public Trainer(Perceptron network, IErrorFunction errorFunction)
        {
            _network = network;
            _errorFunction = errorFunction;
        }

        public void TrainBackPropagation(List<Vector> learningX, List<Vector> learningY,
            List<Vector> validationX, List<Vector> validationY,
            double learningRate,
            int epochs)
        {
            _convergence = new List<Tuple<int, double, double>>();

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                #region Calculate Errs
                double sampleErr = _errorFunction.Function(_network.Process(learningX), learningY),
                    outOfSampleErr = _errorFunction.Function(_network.Process(validationX), validationY);
                _convergence.Add(new Tuple<int, double, double>(epoch, sampleErr, outOfSampleErr));
                #endregion

                #region Gradient and increnemt
                Tuple<Matrix, Vector>[] gradient = GlobalGradients(learningX, learningY);

                for (int l = 0; l < _network.LayerCount; l++)
                    _network[l].Increment(-learningRate * gradient[l].Item1, -learningRate * gradient[l].Item2);
                #endregion
            }

            /*List<Vector> projectedY = _network.Process(learningX);
            double testErr = _errorFunction.Function(projectedY, learningY),
                validationErr = _errorFunction.Function(_network.Process(validationX), validationY);*/
        }

        public void TrainBackPropagationAlternative(List<Vector> learningX, List<Vector> learningY,
            List<Vector> validationX, List<Vector> validationY,
            double learningRate,
            int epochs)
        {
            _convergence = new List<Tuple<int, double, double>>();

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                #region Calculate Errs
                double sampleErr = _errorFunction.Function(_network.Process(learningX), learningY),
                    outOfSampleErr = _errorFunction.Function(_network.Process(validationX), validationY);
                _convergence.Add(new Tuple<int, double, double>(epoch, sampleErr, outOfSampleErr));
                #endregion

                #region Gradient and increnemt
                Tuple<Matrix, Vector>[] gradient = GlobalGradients(learningX, learningY),
                    direction = BuildDirection(gradient, learningRate);

                _descentDirections.Add(direction);

                for (int l = 0; l < _network.LayerCount; l++)
                    _network[l].Increment(direction[l].Item1, direction[l].Item2);
                #endregion
            }

            /*List<Vector> projectedY = _network.Process(learningX);
            double testErr = _errorFunction.Function(projectedY, learningY),
                validationErr = _errorFunction.Function(_network.Process(validationX), validationY);*/
        }

        public void TrainBackPropagationMomentum(List<Vector> learningX, List<Vector> learningY,
            List<Vector> validationX, List<Vector> validationY,
            double learningRate, double momentum,
            int epochs)
        {
            _convergence = new List<Tuple<int, double, double>>();
            _descentDirections = new List<Tuple<Matrix, Vector>[]>();
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                #region Calculate Errs
                double sampleErr = _errorFunction.Function(_network.Process(learningX), learningY),
                    outOfSampleErr = _errorFunction.Function(_network.Process(validationX), validationY);
                _convergence.Add(new Tuple<int, double, double>(epoch, sampleErr, outOfSampleErr));
                #endregion

                #region Gradient and increnemt
                Tuple<Matrix, Vector>[] gradient = GlobalGradients(learningX, learningY),
                    direction = _descentDirections.Count != 0 ? BuildDirection(gradient, learningRate, _descentDirections.Last(), momentum) : BuildDirection(gradient, learningRate);

                _descentDirections.Add(direction);

                for (int l = 0; l < _network.LayerCount; l++)
                    _network[l].Increment(direction[l].Item1, direction[l].Item2);
                #endregion
            }

            /*List<Vector> projectedY = _network.Process(learningX);
            double testErr = _errorFunction.Function(projectedY, learningY),
                validationErr = _errorFunction.Function(_network.Process(validationX), validationY);*/
        }

        public List<Tuple<int, double, double>> Convergence
        {
            get { return _convergence; }
        }

        #region Calculate gradient

        private static Tuple<Matrix, Vector> LayerGradient(Vector inputData, Layer layer, Vector upperGradient)
        {
            return new Tuple<Matrix, Vector>(upperGradient * inputData, upperGradient.Clone);
        }

        private Tuple<Matrix, Vector>[] SampleGradients(Vector X, Vector Y)
        {
            Vector projectedY = _network.Process(X);

            Vector[] delta = new Vector[_network.LayerCount];
            Tuple<Matrix, Vector>[] gradients = new Tuple<Matrix, Vector>[_network.LayerCount];
            int L = _network.LayerCount - 1;
            Layer outputLayer = _network[L];

            #region Outer gradient
            Vector gradient = _errorFunction.Gradient(projectedY, Y);
            #endregion

            delta[L] = Vector.Hadamard(Vector.Apply(outputLayer.Z, outputLayer.Function.Derivative), gradient);
            gradients[L] = LayerGradient(_network[L - 1].A, outputLayer, delta[L]);

            for (int l = L - 1; l >= 0; l--)
            {
                Layer layer = _network[l];
                delta[l] = Vector.Hadamard(Vector.Apply(layer.Z, layer.Function.Derivative), _network[l + 1].Weights.Transpose * delta[l + 1]);
                gradients[l] = l == 0 ? LayerGradient(X, layer, delta[l]) : LayerGradient(_network[l - 1].A, layer, delta[l]);
            }

            return gradients;
        }

        private Tuple<Matrix, Vector>[] GlobalGradients(List<Vector> X, List<Vector> Y)
        {
            int n = X.Count;

            #region Collect gradients for the sample
            Tuple<Matrix, Vector>[][] allGradients = new Tuple<Matrix, Vector>[n][];
            for (int i = 0; i < n; i++)
                allGradients[i] = SampleGradients(X[i], Y[i]);
            #endregion

            #region Calculate the average gradient for each layer
            Tuple<Matrix, Vector>[] aggregated = new Tuple<Matrix, Vector>[_network.LayerCount];
            for (int l = 0; l < _network.LayerCount; l++)
            {
                Matrix w = allGradients[0][l].Item1;
                Vector b = allGradients[0][l].Item2;

                for (int i = 1; i < n; i++)
                {
                    w += allGradients[i][l].Item1;
                    b += allGradients[i][l].Item2;
                }
                aggregated[l] = new Tuple<Matrix, Vector>(w / n, b / n);
            }
            #endregion

            return aggregated;
        }


        private Tuple<Matrix, Vector>[] BuildDirection(Tuple<Matrix, Vector>[] gradient, double factor)
        {
            Tuple<Matrix, Vector>[] direction = new Tuple<Matrix, Vector>[gradient.Length];
            for (int i = 0; i < gradient.Length; i++)
                direction[i] = new Tuple<Matrix, Vector>(gradient[i].Item1 * (-factor), gradient[i].Item2 * (-factor));
            return direction;
        }

        private Tuple<Matrix, Vector>[] BuildDirection(Tuple<Matrix, Vector>[] gradient, double factor, Tuple<Matrix, Vector>[] previousDirection, double momentum)
        {
            Tuple<Matrix, Vector>[] direction = new Tuple<Matrix, Vector>[gradient.Length];
            for (int i = 0; i < gradient.Length; i++)
                direction[i] = new Tuple<Matrix, Vector>(Matrix.LinearCombination(-factor, gradient[i].Item1, momentum, previousDirection[i].Item1), Vector.LinearCombination(-factor, gradient[i].Item2, momentum, previousDirection[i].Item2));
            return direction;
        }

        #endregion
    }
}

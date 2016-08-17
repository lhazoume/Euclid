using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid.Analytics.NeuralNetworks.FeedForward
{
    public class Trainer
    {
        private Perceptron _network;
        private IErrorFunction _errorFunction;
        private List<Tuple<int, double, double>> _convergence;

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

                for (int l = 0; l < _network.LayerCount; l++)
                    _network[l].Increment(direction[l].Item1, direction[l].Item2);
                #endregion
            }
        }

        public void TrainBackPropagationMomentum(List<Vector> learningX, List<Vector> learningY,
            List<Vector> validationX, List<Vector> validationY,
            double learningRate, double momentum,
            int epochs)
        {
            _convergence = new List<Tuple<int, double, double>>();
            List<Tuple<Matrix, Vector>[]> descentDirections = new List<Tuple<Matrix, Vector>[]>();
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                #region Calculate Errs
                double sampleErr = _errorFunction.Function(_network.Process(learningX), learningY),
                    outOfSampleErr = _errorFunction.Function(_network.Process(validationX), validationY);
                _convergence.Add(new Tuple<int, double, double>(epoch, sampleErr, outOfSampleErr));
                #endregion

                #region Gradient and increnemt
                Tuple<Matrix, Vector>[] gradient = GlobalGradients(learningX, learningY),
                    direction = descentDirections.Count != 0 ? BuildDirection(gradient, learningRate, descentDirections.Last(), momentum) : BuildDirection(gradient, learningRate);

                descentDirections.Add(direction);

                for (int l = 0; l < _network.LayerCount; l++)
                    _network[l].Increment(direction[l].Item1, direction[l].Item2);
                #endregion
            }
        }

        public void TrainResilientPropagation(List<Vector> learningX, List<Vector> learningY,
            List<Vector> validationX, List<Vector> validationY,
            double etaPlus, double etaMinus,
            int epochs)
        {
            _convergence = new List<Tuple<int, double, double>>();
            List<Tuple<Matrix, Vector>[]> gradients = new List<Tuple<Matrix, Vector>[]>(),
                deltas = new List<Tuple<Matrix, Vector>[]>();

            deltas.Add(Empty(_network, 0.01));
            gradients.Add(Empty(_network, 0));

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                #region Calculate Errs
                double sampleErr = _errorFunction.Function(_network.Process(learningX), learningY),
                    outOfSampleErr = _errorFunction.Function(_network.Process(validationX), validationY);
                _convergence.Add(new Tuple<int, double, double>(epoch, sampleErr, outOfSampleErr));
                #endregion

                #region Gradient
                Tuple<Matrix, Vector>[] gradient = GlobalGradients(learningX, learningY);
                gradients.Add(gradient);
                #endregion

                #region Calculate Delta and increment
                Tuple<Matrix, Vector>[] delta = Empty(_network, 0),
                    increment = Empty(_network, 0);
                for (int l = 0; l < gradient.Length; l++)
                {
                    #region Deltas

                    #region Matrix
                    for (int k = 0; k < delta[l].Item1.Size; k++)
                    {
                        double previousDelta = deltas[deltas.Count - 1][l].Item1[k],
                            previousGradient = gradients[gradients.Count - 2][l].Item1[k],
                            currentGradient = gradient[l].Item1[k];
                        int sign = Math.Sign(previousGradient * currentGradient);
                        delta[l].Item1[k] = (sign > 0 ? etaPlus : (sign < 0 ? etaMinus : 1)) * previousDelta;
                    }
                    #endregion

                    #region Vector
                    for (int k = 0; k < delta[l].Item2.Size; k++)
                    {
                        double previousDelta = deltas[deltas.Count - 1][l].Item2[k],
                            previousGradient = gradients[gradients.Count - 2][l].Item2[k],
                            currentGradient = gradient[l].Item2[k];
                        int sign = Math.Sign(previousGradient * currentGradient);
                        delta[l].Item2[k] = (sign > 0 ? etaPlus : (sign < 0 ? etaMinus : 1)) * previousDelta;
                    }
                    #endregion

                    #endregion

                    #region Increment

                    #region Matrix
                    for (int k = 0; k < delta[l].Item1.Size; k++)
                        increment[l].Item1[k] = -Math.Sign(gradient[l].Item1[k]) * delta[l].Item1[k];
                    #endregion

                    #region Vector
                    for (int k = 0; k < delta[l].Item2.Size; k++)
                        increment[l].Item2[k] = -Math.Sign(gradient[l].Item2[k]) * delta[l].Item2[k];
                    #endregion

                    #endregion
                }
                deltas.Add(delta);
                #endregion

                #region Perform Incrementation
                for (int l = 0; l < _network.LayerCount; l++)
                    _network[l].Increment(increment[l].Item1, increment[l].Item2);
                #endregion
            }
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
            //for (int l = 0; l < _network.LayerCount; l++)
            Parallel.For(0, _network.LayerCount, l =>
            {
                Matrix w = allGradients[0][l].Item1;
                Vector b = allGradients[0][l].Item2;

                for (int i = 1; i < n; i++)
                {
                    w += allGradients[i][l].Item1;
                    b += allGradients[i][l].Item2;
                }

                aggregated[l] = new Tuple<Matrix, Vector>(w / n, b / n);
            });
            #endregion

            return aggregated;
        }

        #endregion

        #region Services

        private static Tuple<Matrix, Vector>[] BuildDirection(Tuple<Matrix, Vector>[] gradient, double factor)
        {
            Tuple<Matrix, Vector>[] direction = new Tuple<Matrix, Vector>[gradient.Length];
            for (int i = 0; i < gradient.Length; i++)
                direction[i] = new Tuple<Matrix, Vector>(gradient[i].Item1 * (-factor), gradient[i].Item2 * (-factor));
            return direction;
        }

        private static Tuple<Matrix, Vector>[] BuildDirection(Tuple<Matrix, Vector>[] gradient, double factor, Tuple<Matrix, Vector>[] previousDirection, double momentum)
        {
            Tuple<Matrix, Vector>[] direction = new Tuple<Matrix, Vector>[gradient.Length];
            for (int i = 0; i < gradient.Length; i++)
                direction[i] = new Tuple<Matrix, Vector>(Matrix.LinearCombination(-factor, gradient[i].Item1, momentum, previousDirection[i].Item1), Vector.LinearCombination(-factor, gradient[i].Item2, momentum, previousDirection[i].Item2));
            return direction;
        }

        private static Tuple<Matrix, Vector>[] Empty(Perceptron perceptron, double value)
        {
            Tuple<Matrix, Vector>[] ones = new Tuple<Matrix, Vector>[perceptron.LayerCount];
            for (int i = 0; i < ones.Length; i++)
                ones[i] = new Tuple<Matrix, Vector>(Matrix.Create(perceptron[i].Weights.Rows, perceptron[i].Weights.Columns, value), Vector.Create(perceptron[i].Biases.Size, value));
            return ones;
        }

        #endregion
    }
}

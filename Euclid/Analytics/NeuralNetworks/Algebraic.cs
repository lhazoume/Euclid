using System;

namespace Euclid.Analytics.NeuralNetworks
{
    public class Algebraic : IActivationFunction
    {
        public double Function(double x)
        {
            return x / Math.Sqrt(1 + x * x);
        }
        public double Derivative(double x)
        {
            return Math.Pow(1 + x * x, -1.5);
        }
        public double Max
        {
            get { return +1; }
        }
        public double Min
        {
            get { return -1; }
        }
    }
}

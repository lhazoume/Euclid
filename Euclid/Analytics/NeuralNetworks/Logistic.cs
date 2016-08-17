using System;

namespace Euclid.Analytics.NeuralNetworks
{
    public class Logistic : IActivationFunction
    {
        public double Function(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
        public double Derivative(double x)
        {
            double e = Math.Exp(-x);
            return  e / Math.Pow(1 + e, 2);
        }
        public double Max
        {
            get { return +1; }
        }
        public double Min
        {
            get { return 0; }
        }
    }
}

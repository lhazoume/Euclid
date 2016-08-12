using System;

namespace Euclid.Analytics.NeuralNetworks
{
    public class HyperbolicTan : IActivationFunction
    {
        public double Function(double x)
        {
            double e = Math.Exp(-2 * x);
            return (1 - e) / (1 + e);
        }
        public double Derivative(double x)
        {
            double e = Math.Exp(-2 * x);
            return 4 * e / Math.Pow(1 + e, 2);
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

using Euclid.Serialization;
using System;

namespace Euclid.Analytics.NeuralNetworks
{
    public interface IActivationFunction
    {
        double Function(double x);
        double Derivative(double x);
        double Max { get; }
        double Min { get; }
    }
}

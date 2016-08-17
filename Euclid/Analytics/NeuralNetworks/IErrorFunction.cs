using System.Collections.Generic;

namespace Euclid.Analytics.NeuralNetworks
{
    public interface IErrorFunction
    {
        double Function(Vector x, Vector benchmark);
        double Function(List<Vector> x, List<Vector> y);
        Vector Gradient(Vector x, Vector benchmark);
    }
}

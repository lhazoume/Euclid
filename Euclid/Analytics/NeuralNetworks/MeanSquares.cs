using System.Collections.Generic;

namespace Euclid.Analytics.NeuralNetworks
{
    public class MeanSquares : IErrorFunction
    {
        public double Function(Vector x, Vector benchmark)
        {
            return (x - benchmark).Norm2;
        }

        public double Function(List<Vector> x, List<Vector> y)
        {
            double sum = 0;
            for (int i = 0; i < x.Count; i++)
                sum += (x[i] - y[i]).Norm2;
            return sum;
        }

        public Vector Gradient(Vector x, Vector benchmark)
        {
            return 2 * (x - benchmark);
        }
    }
}

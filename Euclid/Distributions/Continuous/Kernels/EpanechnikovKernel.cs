using System;

namespace Euclid.Distributions.Continuous.Kernels
{
    public class EpanechnikovKernel : IDensityKernel
    {
        public double K(double x)
        {
            if (Math.Abs(x) >= 1) return 0;
            return 0.75 * (1 - x * x);
        }

        public double IntegralK(double x)
        {
            if (x <= -1) return 0;
            if (x >= 1) return 1;
            return 0.75 * x * (1 - x * x / 3) + 0.5;
        }

        public double Variance
        {
            get { return 1.0 / 15.0; }
        }
    }
}

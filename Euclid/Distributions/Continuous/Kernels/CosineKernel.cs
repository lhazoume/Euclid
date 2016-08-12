using System;

namespace Euclid.Distributions.Continuous.Kernels
{
    public class CosineKernel : IDensityKernel
    {
        public double K(double x)
        {
            if (Math.Abs(x) >= 1) return 0;
            return 0.25 * Math.PI * Math.Cos(x * Math.PI * 0.5);
        }

        public double IntegralK(double x)
        {
            if (x <= -1) return 0;
            if (x >= 1) return 1;
            return 0.5 * (1 + Math.Sin(x * Math.PI * 0.5));
        }

        public double Variance
        {
            get { return 1 - 8 * Math.Pow(Math.PI, -2); }
        }
    }
}

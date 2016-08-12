using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Distributions.Continuous.Kernels
{
    public interface IDensityKernel
    {
        double K(double x);
        double IntegralK(double x);
        double Variance { get; }
    }
}

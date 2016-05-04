using System;

namespace Euclid.Solvers
{
    public static class FuncHelper
    {
        private const double STEP = 1e-10;

        public static Func<double, double> Differentiate(this Func<double, double> function)
        {
            return x => (function(x + STEP) - function(x - STEP)) / (2 * STEP);
        }
    }
}

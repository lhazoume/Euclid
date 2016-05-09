using Euclid.Arithmetics;
using System;
using System.Linq;

namespace Euclid.Numerics
{
    public static class Differentiator
    {
        public static Func<double, double> Differentiate(this Func<double, double> function, DifferenceForm form, double step)
        {
            if (function == null) throw new ArgumentNullException("The function should not be null");
            if (step <= 0) throw new ArgumentException("the step should be stricly positive");
            switch (form)
            {
                case DifferenceForm.Forward: return x => (function(x + step) - function(x)) / step;
                case DifferenceForm.Backward: return x => (function(x) - function(x - step)) / step;
                default: return x => (function(x + 0.5 * step) - function(x - 0.5 * step)) / step;
            }
        }

        public static Func<double, double> Differentiate(this Func<double, double> function, int n, DifferenceForm form, double step)
        {
            if (function == null) throw new ArgumentNullException("The function should not be null");
            if (step <= 0) throw new ArgumentException("the step should be stricly positive");
            if (n < 1) throw new ArgumentException("the degree should be strictly positive");

            PascalTriangle pt = new PascalTriangle(n);
            int[] indexes = new int[1 + n];
            for (int i = 0; i <= n; i++) indexes[i] = i;

            switch (form)
            {
                case DifferenceForm.Forward: return x => indexes.Sum(i => (1 - 2 * (i % 2)) * pt[i] * function(x + (n - i) * step)) / Math.Pow(step, n);
                case DifferenceForm.Backward: return x => indexes.Sum(i => (1 - 2 * (i % 2)) * pt[i] * function(x - i * step)) / Math.Pow(step, n);
                default: return x => indexes.Sum(i => (1 - 2 * (i % 2)) * pt[i] * function(x + (n - 2 * i) * step / 2)) / Math.Pow(step, n);
            }
        }
    }
}

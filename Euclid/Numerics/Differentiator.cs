using Euclid.Arithmetics;
using Euclid.Solvers;
using System;
using System.Linq;

namespace Euclid.Numerics
{
    /// <summary>
    /// Numeric differentiation class
    /// </summary>
    public static class Differentiator
    {
        /// <summary> First degree numerical differentiation</summary>
        /// <param name="function">the function to differentiate</param>
        /// <returns>the derivative</returns>
        public static Func<double, double> Differentiate(this Func<double, double> function)
        {
            return Differentiate(function, DifferenceForm.Central, Descents.STEP_EPSILON);
        }

        /// <summary>
        /// First degree numerical differentiation
        /// </summary>
        /// <param name="function">the function to differentiate</param>
        /// <param name="form">the differentiation form</param>
        /// <param name="step">the differentiation step</param>
        /// <returns>the derivative</returns>
        public static Func<double, double> Differentiate(this Func<double, double> function, DifferenceForm form, double step)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (step <= 0) throw new ArgumentException("the step should be stricly positive");
            switch (form)
            {
                case DifferenceForm.Forward: return x => (function(x + step) - function(x)) / step;
                case DifferenceForm.Backward: return x => (function(x) - function(x - step)) / step;
                default: return x => (function(x + 0.5 * step) - function(x - 0.5 * step)) / step;
            }
        }

        /// <summary> High degree numerical differentiation</summary>
        /// <param name="function">the function to differentiate</param>
        /// <param name="n">the degree</param>
        /// <param name="form">the differentiation form</param>
        /// <param name="step">the differentiation degree</param>
        /// <returns>the n-degree derivative</returns>
        public static Func<double, double> Differentiate(this Func<double, double> function, int n, DifferenceForm form, double step)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (step <= 0) throw new ArgumentException("the step should be stricly positive");
            if (n < 1) throw new ArgumentException("the degree should be strictly positive");

            BinomialCoefficients pt = new BinomialCoefficients(n);
            int[] indexes = new int[1 + n];
            for (int i = 0; i <= n; i++) indexes[i] = i;

            switch (form)
            {
                case DifferenceForm.Forward: return x => indexes.Sum(i => (1 - 2 * (i % 2)) * pt[i] * function(x + (n - i) * step)) / Math.Pow(step, n);
                case DifferenceForm.Backward: return x => indexes.Sum(i => (1 - 2 * (i % 2)) * pt[i] * function(x - i * step)) / Math.Pow(step, n);
                default: return x => indexes.Sum(i => (1 - 2 * (i % 2)) * pt[i] * function(x + (n - 2 * i) * step / 2)) / Math.Pow(step, n);
            }
        }

        public static Matrix Hessian(Func<Vector, double> func, Vector center, Vector bump)
        {
            if (bump.Size != center.Size || bump.Data.Any(d => d <= 0))
                throw new ArgumentException("bump sign irrelevant", nameof(bump));
            int n = center.Size;
            Matrix result = Matrix.Create(n, n);
            double refValue = func(center);

            for (int i = 0; i < n; i++)
            {
                Vector iBump = Vector.ExtractIthDimension(bump, i);
                result[i, i] = (func(center + iBump) + func(center - iBump) - 2 * refValue) / Math.Pow(bump[i], 2);

                for (int j = i + 1; j < n; j++)
                {
                    Vector jBump = Vector.ExtractIthDimension(bump, j);
                    result[i, j] = (func(center + iBump + jBump) + func((center - iBump) - jBump)
                        - func((center + iBump) - jBump) - func((center + jBump) - iBump)) / (4 * bump[i] * bump[j]);
                    result[j, i] = result[i, j];
                }
            }
            return result;

        }
    }
}

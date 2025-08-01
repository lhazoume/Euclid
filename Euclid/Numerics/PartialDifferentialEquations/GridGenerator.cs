using System;
using System.Linq;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    public interface IGridGenerator
    {
        /// <summary>
        /// Generates an array of grid points between a minimum and maximum value.
        /// </summary>
        /// <param name="min">The minimum value of the grid.</param>
        /// <param name="max">The maximum value of the grid.</param>
        /// <param name="count">The number of points in the grid.</param>
        /// <returns>An array of doubles representing the grid points.</returns>
        double[] Generate(double min, double max, int count);
    }
    public class UniformGridGenerator : IGridGenerator
    {
        public double[] Generate(double min, double max, int count)
        {
            if (count < 2)
                return count == 1 ? new[] { (min + max) / 2.0 } : Array.Empty<double>();

            return Enumerable.Range(0, count).Select(i => min + i * (max - min) / (count - 1)).ToArray();
        }
    }

    public class SinhGridGenerator : IGridGenerator
    {
        private readonly double _concentrationPoint;
        private readonly double _concentrationFactor; // A small value means high concentration

        public SinhGridGenerator(double concentrationPoint, double concentrationFactor = 1.0)
        {
            if (concentrationFactor <= 0)
                throw new ArgumentException("Concentration factor must be positive.", nameof(concentrationFactor));

            _concentrationPoint = concentrationPoint;
            _concentrationFactor = concentrationFactor;
        }

        public double[] Generate(double min, double max, int count)
        {
            if (count < 2)
                return count == 1 ? new[] { (min + max) / 2.0 } : Array.Empty<double>();

            var points = new double[count];
            double dxi = 1.0 / (count - 1);

            double c = Arcsinh((min - _concentrationPoint) / _concentrationFactor);
            double d = Arcsinh((max - _concentrationPoint) / _concentrationFactor);

            for (int i = 0; i < count; i++)
            {
                double xi = i * dxi;
                points[i] = _concentrationPoint + _concentrationFactor * Math.Sinh(c * (1 - xi) + d * xi);
            }
            return points;
        }
        static double Arcsinh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x + 1));
        }
    }
    public class LogSpaceGridGenerator : IGridGenerator
    {
        public double[] Generate(double min, double max, int count)
        {
            if (min <= 0 || max <= 0)
                throw new ArgumentException("Log-space grid requires positive min and max values.");

            if (count < 2)
                return count == 1 ? new[] { Math.Sqrt(min * max) } : Array.Empty<double>();

            double logMin = Math.Log(min);
            double logMax = Math.Log(max);

            return Enumerable.Range(0, count).Select(i => logMin + i * (logMax - logMin) / (count - 1)).Select(logPoint => Math.Exp(logPoint)).ToArray();
        }
    }
    public class PowerGridGenerator : IGridGenerator
    {
        private readonly double _power;
        public PowerGridGenerator(double power)
        {
            if (power <= 0)
                throw new ArgumentException("Power must be positive.", nameof(power));
            _power = power;
        }

        public double[] Generate(double min, double max, int count)
        {
            if (count < 2)
                return count == 1 ? new[] { (min + max) / 2.0 } : Array.Empty<double>();

            double[] points = new double[count];
            double range = max - min;

            for (int i = 0; i < count; i++)
            {
                // Create a uniform point 'u' in [0, 1]
                double u = (double)i / (count - 1);
                double transformed_u = Math.Pow(u, _power);
                points[i] = min + transformed_u * range; // Map back to the [min, max] interval
            }
            return points;
        }
    }
}
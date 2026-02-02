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
        /// <param name="max">The maximum value of the grid .</param>
        /// <param name="count">The number of points in the grid.</param>
        /// <returns>An array of doubles representing the grid points.</returns>
        double[] Generate(double min, double max, int count);
    }

    // Grilles "classiques"
    public class UniformGridGenerator : IGridGenerator
    {
        public double[] Generate(double min, double max, int count)
        {
            if (count <= 0) return Array.Empty<double>();
            if (count == 1) return new[] { 0.5 * (min + max) };

            return Enumerable.Range(0, count).Select(i => min + i * (max - min) / (count - 1)).ToArray();
        }
    }

    public class SinhGridGenerator : IGridGenerator
    {
        private readonly double _concentrationPoint;
        private readonly double _concentrationFactor;

        public SinhGridGenerator(double concentrationPoint, double concentrationFactor = 1.0)
        {
            if (concentrationFactor <= 0)
                throw new ArgumentException("Concentration factor must be positive.", nameof(concentrationFactor));

            _concentrationPoint = concentrationPoint;
            _concentrationFactor = concentrationFactor;
        }

        public double[] Generate(double min, double max, int count)
        {
            if (count <= 0) return Array.Empty<double>();
            if (count == 1) return new[] { 0.5 * (min + max) };

            double[] points = new double[count];
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

        static double Arcsinh(double x) => Math.Log(x + Math.Sqrt(x * x + 1));
    }


    // Grille AGNOSTIQUE (quantiles)
    public class QuantileGridGenerator : IGridGenerator
    {
        private readonly Func<double, double> _inverseCdf;
        private readonly double _minProbability;
        private readonly double _maxProbability;


        public QuantileGridGenerator(Func<double, double> inverseCdf, double minProbability = 0.001, double maxProbability = 0.999)
        {
            if (minProbability < 0 || minProbability >= 1)
                throw new ArgumentOutOfRangeException(nameof(minProbability), "Minimum probability must be in [0, 1).");
            if (maxProbability <= minProbability || maxProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(maxProbability), "Maximum probability must be greater than min probability and in (0, 1].");

            _inverseCdf = inverseCdf ?? throw new ArgumentNullException(nameof(inverseCdf));
            _minProbability = minProbability;
            _maxProbability = maxProbability;
        }

        /// <remarks>min et max ignorés.</remarks>
        public double[] Generate(double min, double max, int count)
        {
            if (count <= 0) return Array.Empty<double>();
            if (count == 1) return new[] { _inverseCdf((_minProbability + _maxProbability) * 0.5) };

            double[] points = new double[count];
            double probabilityRange = _maxProbability - _minProbability;
            double probabilityStep = probabilityRange / (count - 1);

            for (int i = 0; i < count; i++)
            {
                double p = _minProbability + i * probabilityStep;
                points[i] = _inverseCdf(p);
            }
            return points;
        }
    }

    public class GbmQuantileLogGridGenerator : IGridGenerator
    {
        private readonly double _F, _sigma, _t, _epsilon;

        public GbmQuantileLogGridGenerator(double F, double sigma, double t, double epsilon)
        {
            if (F <= 0) throw new ArgumentException("F must be > 0.", nameof(F));
            if (sigma < 0) throw new ArgumentException(nameof(sigma));
            if (t <= 0) throw new ArgumentException(nameof(t));
            if (epsilon <= 0 || epsilon >= 0.5) throw new ArgumentException(nameof(epsilon));
            _F = F; _sigma = sigma; _t = t; _epsilon = epsilon;
        }


        public double[] Generate(double min, double max, int count)
        {
            if (count <= 0) return Array.Empty<double>();
            if (count == 1) return new[] { _F };

            int n = Math.Max(1, (count - 1) / 2);
            int leftCount = n;
            int rightCount = count - leftCount - 1; // garantit la taille exacte "count"

            double gamma = _sigma * Math.Sqrt(_t) * Fn.InvPhi(1 - _epsilon);
            double delta = (n > 0) ? (gamma / n) : 0.0;

            double[] xs = new double[count];
            int centerIdx = leftCount;
            xs[centerIdx] = _F;

            // indices décroissants
            for (int i = 1; i <= leftCount; i++)
            {
                double f = Math.Exp(i * delta);
                xs[centerIdx - i] = _F / f;
            }
            // indice croissants
            for (int i = 1; i <= rightCount; i++)
            {
                double f = Math.Exp(i * delta);
                xs[centerIdx + i] = _F * f;
            }
            return xs;
        }
    }

    public class AbmQuantileLinearGridGenerator : IGridGenerator
    {
        private readonly double _F, _sigma, _t, _epsilon;

        public AbmQuantileLinearGridGenerator(double F, double sigma, double t, double epsilon)
        {
            if (t <= 0) throw new ArgumentException(nameof(t));
            if (sigma < 0) throw new ArgumentException(nameof(sigma));
            if (epsilon <= 0 || epsilon >= 0.5) throw new ArgumentException(nameof(epsilon));
            _F = F; _sigma = sigma; _t = t; _epsilon = epsilon;
        }

        public double[] Generate(double min, double max, int count)
        {
            if (count <= 0) return Array.Empty<double>();
            if (count == 1) return new[] { _F };

            int n = Math.Max(1, (count - 1) / 2);
            int leftCount = n;
            int rightCount = count - leftCount - 1;

            double gamma = _sigma * Math.Sqrt(_t) * Fn.InvPhi(1 - _epsilon);
            double delta = (n > 0) ? (gamma / n) : 0.0;

            double[] xs = new double[count];
            int centerIdx = leftCount;
            xs[centerIdx] = _F;

            for (int i = 1; i <= leftCount; i++)
                xs[centerIdx - i] = _F - i * delta;

            for (int i = 1; i <= rightCount; i++)
                xs[centerIdx + i] = _F + i * delta;

            return xs;
        }
    }


    public class IncludePointGrid : IGridGenerator
    {
        private readonly IGridGenerator _inner;
        private readonly double _point;

        public IncludePointGrid(IGridGenerator inner, double point)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _point = point;
        }

        public double[] Generate(double min, double max, int count)
        {
            double[] g = _inner.Generate(min, max, count) ?? Array.Empty<double>();
            bool exists = g.Any(x => x == _point);

            if (exists) return g.OrderBy(x => x).ToArray();

            double[] extended = g.Concat(new[] { _point }).OrderBy(x => x).ToArray();
            return extended;
        }
    }

}

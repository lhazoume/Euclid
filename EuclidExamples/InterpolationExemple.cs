using System;
using System.Collections.Generic;
using System.Linq;
using Euclid.Interpolations.Interpolator1D;


public class InterpolationExample
{
    private static readonly double[] xDataPoints = { -3.0, -2.0, -1.5, -1.0, -0.5, 0.0, 0.5, 1.0, 1.5, 2.0, 3.0 };
    private static readonly double[] queryPoints = xDataPoints
        .Zip(xDataPoints.Skip(1), (a, b) => (a + b) / 2)
        .ToArray();

    private static readonly Dictionary<string, Func<double, double>> TestFunctions = new Dictionary<string, Func<double, double>>
        {
            { "sin",           Math.Sin },
            { "cos",           Math.Cos },
            { "exp",           Math.Exp },
            { "polynomiale_2", t => 3 * t * t + 2 }
        };

    public static void CubicSplineExample()
    {
        Console.WriteLine("=== CUBIC SPLINE INTERPOLATION TESTS ===\n");
        foreach (var kv in TestFunctions)
        {
            string name = kv.Key;
            Func<double, double> func = kv.Value;
            // Sample the function
            double[] yData = xDataPoints.Select(func).ToArray();

            CubicSpline spline = new CubicSpline(allowExtrapolation: true, bcType: BoundaryType.Clamped);
            spline.SetData(xDataPoints, yData);

            Console.WriteLine($"-- Fonction: {name} --");
            foreach (double xi in queryPoints)
            {
                double yiTrue = func(xi);
                double yiInterp = spline.ValueAt(xi);
                Console.WriteLine($"x = {xi,6:F3} | vrai = {yiTrue,8:F5} | spline = {yiInterp,8:F5} | erreur = {Math.Abs(yiInterp - yiTrue),8:F5}");
            }
            Console.WriteLine();
        }
        Console.WriteLine("=== FIN CUBIC SPLINE TESTS ===\n");
    }

    public static void HymanExample()
    {
        Console.WriteLine("=== HYMAN SPLINE INTERPOLATION TEST ===\n");
        // Fonction à tester pour Hyman
        Func<double, double> funcToTest = x => Math.Exp(-x * x);

        // Sample la fonction
        double[] yData = xDataPoints.Select(funcToTest).ToArray();
        var hyman = new Hyman(allowExtrapolation: false);
        hyman.SetData(xDataPoints, yData);

        Console.WriteLine("Fonction: exp(-x^2)");
        foreach (double xi in queryPoints)
        {
            double yiTrue = funcToTest(xi);
            double yiInterp = hyman.ValueAt(xi);
            Console.WriteLine($"x = {xi,6:F3} | vrai = {yiTrue,8:F5} | hyman = {yiInterp,8:F5} | erreur = {Math.Abs(yiInterp - yiTrue),8:F5}");
        }
        Console.WriteLine("=== FIN HYMAN SPLINE TEST ===\n");
    }
}

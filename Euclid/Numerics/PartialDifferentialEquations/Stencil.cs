using System;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    /// <summary>  Provides static methods for computing finite difference stencil coefficients for partial derivatives on non-uniform grids in two dimensions. Includes support for first and second order derivatives,
    /// as well as mixed derivatives (cross-diffusion).
    /// </summary>
    public static class Stencil
    {
        private const double EPS = 1e-15;
        /// <summary>
        /// Computes the coefficients for the centered second-order partial derivative in the x direction on a non-uniform grid.
        /// </summary>
        /// <param name="hxMinus">Grid spacing to the left of the central point in the x direction.</param>
        /// <param name="hxPlus">Grid spacing to the right of the central point in the x direction.</param>
        /// <returns>
        /// A tuple containing the coefficients for the points (i-1, j), (i, j), (i+1, j):
        /// (minus: (i-1, j), center: (i, j), plus: (i+1, j)).
        /// </returns>
        public static (double minus, double center, double plus) DxxCoeffs(double hxMinus, double hxPlus)
        {
            ValidateSteps(hxMinus, hxPlus, nameof(hxMinus), nameof(hxPlus));
            double minus = 2.0 / (hxMinus * (hxMinus + hxPlus));
            double center = -2.0 / (hxMinus * hxPlus);
            double plus = 2.0 / (hxPlus * (hxMinus + hxPlus));
            return (minus, center, plus);
        }

        /// <summary>
        /// Computes the coefficients for the centered first-order partial derivative in the x direction on a non-uniform grid.
        /// </summary>
        /// <param name="hxMinus">Grid spacing to the left of the central point in the x direction.</param>
        /// <param name="hxPlus">Grid spacing to the right of the central point in the x direction.</param>
        /// <param name="driftX">Drift coefficient in the x direction.</param>
        /// <returns>
        /// A tuple containing the coefficients for the points (i-1, j), (i, j), (i+1, j):
        /// (minus: (i-1, j), center: (i, j), plus: (i+1, j)).
        /// </returns>
        public static (double minus, double center, double plus) DxCoeffs(double hxMinus, double hxPlus, double driftX)
        {
            ValidateSteps(hxMinus, hxPlus, nameof(hxMinus), nameof(hxPlus));
            double minus = driftX * (-hxPlus / (hxMinus * (hxMinus + hxPlus)));
            double center = driftX * ((hxPlus - hxMinus) / (hxMinus * hxPlus));
            double plus = driftX * (hxMinus / (hxPlus * (hxMinus + hxPlus)));
            return (minus, center, plus);
        }

        /// <summary>
        /// Computes the coefficients for the centered second-order partial derivative in the y direction on a non-uniform grid.
        /// </summary>
        /// <param name="hyMinus">Grid spacing below the central point in the y direction.</param>
        /// <param name="hyPlus">Grid spacing above the central point in the y direction.</param>
        /// <returns>
        /// A tuple containing the coefficients for the points (i, j-1), (i, j), (i, j+1):
        /// (minus: (i, j-1), center: (i, j), plus: (i, j+1)).
        /// </returns>
        public static (double minus, double center, double plus) DyyCoeffs(double hyMinus, double hyPlus)
        {
            ValidateSteps(hyMinus, hyPlus, nameof(hyMinus), nameof(hyPlus));
            double minus = 2.0 / (hyMinus * (hyMinus + hyPlus));
            double center = -2.0 / (hyMinus * hyPlus);
            double plus = 2.0 / (hyPlus * (hyMinus + hyPlus));
            return (minus, center, plus);
        }

        /// <summary>
        /// Computes the coefficients for the centered first-order partial derivative in the y direction on a non-uniform grid.
        /// </summary>
        /// <param name="hyMinus">Grid spacing below the central point in the y direction.</param>
        /// <param name="hyPlus">Grid spacing above the central point in the y direction.</param>
        /// <param name="driftY">Drift coefficient in the y direction.</param>
        /// <returns>
        /// A tuple containing the coefficients for the points (i, j-1), (i, j), (i, j+1):
        /// (minus: (i, j-1), center: (i, j), plus: (i, j+1)).
        /// </returns>
        public static (double minus, double center, double plus) DyCoeffs(double hyMinus, double hyPlus, double driftY)
        {
            ValidateSteps(hyMinus, hyPlus, nameof(hyMinus), nameof(hyPlus));
            double minus = driftY * (-hyPlus / (hyMinus * (hyMinus + hyPlus)));
            double center = driftY * ((hyPlus - hyMinus) / (hyMinus * hyPlus));
            double plus = driftY * (hyMinus / (hyPlus * (hyMinus + hyPlus)));
            return (minus, center, plus);
        }

        /// <summary>
        /// Computes the coefficients for the mixed second-order derivative (cross-diffusion) stencil
        /// on a non-uniform grid. The coefficients correspond to the four corners around the central point.
        /// </summary>
        /// <param name="hxMinus">Grid spacing to the left of the central point in the x-direction.</param>
        /// <param name="hxPlus">Grid spacing to the right of the central point in the x-direction.</param>
        /// <param name="hyMinus">Grid spacing below the central point in the y-direction.</param>
        /// <param name="hyPlus">Grid spacing above the central point in the y-direction.</param>
        /// <param name="crossDiffusion">The cross-diffusion coefficient.</param>
        /// <returns>
        /// A tuple containing the coefficients for the four corners:
        /// (minusMinus: (i-1, j-1), plusMinus: (i+1, j-1), minusPlus: (i-1, j+1), plusPlus: (i+1, j+1)).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if any grid step is non-positive or if the denominator is too small, which may lead to numerical instability.
        /// </exception>
        public static (double minusMinus, double plusMinus, double minusPlus, double plusPlus)
            DxyCornerCoeffs(double hxMinus, double hxPlus, double hyMinus, double hyPlus, double crossDiffusion)
        {
            ValidateSteps(hxMinus, hxPlus, nameof(hxMinus), nameof(hxPlus));
            ValidateSteps(hyMinus, hyPlus, nameof(hyMinus), nameof(hyPlus));

            double denomX = hxPlus + hxMinus;
            double denomY = hyPlus + hyMinus;
            if (Math.Abs(denomX) < EPS || Math.Abs(denomY) < EPS)
                throw new ArgumentException("Degenerate mixed derivative denominator (hxPlus+hxMinus or hyPlus+hyMinus too small).");

            double baseCoeff = crossDiffusion / (denomX * denomY);

            double minusMinus = +baseCoeff; // (i-1, j-1)
            double plusMinus = -baseCoeff; // (i+1, j-1)
            double minusPlus = -baseCoeff; // (i-1, j+1)
            double plusPlus = +baseCoeff; // (i+1, j+1)

            return (minusMinus, plusMinus, minusPlus, plusPlus);
        }

        #region Helpers

        private static void ValidateSteps(double minusStep, double plusStep, string minusName, string plusName)
        {
            if (minusStep <= 0.0)
                throw new ArgumentException($"{minusName} must be strictly positive.");
            if (plusStep <= 0.0)
                throw new ArgumentException($"{plusName} must be strictly positive.");
            if (Math.Abs(minusStep) < EPS || Math.Abs(plusStep) < EPS)
                throw new ArgumentException("Grid steps are too small and may lead to numerical instability.");
        }

        #endregion
    }
}

namespace Euclid.Solvers
{
    /// <summary>
    /// Default thresholds for iterative solvers
    /// </summary>
    public static class Descents
    {
        /// <summary>Default absolute tolerance for the solvers </summary>
        public const double ERR_EPSILON = 1e-10;
        /// <summary>Default tolerance for gradients and derivatives </summary>
        public const double GRADIENT_EPSILON = 1e-10;
        /// <summary>Default increment </summary>
        public const double STEP_EPSILON = 1e-8;
    }
}

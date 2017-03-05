namespace Euclid.Solvers
{
    /// <summary>
    /// The status of a solver
    /// </summary>
    public enum SolverStatus
    {
        /// <summary> The solver did not run </summary>
        NotRan = 0,
        /// <summary> The solver stopped because the error value reached it's target</summary>
        FunctionConvergence = 1,
        /// <summary> The solver stopped because the error gradient reached it's target</summary>
        GradientConvergence = 2,
        /// <summary> The solver exceeded its maximum iterations </summary>
        IterationExceeded = 3,
        /// <summary> The solver exceeded the number of evaluation without substantial change in error value</summary>
        StationaryFunction = 4,
        /// <summary> The function is not adapted to the solver's requirements </summary>
        BadFunction = 5,
        /// <summary> The solver diverged </summary>
        Diverged = 6
    }
}

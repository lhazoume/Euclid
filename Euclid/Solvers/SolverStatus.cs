namespace Euclid.Solvers
{
    /// <summary>
    /// The status of a solver
    /// </summary>
    public enum SolverStatus
    {
        /// <summary> The solver did not run </summary>
        NotRan = 0,
        /// <summary> The solver ran as expected </summary>
        Normal = 1,
        /// <summary> The solver exceeded its maximum iterations </summary>
        IterationExceeded = 2,
        /// <summary> The function is not adapted to the solver's requirements </summary>
        BadFunction = 3,
        /// <summary> The solver diverged </summary>
        Diverged = 4
    }
}

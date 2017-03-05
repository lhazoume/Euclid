using System;
using System.Collections.Generic;

namespace Euclid.Solvers
{
    /// <summary>
    /// Interface for single variable solvers
    /// </summary>
    public interface ISingleVariableSolver
    {
        /// <summary>
        /// Gets and sets the function to solve for
        /// </summary>
        Func<double, double> Function { get; set; }

        /// <summary>
        /// Gets the solver's status
        /// </summary>
        SolverStatus Status { get; }

        /// <summary>
        /// Gets and sets the maximum number of iterations
        /// </summary>
        int MaxIterations { get; set; }

        /// <summary>
        /// Gets the result
        /// </summary>
        double Result { get; }

        /// <summary>
        /// Gets the final error
        /// </summary>
        double Error { get; }

        /// <summary>
        /// Gets the convergence path
        /// </summary>
        List<Tuple<double, double>> Convergence { get; }

        /// <summary>
        /// Solves for the given function, parameters and initial conditions, target set @ 0
        /// </summary>
        void Solve();

        /// <summary>
        /// Solves for the given function, parameters, initial conditions and target
        /// </summary>
        /// <param name="target">the target value for the function</param>
        void Solve(double target);
    }
}

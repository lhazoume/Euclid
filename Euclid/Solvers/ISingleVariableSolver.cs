using System;

namespace Euclid.Solvers
{
    public interface ISingleVariableSolver
    {
        Func<double, double> Function { get; set; }

        SolverStatus Status { get; }
        int Iterations { get; }
        double Result { get; }
        double Error { get; }

        void Solve();
        void Solve(double target);
    }
}

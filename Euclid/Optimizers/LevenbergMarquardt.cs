using Euclid.Numerics;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Optimizers
{
    /// <summary>
    /// Nonlinear least‑squares optimizer based on the Levenberg–Marquardt algorithm.
    /// </summary>
    public class LevenbergMarquardt
    {
        #region Declarations
        private readonly Func<Vector, Vector> _residuals;
        private readonly Func<Vector, Matrix> _jacobian; 
        private readonly int _maxIter;
        private readonly int _maxStaticIter;
        private readonly double _tau;
        private readonly double _gradientTolerance;
        private readonly double _functionTolerance;
        private readonly double _vInit;
        private readonly double _stepSize; // Step size for numerical Jacobian
        private SolverStatus _status = SolverStatus.NotRan;
        private Vector _result;      // current solution
        private double _error;
        private readonly OptimizationType _optimizationType;
        private readonly DifferenceForm _differenceForm = DifferenceForm.Central;
        private readonly int _sign;
        private readonly List<double> _errors = new List<double>();
        private readonly List<double> _lambdas = new List<double>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LevenbergMarquardt"/> class.
        /// </summary>
        /// <param name="initialGuess">Initial parameter estimate.</param>
        /// <param name="residuals">Function computing the residual vector r(x).</param>
        /// <param name="jacobian">Function computing the Jacobian matrix J(x) of the residuals. If null, finite differences will be used.</param>
        /// <param name="optimizationType">Optimization type (minimization or maximization).</param>
        /// <param name="scheme">Finite difference scheme for numerical Jacobian (forward, backward, or central).</param>
        /// <param name="maxIter">Maximum number of iterations.</param>
         /// <param name="maxStaticIter">Maximum number of static iterations allowed without improvement.</param>

        /// <param name="tau">Initial factor for the Levenberg‑Marquardt parameter (lambda).</param>
        /// <param name="gradientTolerance">Tolerance for the gradient norm. Optimization stops if ||J^T r|| ≤ gradientTolerance.</param>
        /// <param name="functionTolerance">Tolerance for the function value. Optimization stops if ||r|| ≤ functionTolerance.</param>
        /// <param name="vInit">Initial factor for increasing lmabda when a step is rejected.</param>
        /// <param name="stepSize">Relative step size for finite differences in numerical Jacobian.</param>
        public LevenbergMarquardt(
            Vector initialGuess,
            Func<Vector, Vector> residuals,
            Func<Vector, Matrix> jacobian = null,
            OptimizationType optimizationType = OptimizationType.Min,
            DifferenceForm scheme = DifferenceForm.Central,
            int maxIter = 100,
            int maxStaticIter = 50,
            double tau = 1e-3,
            double gradientTolerance = Descents.GRADIENT_EPSILON,
            double functionTolerance = 1e-8,
            double vInit = 2.0,
            double stepSize = 1e-8)
        {
            _result = initialGuess ?? throw new ArgumentNullException(nameof(initialGuess));
            _residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
            _jacobian = jacobian;
            _differenceForm = scheme;

            _optimizationType = optimizationType;
            _sign = _optimizationType == OptimizationType.Min ? -1 : 1;   

            _maxIter = maxIter;
            _maxStaticIter = maxStaticIter;
            _tau = tau;
            _gradientTolerance = gradientTolerance;
            _functionTolerance = functionTolerance;
            _vInit = vInit;
            _stepSize = stepSize;
        }

        #endregion

        #region Properties 
        /// <summary>
        /// Gets the current status of the optimizer.
        /// </summary>
        public SolverStatus Status => _status;
        /// <summary>
        /// Gets the current result of the optimization.
        /// </summary>
        public Vector Result => _result;
        /// <summary>
        /// Gets the current error value.
        /// </summary>
        public double Error => _error;
        /// <summary>
        /// Gets the list of error values during the optimization process.
        /// </summary>
        public IEnumerable<double> Errors => _errors;
        /// <summary>
        /// Gets the list of lambda values used during the optimization process.
        /// </summary>
        public IEnumerable<double> Lambdas => _lambdas;
        /// <summary>Gets the optimization type</summary>
        public OptimizationType OptimizationType => _optimizationType;

        #endregion
        #region Methods
        #region Numerical Jacobian
        /// <summary>
        /// Computes the Jacobian numerically using forward finite differences.
        /// </summary>
        /// <param name="function">The residual function.</param>
        /// <param name="x">The point at which to evaluate the Jacobian.</param>
        /// <param name="increment">The relative step size for finite differences.</param>
        /// <param name="scheme">The finite difference scheme to use (forward, backward, or central).</param>
        /// <returns>The estimated Jacobian matrix.</returns>
        private static Matrix NumericalJacobian(Func<Vector, Vector> function, Vector x,double increment, DifferenceForm scheme)
        {
            if (increment <= 0) throw new ArgumentOutOfRangeException(nameof(increment), "eps doit être strictement positif.");

            int n = x.Size;
            Vector f0 = function(x);
            int m = f0.Size;
            Matrix J = Matrix.Create(m, n);

            for (int j = 0; j < n; j++)
            {
                double xj0 = x[j];

                Vector rPlus = null, rMinus = null;

                if (scheme == DifferenceForm.Forward || scheme == DifferenceForm.Central)
                {
                    x[j] = xj0 + increment;
                    rPlus = function(x);
                }
                if (scheme == DifferenceForm.Backward || scheme == DifferenceForm.Central)
                {
                    x[j] = xj0 - increment;
                    rMinus = function(x);
                }
                x[j] = xj0;

                for (int i = 0; i < m; i++)
                {
                    if (scheme == DifferenceForm.Forward)
                    {
                        J[i, j] = (rPlus[i] - f0[i]) / increment;
                    }
                    else if (scheme == DifferenceForm.Backward)
                    {
                        J[i, j] = (f0[i] - rMinus[i]) / increment;
                    }
                    else
                    {
                        J[i, j] = (rPlus[i] - rMinus[i]) / (2 * increment);
                    }
                }
            }

            return J;
        }

        #endregion

        #region Optimisation
        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm from J.Moré 
        /// </summary>
        public void OptimizeAdaptive()
        {
            _errors.Clear();
            _lambdas.Clear();
            _status = SolverStatus.NotRan;

            EndCriteria endCriteria = new EndCriteria(
                maxIterations: _maxIter,
                maxStaticIterations: _maxStaticIter,
                functionEpsilon: _functionTolerance,
                gradientEpsilon: _gradientTolerance
            );

            double lambda =0;
            double v = _vInit;

            while (true)
            {
                Vector residual = _residuals(_result);
                _error = 0.5 * residual.SumOfSquares;
                Matrix jacobian = _jacobian != null ? _jacobian(_result) : NumericalJacobian(_residuals, _result, _stepSize, _differenceForm);
                Vector gradient = jacobian.Transpose * residual;
                Matrix JTJ = Matrix.TransposeBySelf(jacobian);
                if (_errors.Count == 0)
                {
                    double diagMax = JTJ.Rows > 0 ? Enumerable.Range(0, JTJ.Rows).Max(i => Math.Abs(JTJ[i, i])) : 1.0;
                    lambda = _tau * diagMax;
                }

                double gradNorm = gradient.Norm2;
                Matrix A = JTJ + Matrix.CreateIdentityMatrix(JTJ.Rows, JTJ.Columns) * lambda;
                Vector delta = _sign * A.SolveWith(gradient);

                Vector xNew = _result + delta;
                Vector residualNew = _residuals(xNew);
                double errorNew = 0.5 * residualNew.SumOfSquares;

                double predictedReduction = -_sign * 0.5 * Vector.Scalar(delta, lambda * delta - gradient);
                double actualReduction = -_sign *( _error - errorNew);
                double reductionRatio = (predictedReduction > double.Epsilon) ? actualReduction / predictedReduction : -1.0;

                // Moré's rule for updating lambda
                if (reductionRatio > 0 && !double.IsInfinity(errorNew) && !double.IsNaN(errorNew))
                {
                    _result = xNew;
                    _error = errorNew;
                    lambda *= Math.Max(1.0 / 3.0, 1.0 - Math.Pow(2.0 * reductionRatio - 1.0, 3.0));
                    v = _vInit;
                    if (endCriteria.ShouldStop(value: _error, gradient: gradNorm))
                    {
                        _status = endCriteria.Status;
                        return;
                    }
                }
                else
                {
                    lambda *= v;
                    v *= 2.0;
                }
                _errors.Add(_error);
                _lambdas.Add(lambda);
            }
        }

        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm.
        /// </summary>
        public void Optimize()
        {
            _errors.Clear();
            _lambdas.Clear();
            _status = SolverStatus.NotRan;

            EndCriteria endCriteria = new EndCriteria(
                maxIterations: _maxIter,
                maxStaticIterations: _maxStaticIter,
                functionEpsilon: _functionTolerance,
                gradientEpsilon: _gradientTolerance
            );

            double lambda = _tau;
            double v = _vInit;
            while (true)
            {
                Vector residual = _residuals(_result);
                _error = 0.5 * residual.SumOfSquares;
                Matrix jacobian = _jacobian != null ? _jacobian(_result) : NumericalJacobian(_residuals, _result, _stepSize,_differenceForm);
                Vector gradient = jacobian.Transpose * residual;
                Matrix JTJ = Matrix.TransposeBySelf(jacobian);

                Matrix A = JTJ + Matrix.CreateIdentityMatrix(JTJ.Rows, JTJ.Columns) * lambda;
                Vector delta = _sign * A.SolveWith(gradient);
                double gradNorm = gradient.Norm2;

                Vector xNew = _result + delta;
                Vector residualNew = _residuals(xNew);
                double errorNew = 0.5 * residualNew.SumOfSquares;

                double predictedReduction = -_sign * 0.5 * Vector.Scalar(delta, lambda * delta - gradient);
                double actualReduction = -_sign * (_error - errorNew);
                double reductionRatio = (predictedReduction > double.Epsilon) ? actualReduction / predictedReduction : -1;

                if (reductionRatio > 0 && !double.IsInfinity(errorNew) && !double.IsNaN(errorNew))
                {
                    _result = xNew;
                    residual = residualNew;
                    _error = errorNew;
                    lambda /= v;
                    if (endCriteria.ShouldStop(value: _error, gradient: gradNorm))
                    {
                        _status = endCriteria.Status;
                        return;
                    }
                }
                else
                {
                    lambda *= v;
                }
                _errors.Add(_error);
                _lambdas.Add(lambda);
            }
        }

        #endregion
        #endregion
    }
}

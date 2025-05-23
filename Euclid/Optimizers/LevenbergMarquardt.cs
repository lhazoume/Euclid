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
        private readonly Vector _bump;
        private readonly int _maxIter;
        private readonly int _maxStaticIter;
        private readonly double _tau;
        private readonly double _gradientTolerance;
        private readonly double _functionTolerance;
        private readonly double _vInit;
        private Vector _result;
        private double _error;
        private readonly OptimizationType _optimizationType;
        private SolverStatus _status = SolverStatus.NotRan;
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
        /// <param name="bump">Initial step size for finite differences in the Jacobian. If null, a default value will be used.</param>
        /// <param name="optimizationType">Optimization type (minimization or maximization).</param>
        /// <param name="maxIter">Maximum number of iterations.</param>
        /// <param name="maxStaticIter">Maximum number of static iterations allowed without improvement.</param>
        /// <param name="tau">Initial factor for the Levenberg‑Marquardt parameter (lambda).</param>
        /// <param name="gradientTolerance">Tolerance for the gradient norm. Optimization stops if ||J^T r|| ≤ gradientTolerance.</param>
        /// <param name="functionTolerance">Tolerance for the function value. Optimization stops if ||r|| ≤ functionTolerance.</param>
        /// <param name="vInit">Initial factor for increasing lmabda when a step is rejected.</param>
        public LevenbergMarquardt(
            Vector initialGuess,
            Func<Vector, Vector> residuals,
            Func<Vector, Matrix> jacobian = null,
            Vector bump = null,
            OptimizationType optimizationType = OptimizationType.Min,
            int maxIter = 100,
            int maxStaticIter = 50,
            double tau = 1e-3,
            double gradientTolerance = Descents.GRADIENT_EPSILON,
            double functionTolerance = Descents.ERR_EPSILON,
            double vInit = 2.0
            )
        {
            _result = initialGuess ?? throw new ArgumentNullException(nameof(initialGuess));
            _residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
            int dim = initialGuess.Size;
            _bump = bump ?? Vector.Create(dim, Descents.STEP_EPSILON);
            if (_bump.Data.Any(d => d <= 0)) throw new ArgumentException("bump must be > 0", nameof(bump));

            _jacobian = jacobian ?? _residuals.Jacobian(_bump);

            _optimizationType = optimizationType;
            _sign = _optimizationType == OptimizationType.Min ? -1 : 1;   

            _maxIter = maxIter;
            _maxStaticIter = maxStaticIter;
            _tau = tau;
            _gradientTolerance = gradientTolerance;
            _functionTolerance = functionTolerance;
            _vInit = vInit;
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

        #region Optimisation
        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm from J.Moré 
        /// </summary>
        public void OptimizeAdaptive()
        {
            _errors.Clear();
            _lambdas.Clear();
            _status = SolverStatus.Diverged;

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
                Matrix jacobian = _jacobian(_result);
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

                Vector resultNew = _result + delta;
                Vector residualNew = _residuals(resultNew);
                double errorNew = 0.5 * residualNew.SumOfSquares;

                double predictedReduction = -_sign * 0.5 * Vector.Scalar(delta, lambda * delta - gradient);
                double actualReduction = -_sign *( _error - errorNew);
                double reductionRatio = (predictedReduction > double.Epsilon) ? actualReduction / predictedReduction : -1.0;

                // Moré's rule for updating lambda
                if (reductionRatio > 0 && !double.IsInfinity(errorNew) && !double.IsNaN(errorNew))
                {
                    _result = resultNew;
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
            _status = SolverStatus.Diverged;

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
                Matrix jacobian = _jacobian(_result);
                Vector gradient = jacobian.Transpose * residual;
                Matrix JTJ = Matrix.TransposeBySelf(jacobian);

                Matrix A = JTJ + Matrix.CreateIdentityMatrix(JTJ.Rows, JTJ.Columns) * lambda;
                Vector delta = _sign * A.SolveWith(gradient);
                double gradNorm = gradient.Norm2;

                Vector resultNew = _result + delta;
                Vector residualNew = _residuals(resultNew);
                double errorNew = 0.5 * residualNew.SumOfSquares;

                double predictedReduction = -_sign * 0.5 * Vector.Scalar(delta, lambda * delta - gradient);
                double actualReduction = -_sign * (_error - errorNew);
                double reductionRatio = (predictedReduction > double.Epsilon) ? actualReduction / predictedReduction : -1;

                if (reductionRatio > 0 && !double.IsInfinity(errorNew) && !double.IsNaN(errorNew))
                {
                    _result = resultNew;
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

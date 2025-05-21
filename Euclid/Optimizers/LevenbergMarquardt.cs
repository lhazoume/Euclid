using Euclid.Solvers;
using System;
using System.Collections.Generic;

namespace Euclid.Optimizers
{
    /// <summary>
    /// Nonlinear least‑squares optimizer based on the Levenberg–Marquardt algorithm.
    /// </summary>
    public class LevenbergMarquardt
    {
        #region Declarations
        private readonly Func<Vector, Vector> _residuals;   // r(x)
        private readonly Func<Vector, Matrix> _jacobian;    // J(x)  can be null (=> numerical Jacobian)

        private readonly int _maxIter;
        private readonly double _tau;           // lambda0 = tau * max(diag(Jtranspose J))
        private readonly double _gradientTolerance;  // Tolerance on the gradient norm
        private readonly double _parameterTolerance; // Tolerance on the variation of x
        private readonly double _functionTolerance;      // Tolerance on the variation of the cost function
        private readonly double _vInit;
        private readonly double _stepSize;      // Step size for numerical Jacobian

        private SolverStatus _status = SolverStatus.NotRan;
        private Vector _x;      // current solution
        private double _error;  // current (0.5 * ||r(x)||^2)
        private int _nResidualEvals; // Number of residual function evaluations
        private int _nJacobianEvals; // Number of Jacobian evaluations (if analytical)

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
        /// <param name="maxIter">Maximum number of iterations.</param>
        /// <param name="tau">Initial factor for the Levenberg‑Marquardt parameter (lambda).</param>
        /// <param name="gradientTolerance">Tolerance for the gradient norm. Optimization stops if ||J^T r|| ≤ gradientTolerance.</param>
        /// <param name="parameterTolerance">Tolerance for parameter variation. Optimization stops if ||delta x|| ≤ parameterTolerance * (||x|| + parameterTolerance).</param>
        /// <param name="functionTolerance">Tolerance for cost function variation. Optimization stops if |Δerror| ≤ functionTolerance * (error + functionTolerance).</param>
        /// <param name="vInit">Initial factor for increasing λ when a step is rejected.</param>
        /// <param name="stepSize">Relative step size for finite differences in numerical Jacobian.</param>
        public LevenbergMarquardt(
            Vector initialGuess,
            Func<Vector, Vector> residuals,
            Func<Vector, Matrix> jacobian = null,
            int maxIter = 100,
            double tau = 1e-3,
            double gradientTolerance = 1e-8,
            double parameterTolerance = 1e-8,
            double functionTolerance = 1e-8,
            double vInit = 2.0,
            double stepSize = 1e-8)
        {
            _x = initialGuess?.Clone ?? throw new ArgumentNullException(nameof(initialGuess));
            _residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
            _jacobian = jacobian;

            _maxIter = maxIter;
            _tau = tau;
            _gradientTolerance = gradientTolerance;
            _parameterTolerance = parameterTolerance;
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
        public Vector Result => _x;
        /// <summary>
        /// Gets the current error value.
        /// </summary>
        public double Error => _error;
        /// <summary>
        /// Number of residual evaluations.
        /// </summary>
        public int ResidualEvaluations => _nResidualEvals;
        /// <summary>Total number of Jacobian function evaluations (if provided analytically).</summary>
        public int JacobianEvaluations => _nJacobianEvals;
        /// <summary>
        /// Gets the list of error values during the optimization process.
        /// </summary>
        public IEnumerable<double> Errors => _errors;
        /// <summary>
        /// Gets the list of lambda values used during the optimization process.
        /// </summary>
        public IEnumerable<double> Lambdas => _lambdas;
        #endregion
        #region Methods
        #region Numerical Jacobian

        /// <summary>
        /// Computes the Jacobian numerically using central finite differences.
        /// Each call to this method performs 2 * x.Size evaluations of the residual function.
        /// </summary>
        /// <param name="rFunc">The residual function.</param>
        /// <param name="x">The point at which to evaluate the Jacobian.</param>
        /// <param name="incrementResidualEvals">Action to increment the residual evaluation counter.</param>
        /// <param name="eps">The relative step size for finite differences.</param>
        /// <returns>The estimated Jacobian matrix.</returns>
        private static Matrix NumericalJacobian(
            Func<Vector, Vector> rFunc,
            Vector x,
            Action incrementResidualEvals,
            double eps)
        {
            int n = x.Size;
            Matrix J = null;

            for (int j = 0; j < n; j++)
            {
                double xjOriginal = x[j];
                double h = Math.Max(1.0, Math.Abs(xjOriginal)) * eps;
                if (h == 0) h = eps;
                // Evaluate r(x + h e_j)
                x[j] = xjOriginal + h;
                Vector rPlusH = rFunc(x);
                incrementResidualEvals();
                // Evaluate r(x - h e_j)
                x[j] = xjOriginal - h;
                Vector rMinusH = rFunc(x);
                incrementResidualEvals();

                x[j] = xjOriginal;

                if (J == null)
                    J = Matrix.Create(rPlusH.Size, n);

                for (int i = 0; i < rPlusH.Size; i++)
                {
                    J[i, j] = (rPlusH[i] - rMinusH[i]) / (2 * h);
                }
            }
            return J ?? Matrix.Create(0, n);
        }

        #endregion

        #region Optimisation
        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm.
        /// </summary>
        public void Optimize()
        {
            // Initialisation
            _nResidualEvals = 0;
            _nJacobianEvals = 0;
            _errors.Clear();
            _lambdas.Clear();
            _status = SolverStatus.NotRan;

            // Compute initial residual vector and error
            Vector r = _residuals(_x);
            _nResidualEvals++;
            _error = 0.5 * Vector.Scalar(r, r);
            // Compute Jacobian (analytical if provided, otherwise n
            Matrix J;
            if (_jacobian != null)
            {
                J = _jacobian(_x);
                _nJacobianEvals++;
            }
            else
            {
                J = NumericalJacobian(_residuals, _x, () => _nResidualEvals++, _stepSize);
            }

            Vector g = J.Transpose * r; // Gradient 
            // Compute JTJ and its largest diagonal element
            Matrix JTJ = Matrix.TransposeBySelf(J);
            double diagMax = 0;
            if (JTJ.Rows > 0)
            {
                for (int i = 0; i < JTJ.Rows; i++)
                    diagMax = Math.Max(diagMax, Math.Abs(JTJ[i, i]));
            }
            if (diagMax == 0) diagMax = 1.0;

            double lambda = _tau * diagMax;
            double v = _vInit;

            _errors.Add(_error);
            _lambdas.Add(lambda);
           
            if (g.Norm2 <= _gradientTolerance)
            {
                _status = SolverStatus.GradientConvergence;
                return;
            }

            _status = SolverStatus.IterationExceeded;
            // Main optimization loop
            for (int k = 0; k < _maxIter; k++)
            {
                Matrix A = JTJ + (lambda * Matrix.CreateIdentityMatrix(JTJ.Rows, JTJ.Columns));
                Vector delta = A.SolveWith(-g);

                if (delta.Norm2 <= _parameterTolerance * (_x.Norm2 + _parameterTolerance))
                {
                    _status = SolverStatus.StationaryFunction;
                    break;
                }

                Vector xNew = _x + delta;
                Vector rNew = _residuals(xNew);
                _nResidualEvals++;
                double errorNew = 0.5 * Vector.Scalar(rNew, rNew);
                // Compute gain denominator and gain ratio
                double gainDen = 0.5 * Vector.Scalar(delta, lambda * delta - g);
                double rho = (gainDen > double.Epsilon) ? (_error - errorNew) / gainDen : -1;

                if (rho > 0 && !double.IsInfinity(errorNew) && !double.IsNaN(errorNew))
                {
                    double errorOld = _error;
                    _x = xNew;
                    _error = errorNew;
                    r = rNew;

                    if (_jacobian != null)
                    {
                        J = _jacobian(_x);
                        _nJacobianEvals++;
                    }
                    else
                    {
                        J = NumericalJacobian(_residuals, _x, () => _nResidualEvals++, _stepSize);
                    }

                    g = J.Transpose * r;
                    JTJ = Matrix.TransposeBySelf(J);

                    lambda *= Math.Max(1.0 / 3.0, 1.0 - Math.Pow(2.0 * rho - 1.0, 3));
                    v = _vInit;

                    _errors.Add(_error);
                    _lambdas.Add(lambda);

                    if (g.Norm2 <= _gradientTolerance)
                    {
                        _status = SolverStatus.GradientConvergence;
                        break;
                    }
                    if (Math.Abs(errorOld - _error) <= _functionTolerance * (Math.Abs(errorOld) + _functionTolerance))
                    {
                        _status = SolverStatus.FunctionConvergence;
                        break;
                    }
                }
                else
                {
                    lambda *= v;
                    v *= 2.0;

                    _errors.Add(_error);
                    _lambdas.Add(lambda);
                }
            }
        }
        #endregion
        #endregion
    }
}

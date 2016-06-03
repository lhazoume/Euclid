using System;
using System.Collections.Generic;

namespace Euclid.Solvers
{
    /// <summary>
    /// Class used to perform a gradient descent on any multivariate function
    /// </summary>
    public class GradientDescent
    {
        #region Declarations
        private Func<Vector, double> _function;
        private double _error;
        private int _iterations, _maxIterations, _maxLineSearchIterations, _evaluations;
        private Vector _initialGuess, _increments, _result;
        private List<Vector> _descentDirections = new List<Vector>();
        private List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private LineSearch _lineSearch;
        private SolverStatus _status = SolverStatus.NotRan;
        #endregion

        /// <summary>
        /// Builds a GradientDescent helper
        /// </summary>
        /// <param name="initialGuess">the initial guess Vector</param>
        /// <param name="lineSearch">the line search method</param>
        /// <param name="maxIterations">the maximum number of iterations in the gradient</param>
        /// <param name="maxLineSearchIterations">the maximum number of iterations in the line search</param>
        public GradientDescent(Vector initialGuess, LineSearch lineSearch, Func<Vector, double> function, int maxIterations, int maxLineSearchIterations)
        {
            _initialGuess = initialGuess.Clone;
            _lineSearch = lineSearch;
            _maxIterations = maxIterations;
            _evaluations = 0;
            _function = function;
            _maxLineSearchIterations = maxLineSearchIterations;
        }

        #region Accessors

        #region Settables

        /// <summary>Gets and sets the line search method used to optimize the step</summary>
        public LineSearch LineSearch
        {
            get { return _lineSearch; }
            set { _lineSearch = value; }
        }

        /// <summary>Gets and sets the function to minimize</summary>
        public Func<Vector, double> Function
        {
            get { return _function; }
            set { _function = value; }
        }

        /// <summary>Gets and sets the maximum number of iterations </summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set { _maxIterations = value; }
        }

        /// <summary>Gets and sets the maxium number of iterations in the line search </summary>
        public int MaxLineSearchIterations
        {
            get { return _maxLineSearchIterations; }
            set { _maxLineSearchIterations = value; }
        }

        #endregion

        #region Get

        /// <summary>
        /// The final status of the solver
        /// </summary>
        public SolverStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Returns the number of interations of the solver
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
        }

        /// <summary>
        /// Returns the final value of the function
        /// </summary>
        public double Error
        {
            get { return _error; }
        }

        /// <summary>
        /// The result of the solver
        /// </summary>
        public Vector Result
        {
            get { return _result; }
        }

        /// <summary>Gets the details of the convergence (gradient norm, error)</summary>
        public List<Tuple<double, double>> Convergence
        {
            get { return new List<Tuple<double, double>>(_convergence); }
        }

        /// <summary>Gets the number of times the function was evaluated </summary>
        public int Evaluations
        {
            get { return _evaluations; }
        }

        #endregion

        #endregion

        #region Line searches

        private double LineSearchBranch(double error, Vector x, Vector direction, Vector gradient)
        {
            switch (_lineSearch)
            {
                case LineSearch.Armijo: return ArmijoLineSearch(error, x, direction, gradient);
                case LineSearch.ArmijoGoldStein: return ArmijoGoldsteinLineSearch(error, x, direction, gradient);
                case LineSearch.StrongWolfe: return StrongWolfeLineSearch(error, x, direction, gradient);
                default: return NaiveLineSearch(error, x, direction, gradient);
            }
        }

        private double NaiveLineSearch(double error, Vector x, Vector direction, Vector gradient)
        {
            double h0 = error,
                d0 = Vector.Scalar(direction, gradient),
                alpha = 1;
            int k = 0;

            while (_function(x + (alpha * direction)) >= error && k < _maxLineSearchIterations)
            {
                _evaluations++;
                alpha *= 0.5;
                k++;
            }

            return alpha;
        }

        private double ArmijoLineSearch(double error, Vector x, Vector direction, Vector gradient)
        {
            double h0 = error,
                c1_d0 = 1e-4 * Vector.Scalar(direction, gradient),
                alpha = 1;
            int k = 0;
            while (_function(x + (alpha * direction)) > error + c1_d0 * alpha && k < _maxLineSearchIterations)
            {
                _evaluations++;
                alpha *= 0.5;
                k++;
            }

            return alpha;
        }

        private double ArmijoGoldsteinLineSearch(double error, Vector x, Vector direction, Vector gradient)
        {
            double h0 = error,
                d0 = Vector.Scalar(direction, gradient),
                alpha = 1,
                c1_d0 = 1e-4 * d0,
                c2_d0 = 0.9 * d0;

            int k = 0;

            while ((_function(x + (alpha * direction)) > error + c1_d0 * alpha
                ||
                Vector.Scalar(direction, NumericalGradient(x + (alpha * direction), _increments)) < c2_d0)
                &&
                k < _maxLineSearchIterations)
            {
                _evaluations++;
                alpha *= 0.5;
                k++;
            }
            return alpha;
        }

        private double StrongWolfeLineSearch(double error, Vector x, Vector direction, Vector gradient)
        {
            double h0 = error,
                d0 = Vector.Scalar(direction, gradient),
                alpha = 1,
                c1_d0 = 1e-4 * d0,
                c2_d0 = 0.9 * d0;
            int k = 0;
            double curvature = Vector.Scalar(direction, NumericalGradient(x + (alpha * direction), _increments));

            while ((_function(x + (alpha * direction)) > error + c1_d0 * alpha
                ||
                curvature < c2_d0
                ||
                Math.Abs(curvature) > Math.Abs(c2_d0))
                &&
                k < _maxLineSearchIterations)
            {
                _evaluations++;
                alpha *= 0.5;
                curvature = Vector.Scalar(direction, NumericalGradient(x + (alpha * direction), _increments));
                k++;
            }
            return alpha;
        }

        #endregion

        #region Methods

        #region Services
        private Vector NumericalGradient(Vector x, Vector increment)
        {
            double refValue = _function(x);
            _evaluations++;
            Vector result = Vector.Create(x.Size);

            for (int i = 0; i < result.Size; i++)
            {
                Vector xd = x.Clone;
                xd[i] += increment[i];
                result[i] = (_function(xd) - refValue) / increment[i];
                _evaluations++;
            }

            return result;
        }
        #endregion

        /// <summary>Minimizes the function using classic Gradient Descent algorithm</summary>
        public void Minimize()
        {
            _evaluations = 0;
            if (_function == null) throw new NullReferenceException("function should not be null");
            _increments = Vector.Create(_initialGuess.Size, Descents.STEP_EPSILON);

            _convergence.Clear();

            _result = _initialGuess.Clone;
            _status = SolverStatus.Diverged;
            _error = _function(_result);
            _evaluations++;
            Vector gradient = NumericalGradient(_result, _increments),
                direction = -gradient;

            _descentDirections.Add(direction.Clone);
            _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));

            _iterations = 1;

            while (gradient.Norm2 > Descents.GRADIENT_EPSILON && _iterations <= _maxIterations)
            {
                double factor = LineSearchBranch(_error, _result, direction, gradient);
                _result = _result + (factor * direction);

                _error = _function(_result);
                _evaluations++;
                gradient = NumericalGradient(_result, _increments);
                direction = -gradient;

                _descentDirections.Add(direction.Clone);
                _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));

                _iterations++;
            }

            if (gradient.Norm2 <= Descents.GRADIENT_EPSILON)
                _status = SolverStatus.Normal;
            else if (_iterations > _maxIterations)
                _status = SolverStatus.IterationExceeded;
        }

        /// <summary>Minimizes the function using the BFGS gradient descent</summary>
        public void MinimizeBFGS()
        {
            _evaluations = 0;
            if (_function == null) throw new NullReferenceException("function should not be null");
            _increments = Vector.Create(_initialGuess.Size, Descents.STEP_EPSILON);

            _convergence.Clear();

            List<Vector> gradients = new List<Vector>();

            _result = _initialGuess.Clone;
            _status = SolverStatus.Diverged;
            _error = _function(_result);
            _evaluations++;
            Vector gradient = NumericalGradient(_result, _increments),
                direction = -gradient;
            Matrix B = Matrix.CreateIdentityMatrix(gradient.Size, gradient.Size);

            _descentDirections.Add(direction.Clone);
            _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));
            gradients.Add(gradient);

            _iterations = 1;

            while (gradient.Norm2 > Descents.GRADIENT_EPSILON && _iterations <= _maxIterations)
            {
                double factor = LineSearchBranch(_error, _result, direction, gradient);
                Vector s = factor * direction;
                _result = _result + s;

                _error = _function(_result);
                _evaluations++;
                gradient = NumericalGradient(_result, _increments);
                gradients.Add(gradient);
                Vector y = gradients[gradients.Count - 1] - gradients[gradients.Count - 2];
                B = B + (y * y) / Vector.Scalar(y, s) - ((B * s) * (s * B)) / Vector.Scalar(s, B * s);
                direction = -gradient;

                _descentDirections.Add(direction.Clone);
                _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));

                _iterations++;
            }

            if (gradient.Norm2 <= Descents.GRADIENT_EPSILON)
                _status = SolverStatus.Normal;
            else if (_iterations > _maxIterations)
                _status = SolverStatus.IterationExceeded;
        }

        #endregion
    }
}

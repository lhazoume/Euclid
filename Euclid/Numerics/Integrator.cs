using Euclid.Solvers;
using System;
using System.Collections.Generic;

namespace Euclid.Numerics
{
    public class Integrator
    {
        #region Declaration
        private Func<double, double> _f;
        private IntegrationForm _form;
        private double _result = 0,
            _error = 0,
            _a, _b;
        private List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private int _iterations = 0,
            _maxIterations;
        private SolverStatus _status = SolverStatus.NotRan;
        #endregion

        public Integrator(double a, double b, Func<double, double> f, IntegrationForm form, int maxIterations)
        {
            _a = a;
            _b = b;
            _f = f;
            _maxIterations = maxIterations;
            _form = form;
        }

        #region Accessors

        #region Settables

        public Func<double, double> Function
        {
            get { return _f; }
            set { _f = value; }
        }
        public int MaxIterations
        {
            get { return _maxIterations; }
            set { _maxIterations = value; }
        }
        public double LowerBound
        {
            get { return _a; }
            set { _a = value; }
        }
        public double UpperBound
        {
            get { return _b; }
            set { _b = value; }
        }
        public IntegrationForm Form
        {
            get { return _form; }
            set { _form = value; }
        }

        #endregion

        #region Get

        public double Error
        {
            get { return _error; }
        }
        public int Iterations
        {
            get { return _iterations; }
        }
        public SolverStatus Status
        {
            get { return _status; }
        }
        public double Result
        {
            get { return _result; }
        }
        public List<Tuple<double, double>> Convergence
        {
            get { return new List<Tuple<double, double>>(_convergence); }
        }

        #endregion

        #endregion

        #region Methods

        public void Integrate()
        {
            if (_f == null) throw new NullReferenceException("Integrated function should not be null");

            _convergence.Clear();

            int n = 2;
            double previousResult = 0,
                result = Calculate(n);
            _status = SolverStatus.Diverged;
            _error = Math.Abs(previousResult - result);

            _convergence.Add(new Tuple<double, double>(result, _error));
            _iterations = 1;

            while (Math.Abs(_error) > Descents.GRADIENT_EPSILON && _iterations <= _maxIterations)
            {
                n = n * 2;
                previousResult = result;
                result = Calculate(n);

                _error = Math.Abs(previousResult - result);
                _convergence.Add(new Tuple<double, double>(result, _error));
                _iterations++;
            }

            if (Math.Abs(_error) <= Descents.GRADIENT_EPSILON)
                _status = SolverStatus.Normal;
            else if (_iterations > _maxIterations)
                _status = SolverStatus.IterationExceeded;

            _result = result;
        }

        private double Calculate(int n)
        {
            if (_f == null) throw new ArgumentNullException("The function should not be null");
            if (n < 1) throw new ArgumentException("the degree should be strictly positive");
            if (_a == _b) return 0;

            double h = (_b - _a) / n,
                result = 0;

            if (_form == IntegrationForm.Left)
            {
                for (int i = 0; i < n; i++)
                    result += _f(_a + i * h);
            }
            else if (_form == IntegrationForm.Right)
            {
                for (int i = 0; i < n; i++)
                    result += _f(_a + (i + 1) * h);
            }
            else if (_form == IntegrationForm.Middle)
            {
                for (int i = 0; i < n; i++)
                    result += _f(_a + (i + 0.5) * h);
            }
            else if (_form == IntegrationForm.Trapeze)
            {
                result += (_f(_a) + _f(_b)) / 2;
                for (int i = 1; i < n; i++)
                    result += _f(_a + i * h);
            }
            else if (_form == IntegrationForm.Simpson)
            {
                int m = n - (n % 2);
                h = (_b - _a) / m;
                result += _f(_a) + _f(_b) + 4 * _f(_a + (m - 1) * h);
                for (int j = 1; j <= m / 2 - 1; j++)
                    result += 2 * (_f(_a + 2 * j * h) + 2 * _f(_a + (2 * j - 1) * h));
                result /= 3;
            }

            return result * h;
        }

        #endregion
    }
}

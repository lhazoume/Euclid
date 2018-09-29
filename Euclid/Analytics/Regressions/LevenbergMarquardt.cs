using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Regressions
{
    public class LevenbergMarquardt<T, V> where T : IEquatable<T>, IComparable<T> where V : IEquatable<V>, IConvertible
    {
        #region Variables
        private DataFrame<T, double, V> _x;
        private Series<T, double, V> _y;
        private Func<Vector, Vector, double> _function;
        private Func<Vector, Vector, Vector> _gradient;
        private int _maxIterations;
        #endregion

        private bool _returnAverageIfFailed, _withConstant;
        private RegressionStatus _status;
        private double _finalLogLikelihood = double.MinValue;
        private LogisticModel _logisticModel = null;

        #region Estimates the function and the jacobian
        private static Vector Function(Vector beta, DataFrame<T, double, V> x, Func<Vector, Vector, double> function)
        {
            Slice<T, double, V>[] slices = x.GetSlices();
            Vector result = Vector.Create(slices.Length);
            for (int i = 0; i < slices.Length; i++)
                result[i] = function(Vector.Create(slices[i].Data), beta);
            return result;
        }
        private static Matrix Jacobian(Vector beta, DataFrame<T, double, V> x, Func<Vector, Vector, Vector> gradient)
        {
            Slice<T, double, V>[] slices = x.GetSlices();
            Matrix result = Matrix.Create(slices.Length, beta.Size);
            for (int i = 0; i < slices.Length; i++)
                result.SetRow(gradient(Vector.Create(slices[i].Data), beta), i);
            return result;
        }
        private static Vector Increment(Matrix jacobian, double lambda, Vector error)
        {
            Matrix jTj = jacobian.Transpose * jacobian,
                damp = jTj + (lambda * jTj.Diagonal);
            return damp.Inverse * jacobian.Transpose * error;
        }
        #endregion

        public void Optimize(double lambda0, double vu)
        {
            double lambda;
        }
    }
}

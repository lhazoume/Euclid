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
        private readonly DataFrame<T, double, V> _x;
        private readonly Series<T, double, V> _y;
        private readonly Func<Vector, Vector, double> _function;
        private readonly Func<Vector, Vector, Vector> _gradient;
        private readonly int _maxIterations;
        #endregion

        private readonly bool _returnAverageIfFailed;
        private readonly bool _withConstant;
        private readonly RegressionStatus _status;
        private readonly double _finalLogLikelihood = double.MinValue;
        private readonly LogisticModel _logisticModel = null;

        #region Estimates the function,the jacobian and the increment
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

        public void Optimize(double lambda0, double v)
        {
            double lambda;
        }
    }
}

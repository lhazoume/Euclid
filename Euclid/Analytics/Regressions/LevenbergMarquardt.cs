namespace Euclid.Analytics.Regressions
{
    /*
    public class LevenbergMarquardt<T, TV> where T : IEquatable<T>, IComparable<T> where TV : IEquatable<TV>, IConvertible
    {
        #region Variables
        private readonly DataFrame<T, double, TV> _x;
        private readonly Series<T, double, TV> _y;
        private readonly Func<Vector, Vector, double> _function;
        private readonly Func<Vector, Vector, Vector> _gradient;
        private readonly int _maxIterations;
        #endregion

        #region Estimates the function,the jacobian and the increment
        private static Vector Function(Vector beta, DataFrame<T, double, TV> x, Func<Vector, Vector, double> function)
        {
            Slice<T, double, TV>[] slices = x.GetSlices();
            Vector result = Vector.Create(slices.Length);
            for (int i = 0; i < slices.Length; i++)
                result[i] = function(Vector.Create(slices[i].Data), beta);
            return result;
        }
        private static Matrix Jacobian(Vector beta, DataFrame<T, double, TV> x, Func<Vector, Vector, Vector> gradient)
        {
            Slice<T, double, TV>[] slices = x.GetSlices();
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
    */
}

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
        private int _maxIterations;
        #endregion

        private bool _returnAverageIfFailed, _withConstant;
        private RegressionStatus _status;
        private double _finalLogLikelihood = double.MinValue;
        private LogisticModel _logisticModel = null;

    }
}

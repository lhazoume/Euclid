using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Solvers
{
    public class EndCriteria
    {
        #region 
        private int? _maxIterations, _maxStaticIterations;
        private double? _functionEpsilon, _gradientEpsilon;
        #endregion

        public EndCriteria(int? maxIterations = null, int? maxStaticIterations = null, double? functionEpsilon=null, double ? gradientEpsilon = null)
        {

        }

        public bool BelowFunctionEpsilon(double value)
        {
            return _functionEpsilon.HasValue ? Math.Abs( value) < _functionEpsilon : false;
        }

        public bool BelowGradientEpsilon(double value)
        {
            return _gradientEpsilon.HasValue ? Math.Abs(value) < _gradientEpsilon : false;
        }

        public bool ExceededIterations(int iterations)
        {
            return _maxIterations.HasValue ? iterations > _maxIterations.Value : false;
        }

        public bool ExceededMaxStaticIterations(List<double> history)
        {
            if (!_maxStaticIterations.HasValue) return false;
            if (history.Count < _maxStaticIterations.Value) return false;
            List<double> subHistory =  history.GetRange(history.Count - _maxStaticIterations.Value, _maxStaticIterations.Value) ;
            return subHistory.Max() - subHistory.Min() < _functionEpsilon;
        }
    }
}

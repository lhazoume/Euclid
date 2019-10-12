using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.DecisionTree
{
    public interface IDecisionNode
    {
        double Evaluate(double[] data);

        int Leaves { get; }
        int Depth { get; }
    }
    public class DecisionTreeBarrierNode : IDecisionNode
    {
        #region Variables
        private readonly double _barrier;
        private readonly IDecisionNode _greaterOrEqual, _less;
        private readonly int _featureIndex;
        #endregion

        public DecisionTreeBarrierNode(double barrier, int featureIndex, IDecisionNode greaterOrEqual, IDecisionNode less)
        {
            if (featureIndex < 0) throw new ArgumentOutOfRangeException("feaureIndex", "The feature index should be positive");
            if (greaterOrEqual is null) throw new ArgumentNullException("greaterOrEqual", "The greater or equal node is null");
            if (less is null) throw new ArgumentNullException("less", "The less node is null");

            _barrier = barrier;
            _featureIndex = featureIndex;
            _greaterOrEqual = greaterOrEqual;
            _less = less;
        }
        public double Evaluate(double[] data)
        {
            return (data[_featureIndex] >= _barrier ? _greaterOrEqual : _less).Evaluate(data);
        }

        #region Accessors
        public double Barrier { get { return _barrier; } }
        public int FeatureIndex { get { return _featureIndex; } }
        public IDecisionNode GreaterOrEqual { get { return _greaterOrEqual; } }
        public IDecisionNode Less { get { return _less; } }
        public int Leaves { get { return _greaterOrEqual.Leaves + _less.Leaves; } }
        public int Depth { get { return 1 + Math.Max(_greaterOrEqual.Depth, _less.Depth); } }
        #endregion
    }

    public class DecisionTreeEqualNode : IDecisionNode
    {
        #region Variables
        private readonly double _target;
        private readonly IDecisionNode _equal, _notEqual;
        private readonly int _featureIndex;
        #endregion

        public DecisionTreeEqualNode(double target, int featureIndex, IDecisionNode equal, IDecisionNode different)
        {
            if (featureIndex < 0) throw new ArgumentOutOfRangeException("feaureIndex", "The feature index should be positive");
            if (equal is null) throw new ArgumentNullException("equal", "The equal node is null");
            if (different is null) throw new ArgumentNullException("different", "The different node is null");

            _target = target;
            _featureIndex = featureIndex;
            _equal = equal;
            _notEqual = different;
        }
        public double Evaluate(double[] data)
        {
            return (data[_featureIndex] == _target ? _equal : _notEqual).Evaluate(data);
        }

        #region Accessors
        public double Target { get { return _target; } }
        public int FeatureIndex { get { return _featureIndex; } }
        public IDecisionNode Equal { get { return _equal; } }
        public IDecisionNode NotEqual { get { return _notEqual; } }
        public int Leaves { get { return _equal.Leaves + _notEqual.Leaves; } }
        public int Depth { get { return 1 + Math.Max(_equal.Depth, _notEqual.Depth); } }
        #endregion
    }
    public class DecisionTreeConstantLeaf : IDecisionNode
    {
        private readonly double _value;

        public DecisionTreeConstantLeaf(double value)
        {
            _value = value;
        }

        public double Evaluate(double[] data)
        {
            return _value;
        }

        public int Leaves { get { return 1; } }
        public int Depth { get { return 0; } }
    }
}

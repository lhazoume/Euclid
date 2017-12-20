using Euclid.Analytics.Clustering;
using System.Collections.Generic;

namespace Euclid.Analytics
{
    /*
    public class DecisionTreePredictor : IPredictor<double, double>
    {
        private IDecisionNode<Vector, IPredictor<double,double>> _node;

        public DecisionTreePredictor(IDecisionNode<Vector, IPredictor<double, double>> node)
        {
            _node = node.Clone;
        }

        public IDecisionNode<Vector, IPredictor<double, double>> Node
        {
            get { return _node; }
        }

        public double Predict(IList<double> x)
        {
            Vector v = Vector.Create(x);
            IPredictor<double, double> model = _node.Evaluate(v);
            return model.Predict(v.Data);
        }
    }*/
}

using System;

namespace Euclid.Analytics.Clustering
{
    public interface IDecisionNode<C, A>
    {
        A Evaluate(C client);

        int Depth { get; }
        int Children { get; }
        IDecisionNode<C, A> Clone { get; }
    }

    public class DecisionNode<C, A> : IDecisionNode<C, A>
    {
        #region Declaration
        private PrettyPredicate<C> _predicate;
        private IDecisionNode<C, A> _true, _false;
        #endregion

        public DecisionNode(PrettyPredicate<C> predicate, IDecisionNode<C, A> trueBranch, IDecisionNode<C, A> falseBranch)
        {
            _predicate = predicate;
            _true = trueBranch;
            _false = falseBranch;
        }

        #region Accessors

        public PrettyPredicate<C> Predicate
        {
            get { return _predicate; }
            set { _predicate = value; }
        }

        public IDecisionNode<C, A> Clone
        {
            get { return new DecisionNode<C, A>(_predicate.Clone, _true.Clone, _false.Clone); }
        }

        public IDecisionNode<C, A> True
        {
            get { return _true; }
            set { _true = value; }
        }

        public IDecisionNode<C, A> False
        {
            get { return _false; }
            set { _false = value; }
        }

        public int Depth
        {
            get { return 1 + Math.Max(_true.Depth, _false.Depth); }
        }

        public int Children
        {
            get { return 2 + _false.Children + _true.Children; }
        }

        #endregion

        public A Evaluate(C client)
        {
            return (_predicate.Predicate(client) ? _true : _false).Evaluate(client);
        }
    }

    public class DecisionLeaf<C, A> : IDecisionNode<C, A>
    {
        private A _answer;

        public DecisionLeaf(A answer)
        {
            _answer = answer;
        }

        #region Accessors
        public A Answer
        {
            get { return _answer; }
            set { _answer = value; }
        }

        public IDecisionNode<C, A> Clone
        {
            get { return new DecisionLeaf<C, A>(_answer); }
        }

        public int Depth
        {
            get { return 0; }
        }

        public int Children
        {
            get { return 0; }
        }
        #endregion

        public A Evaluate(C client)
        {
            return _answer;
        }

    }

    public class PrettyPredicate<C>
    {
        #region Declarations
        private readonly Predicate<C> _predicate;
        private readonly string _description;
        #endregion

        public PrettyPredicate(string description, Predicate<C> predicate)
        {
            _description = description;
            _predicate = predicate;
        }

        #region Accessors
        public string Description
        {
            get { return _description; }
        }
        public Predicate<C> Predicate
        {
            get { return _predicate; }
        }
        public PrettyPredicate<C> Clone
        {
            get { return new PrettyPredicate<C>(_description, _predicate); }
        }
        #endregion
    }
}

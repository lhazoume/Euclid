using System;

namespace Euclid.Analytics.Clustering
{
    /// <summary>Interface for decision tree node</summary>
    /// <typeparam name="TI">the input type</typeparam>
    /// <typeparam name="TA">the answer type</typeparam>
    public interface IDecisionNode<TI, TA>
    {
        /// <summary>Evaluates the answer for a given input</summary>
        /// <param name="input">the input</param>
        /// <returns>an answer</returns>
        TA Evaluate(TI input);

        /// <summary>Returns the depth of the node</summary>
        int Depth { get; }

        /// <summary>Counts the children of the node</summary>
        int Children { get; }

        /// <summary>Clones the node</summary>
        IDecisionNode<TI, TA> Clone { get; }
    }

    /// <summary>Decision tree node</summary>
    /// <typeparam name="TI">the input type</typeparam>
    /// <typeparam name="TA">the answer type</typeparam>
    public class DecisionNode<TI, TA> : IDecisionNode<TI, TA>
    {
        #region Declaration
        private PrettyPredicate<TI> _predicate;
        private IDecisionNode<TI, TA> _true, _false;
        #endregion

        /// <summary>Builds a decision tree node split according to a predicate</summary>
        /// <param name="predicate">the splitting predicate</param>
        /// <param name="trueBranch">the tree subpart if the predicate is met</param>
        /// <param name="falseBranch">the tree subpart if the predicate is not met</param>
        public DecisionNode(PrettyPredicate<TI> predicate, IDecisionNode<TI, TA> trueBranch, IDecisionNode<TI, TA> falseBranch)
        {
            _predicate = predicate;
            _true = trueBranch;
            _false = falseBranch;
        }

        #region Accessors

        /// <summary>Returns the splitting predicate</summary>
        public PrettyPredicate<TI> Predicate
        {
            get { return _predicate; }
            set { _predicate = value; }
        }

        /// <summary>Clones the node</summary>
        public IDecisionNode<TI, TA> Clone
        {
            get { return new DecisionNode<TI, TA>(_predicate.Clone, _true.Clone, _false.Clone); }
        }

        /// <summary>Returns the tree subpart if the predicate is met </summary>
        public IDecisionNode<TI, TA> True
        {
            get { return _true; }
            set { _true = value; }
        }

        /// <summary>Returns the tree subpart if the predicate is not met</summary>
        public IDecisionNode<TI, TA> False
        {
            get { return _false; }
            set { _false = value; }
        }

        /// <summary>Returns the depth of the node</summary>
        public int Depth
        {
            get { return 1 + Math.Max(_true.Depth, _false.Depth); }
        }

        /// <summary>Counts the children of the node</summary>
        public int Children
        {
            get { return 2 + _false.Children + _true.Children; }
        }

        #endregion

        /// <summary>Evaluates the answer for a given input</summary>
        /// <param name="input">the input</param>
        /// <returns>an answer</returns>
        public TA Evaluate(TI input)
        {
            return (_predicate.Predicate(input) ? _true : _false).Evaluate(input);
        }
    }

    /// <summary>Decision tree leaf</summary>
    /// <typeparam name="TI">the input type</typeparam>
    /// <typeparam name="TA">the answer type</typeparam>
    public class DecisionLeaf<TI, TA> : IDecisionNode<TI, TA>
    {
        private TA _answer;

        /// <summary>Builds a decision tree leaf</summary>
        /// <param name="answer">the fixed answer</param>
        public DecisionLeaf(TA answer)
        {
            _answer = answer;
        }

        #region Accessors

        /// <summary>Returns the answer assigned to the leaf</summary>
        public TA Answer
        {
            get { return _answer; }
            set { _answer = value; }
        }

        /// <summary>Clones the node</summary>
        public IDecisionNode<TI, TA> Clone
        {
            get { return new DecisionLeaf<TI, TA>(_answer); }
        }

        /// <summary>Returns the depth of the node</summary>
        public int Depth
        {
            get { return 0; }
        }

        /// <summary>Counts the children of the node</summary>
        public int Children
        {
            get { return 0; }
        }
        #endregion

        /// <summary>Evaluates the answer for a given input</summary>
        /// <param name="input">the input</param>
        /// <returns>an answer</returns>
        public TA Evaluate(TI input)
        {
            return _answer;
        }

    }

    /// <summary>Decorated predicate class</summary>
    /// <typeparam name="TI">the input type</typeparam>
    public class PrettyPredicate<TI>
    {
        #region Declarations
        private readonly Predicate<TI> _predicate;
        private readonly string _description;
        #endregion

        /// <summary>Builds a decorated predicate class</summary>
        /// <param name="description">the description of the predicate</param>
        /// <param name="predicate">the predicate</param>
        public PrettyPredicate(string description, Predicate<TI> predicate)
        {
            _description = description;
            _predicate = predicate;
        }

        #region Accessors

        /// <summary>Returns the description of the predicate</summary>
        public string Description => _description;

        /// <summary>The predicate</summary>
        public Predicate<TI> Predicate => _predicate;

        /// <summary>Returns a clone of the pretty predicate</summary>
        public PrettyPredicate<TI> Clone
        {
            get { return new PrettyPredicate<TI>(_description, _predicate); }
        }
        #endregion
    }
}

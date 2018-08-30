using System;

namespace Euclid.Analytics.Clustering
{
    /// <summary>Interface for decision tree node</summary>
    /// <typeparam name="I">the input type</typeparam>
    /// <typeparam name="A">the answer type</typeparam>
    public interface IDecisionNode<I, A>
    {
        /// <summary>Evaluates the answer for a given input</summary>
        /// <param name="input">the input</param>
        /// <returns>an answer</returns>
        A Evaluate(I input);

        /// <summary>Returns the depth of the node</summary>
        int Depth { get; }

        /// <summary>Counts the children of the node</summary>
        int Children { get; }

        /// <summary>Clones the node</summary>
        IDecisionNode<I, A> Clone { get; }
    }

    /// <summary>Decision tree node</summary>
    /// <typeparam name="I">the input type</typeparam>
    /// <typeparam name="A">the answer type</typeparam>
    public class DecisionNode<I, A> : IDecisionNode<I, A>
    {
        #region Declaration
        private PrettyPredicate<I> _predicate;
        private IDecisionNode<I, A> _true, _false;
        #endregion

        /// <summary>Builds a decision tree node split according to a predicate</summary>
        /// <param name="predicate">the splitting predicate</param>
        /// <param name="trueBranch">the tree subpart if the predicate is met</param>
        /// <param name="falseBranch">the tree subpart if the predicate is not met</param>
        public DecisionNode(PrettyPredicate<I> predicate, IDecisionNode<I, A> trueBranch, IDecisionNode<I, A> falseBranch)
        {
            _predicate = predicate;
            _true = trueBranch;
            _false = falseBranch;
        }

        #region Accessors

        /// <summary>Returns the splitting predicate</summary>
        public PrettyPredicate<I> Predicate
        {
            get { return _predicate; }
            set { _predicate = value; }
        }

        /// <summary>Clones the node</summary>
        public IDecisionNode<I, A> Clone
        {
            get { return new DecisionNode<I, A>(_predicate.Clone, _true.Clone, _false.Clone); }
        }

        /// <summary>Returns the tree subpart if the predicate is met </summary>
        public IDecisionNode<I, A> True
        {
            get { return _true; }
            set { _true = value; }
        }

        /// <summary>Returns the tree subpart if the predicate is not met</summary>
        public IDecisionNode<I, A> False
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
        public A Evaluate(I input)
        {
            return (_predicate.Predicate(input) ? _true : _false).Evaluate(input);
        }
    }

    /// <summary>Decision tree leaf</summary>
    /// <typeparam name="I">the input type</typeparam>
    /// <typeparam name="A">the answer type</typeparam>
    public class DecisionLeaf<I, A> : IDecisionNode<I, A>
    {
        private A _answer;

        /// <summary>Builds a decision tree leaf</summary>
        /// <param name="answer">the fixed answer</param>
        public DecisionLeaf(A answer)
        {
            _answer = answer;
        }

        #region Accessors

        /// <summary>Returns the answer assigned to the leaf</summary>
        public A Answer
        {
            get { return _answer; }
            set { _answer = value; }
        }

        /// <summary>Clones the node</summary>
        public IDecisionNode<I, A> Clone
        {
            get { return new DecisionLeaf<I, A>(_answer); }
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
        public A Evaluate(I input)
        {
            return _answer;
        }

    }

    /// <summary>Decorated predicate class</summary>
    /// <typeparam name="I">the input type</typeparam>
    public class PrettyPredicate<I>
    {
        #region Declarations
        private readonly Predicate<I> _predicate;
        private readonly string _description;
        #endregion

        /// <summary>Builds a decorated predicate class</summary>
        /// <param name="description">the description of the predicate</param>
        /// <param name="predicate">the predicate</param>
        public PrettyPredicate(string description, Predicate<I> predicate)
        {
            _description = description;
            _predicate = predicate;
        }

        #region Accessors

        /// <summary>Returns the description of the predicate</summary>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>The predicate</summary>
        public Predicate<I> Predicate
        {
            get { return _predicate; }
        }

        /// <summary>Returns a clone of the pretty predicate</summary>
        public PrettyPredicate<I> Clone
        {
            get { return new PrettyPredicate<I>(_description, _predicate); }
        }
        #endregion
    }
}

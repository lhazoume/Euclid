using System.Collections.Generic;
using System.Linq;

namespace Euclid.DataStructures
{
    /// <summary>Represents a tree node</summary>
    /// <typeparam name="T">The value type</typeparam>
    public class TreeNode<T>
    {
        #region Variables
        private T _value;
        private TreeNode<T> _left, _right, _parent;
        #endregion

        private TreeNode(TreeNode<T> left, TreeNode<T> right, TreeNode<T> parent, T value)
        {
            _left = left;
            _right = right;
            _parent = parent;
            _value = value;
        }

        #region Nodes
        /// <summary>Gets and sets the node's value</summary>
        public T Value
        {
            get => _value; set => _value = value;
        }

        /// <summary>Gets and sets the left node</summary>
        public TreeNode<T> Left
        {
            get => _left;
            set
            {
                _left = value;
                _left._parent = this;
            }
        }

        /// <summary>Gets and sets the right node</summary>
        public TreeNode<T> Right
        {
            get => _right;
            set
            {
                _right = value;
                _right._parent = this;
            }
        }

        /// <summary>Gets and sets the parent node</summary>
        public TreeNode<T> Parent
        {
            get => _parent; set => _parent = value;
        }

        /// <summary>Returns all the leaves</summary>
        public List<TreeNode<T>> Leaves
        {
            get
            {
                if (IsLeaf) return new List<TreeNode<T>>() { this };
                return _right.Leaves.Union(_left.Leaves).ToList();
            }
        }
        #endregion

        #region Information
        /// <summary>Indicates if the current node is the root node</summary>
        public bool IsRoot => _parent == null;

        /// <summary>Indicates if the current node is a leaf node</summary>
        public bool IsLeaf=> _left == null && _right == null; 
        #endregion

        #region Builders
        /// <summary>Builds a root tree node</summary>
        /// <param name="left">the left child node</param>
        /// <param name="right">the right child node</param>
        /// <returns>a tree node</returns>
        public static TreeNode<T> BuildRoot(TreeNode<T> left, TreeNode<T> right)
        {
            return new TreeNode<T>(left, right, null, default);
        }
        #endregion
    }
}

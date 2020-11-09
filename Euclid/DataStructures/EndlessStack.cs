using System;
using System.Collections.Generic;

namespace Euclid.DataStructures
{
    /// <summary>Represents an endless stack (i.e a stack that refills whenever it is empty)</summary>
    /// <typeparam name="T">the item type</typeparam>
    public sealed class EndlessStack<T>
    {
        #region Variables
        private readonly Func<int, IEnumerable<T>> _refiller;
        private readonly int _refillSize;
        private readonly Stack<T> _stack;
        #endregion

        /// <summary>Builds an endless stack</summary>
        /// <param name="refiller">the function to provide the refills</param>
        /// <param name="initialSize">the initial size of the stack</param>
        /// <param name="refillSize">the number of items to load when the stack is empty </param>
        public EndlessStack(Func<int, IEnumerable<T>> refiller, int initialSize, int refillSize)
        {
            if (initialSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialSize), "the initial size can not be negative");
            if (refillSize <= 1)
                throw new ArgumentOutOfRangeException(nameof(refillSize), "the auto-refill size should be at least 1");

            _refiller = refiller;
            _stack = new Stack<T>(refiller(initialSize));
            _refillSize = refillSize;
        }

        /// <summary>Reads and delete an item from the top of the stack (refills if needed)</summary>
        /// <returns>the top element</returns>
        public T Pop()
        {
            if (_stack.Count == 0)
                foreach (T item in _refiller(_refillSize))
                    _stack.Push(item);

            T result = _stack.Pop();
            return result;
        }
    }
}

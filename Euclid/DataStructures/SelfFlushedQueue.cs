using System.Collections.Generic;

namespace Euclid.DataStructures
{
    /// <summary>Queue that flushes itself whenever it reaches a given size</summary>
    /// <typeparam name="T">the template type of the data</typeparam>
    public class SelfFlushedQueue<T>
    {
        #region Declarations
        private readonly int _maximumSize;
        private readonly Queue<T> _queue;
        private readonly object _lock = new object();
        private readonly Flush _flush;
        #endregion

        /// <summary>Builds a <c>SelfFlushedQueue</c></summary>
        /// <param name="maximumSize">the maximum size of the Queue</param>
        /// <param name="flushCallBack">the method to use of the flushed data</param>
        public SelfFlushedQueue(int maximumSize, Flush flushCallBack)
        {
            _maximumSize = maximumSize;
            _queue = new Queue<T>(_maximumSize);
            _flush = flushCallBack;
        }

        /// <summary>Adds an item to the queue</summary>
        /// <param name="t">the item to add</param>
        public void Add(T t)
        {
            lock (_lock)
            {
                _queue.Enqueue(t);
                if (_queue.Count >= _maximumSize)
                    ForceFlush();
            }
        }

        /// <summary>
        /// Flushes the data stored in the queue
        /// </summary>
        public void ForceFlush()
        {
            T[] result = _queue.ToArray();
            _queue.Clear();
            _flush(result);
        }

        /// <summary>Delegate definition of the flush method</summary>
        /// <param name="content">the data to treat after flush</param>
        public delegate void Flush(T[] content);
    }
}

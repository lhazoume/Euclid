using System.Collections.Generic;

namespace Euclid.Objects
{
    public class SelfFlushedQueue<T>
    {
        private readonly int _maximumSize, _checkFrequency;
        private readonly Queue<T> _queue;
        private readonly object _lock = new object();
        private Flush _flush;
        private int _ctr;


        public SelfFlushedQueue(int maximumSize, int checkFrequency, Flush flushCallBack)
        {
            _maximumSize = maximumSize;
            _checkFrequency = checkFrequency;
            _queue = new Queue<T>(_maximumSize + _checkFrequency);
            _flush = flushCallBack;
            _ctr = 0;
        }

        public void Add(T t)
        {
            lock (_lock)
            {
                _ctr++;
                _queue.Enqueue(t);
                if (_ctr % _checkFrequency == 0)
                {
                    if (_queue.Count >= _maximumSize)
                    {
                        _ctr = 0;

                        T[] result = _queue.ToArray();
                        _queue.Clear();
                        _flush(result);
                    }
                }
            }
        }

        public delegate void Flush(T[] content);
    }
}

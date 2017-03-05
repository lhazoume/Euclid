using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Objects
{
    /// <summary>A two ways dictionary class</summary>
    /// <typeparam name="T1">the left hand side type</typeparam>
    /// <typeparam name="T2">the right hand side type</typeparam>
    public class Map<T1, T2>
        where T1 : IEquatable<T1>
        where T2 : IEquatable<T2>
    {
        #region Declarations
        private readonly object _lock = new object();
        private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private readonly Dictionary<T2, T1> _backward = new Dictionary<T2, T1>();
        #endregion

        #region Constructors
        private Map(Dictionary<T1, T2> forward, Dictionary<T2, T1> backward)
        {
            _forward = new Dictionary<T1, T2>(forward);
            _backward = new Dictionary<T2, T1>(backward);
        }

        /// <summary>Builds an empty map</summary>
        public Map() { }

        /// <summary>Builds a Map of </summary>
        /// <param name="values"></param>
        public Map(IEnumerable<Tuple<T1, T2>> values)
        {
            foreach (Tuple<T1, T2> tuple in values)
            {
                _forward.Add(tuple.Item1, tuple.Item2);
                _backward.Add(tuple.Item2, tuple.Item1);
            }
        }
        #endregion

        #region Enumerators
        /// <summary>Gets an enumerator for the left hand side</summary>
        public IEnumerator<T1> LeftEnumerator
        {
            get { return _forward.Keys.GetEnumerator(); }
        }

        /// <summary>Gets an enumerator for the right hand side </summary>
        public IEnumerator<T2> Enumerator
        {
            get { return _backward.Keys.GetEnumerator(); }
        }
        #endregion

        #region Add / remove
        /// <summary>Adds a pair to the map</summary>
        /// <param name="t1">the left hand side key</param>
        /// <param name="t2">the right hand side key</param>
        public void Add(T1 t1, T2 t2)
        {
            lock (_lock)
            {
                _forward.Add(t1, t2);
                _backward.Add(t2, t1);
            }
        }

        /// <summary>Removes a pair from the map</summary>
        /// <param name="t1">the left hand side key</param>
        /// <param name="t2">the right hand side key</param>
        public void Remove(T1 t1, T2 t2)
        {
            lock (_lock)
            {
                _forward.Remove(t1);
                _backward.Remove(t2);
            }
        }

        /// <summary>Sets the right hand side value for a given left hand side value</summary>
        /// <param name="key">the left hand side key</param>
        /// <param name="newValue">the new right hand side value</param>
        public void SetForward(T1 key, T2 newValue)
        {
            lock (_lock)
            {
                T2 oldValue = _forward[key];
                _forward[key] = newValue;
                _backward.Remove(oldValue);
                _backward[newValue] = key;
            }
        }

        /// <summary>Sets the left hand side value for a given right hand side value</summary>
        /// <param name="key">the right hand side key</param>
        /// <param name="newValue">the new left right hand side value</param>
        public void SetBackward(T2 key, T1 newValue)
        {
            lock (_lock)
            {
                T1 oldValue = _backward[key];
                _backward[key] = newValue;
                _forward.Remove(oldValue);
                _forward[newValue] = key;
            }
        }
        #endregion

        #region Contains
        /// <summary>Checks if the left hand side contains a key</summary>
        /// <param name="t1">the left hand side key</param>
        /// <returns><c>True</c> if this is a left hand side key, <c>False</c> otherwise</returns>
        public bool ContainsKey(T1 t1)
        {
            return _forward.ContainsKey(t1);
        }

        /// <summary>Checks if the right hand side contains a key</summary>
        /// <param name="t2">the right hand side key</param>
        /// <returns><c>True</c> if this a right hand side key, <c>False</c> otherwise</returns>
        public bool ContainsValue(T2 t2)
        {
            return _backward.ContainsKey(t2);
        }
        #endregion

        #region Access
        /// <summary>Gets the right hand side value associated to a left hand side key</summary>
        /// <param name="t1">The left hand side key</param>
        /// <returns>a right hand side key</returns>
        public T2 Forward(T1 t1)
        {
            T2 rslt;
            lock (_lock) rslt = _forward[t1];
            return rslt;
        }

        /// <summary>Gets the left hand side value associated to a right hand side key</summary>
        /// <param name="t2">The right hand side key</param>
        /// <returns>a left hand side key</returns>
        public T1 Backward(T2 t2)
        {
            T1 rslt;
            lock (_lock) rslt = _backward[t2];
            return rslt;
        }

        /// <summary>Gets the left hand side keys</summary>
        public T1[] Lefts
        {
            get { return _forward.Keys.ToArray(); }
        }

        /// <summary>Gets the right hand side keys</summary>
        public T2[] Rights
        {
            get { return _backward.Keys.ToArray(); }
        }

        #endregion

        /// <summary>Gets the number of pairs in the map</summary>
        public int Count
        {
            get { return _forward.Count; }
        }

        /// <summary>Gets a deep copy of the map</summary>
        public Map<T1, T2> Clone()
        {
            Map<T1, T2> map;
            lock (_lock)
                map = new Map<T1, T2>(_forward, _backward);
            return map;
        }
    }
}

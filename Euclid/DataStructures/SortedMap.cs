using System;
using System.Collections.Generic;

namespace Euclid.DataStructures
{
    /// <summary>
    /// A two ways sorted dictionary class
    /// </summary>
    /// <typeparam name="T1">the left hand side type</typeparam>
    /// <typeparam name="T2">the right hand side type</typeparam>
    public sealed class SortedMap<T1, T2> : Map<T1, T2> where T1 : IEquatable<T1> where T2 : IEquatable<T2>
    {
        #region constructor
        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="forward">forward field</param>
        /// <param name="backward">backward field</param>
        private SortedMap(IDictionary<T1, T2> forward, IDictionary<T2, T1> backward)
        {
            _forward = new SortedDictionary<T1, T2>(forward);
            _backward = new SortedDictionary<T2, T1>(backward);
        }
        /// <summary>
        /// Empty sorted map
        /// </summary>
        public SortedMap()
        {
            _forward = new SortedDictionary<T1, T2>();
            _backward = new SortedDictionary<T2, T1>();
        }
        #endregion

        #region methods
        /// <summary>Gets a deep copy of the sorted map</summary>
        public override Map<T1, T2> Clone()
        {
            SortedMap<T1, T2> map;
            lock (_lock) map = new SortedMap<T1, T2>(_forward, _backward);
            return map;
        }
        #endregion
    }
}

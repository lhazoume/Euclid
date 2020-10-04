using System;
using System.Collections.Generic;

namespace Euclid.DataStructures
{
    /// <summary>Class allowing to cache calculation results</summary>
    /// <typeparam name="T">the key type</typeparam>
    /// <typeparam name="U">the value type</typeparam>
    public class CachedFunction<T, U> where T : IEquatable<T>
    {
        private readonly Dictionary<T, U> _cache = new Dictionary<T, U>();
        private readonly Func<T, U> _function;

        /// <summary>Builds a cached function class</summary>
        /// <param name="function">the function</param>
        public CachedFunction(Func<T, U> function)
        {
            _function = function;
        }

        /// <summary>Returns the value for a given key (if not stored, calculates it and stores it)</summary>
        /// <param name="t">the target key</param>
        /// <returns>the sought value</returns>
        public U this[T t]
        {
            get
            {
                if (_cache.ContainsKey(t))
                    return _cache[t];
                U u = _function(t);
                _cache.Add(t, u);
                return u;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>Header class for the IIndexedSeries</summary>
    /// <typeparam name="T">the type of label</typeparam>
    public class Header<T> where T : IEquatable<T>
    {
        #region Declarations
        private Map<T, int> _map;
        #endregion

        #region Constructors

        /// <summary>Standard builder</summary>
        public Header()
        {
            _map = new Map<T, int>();
        }

        /// <summary>Builds a zero-indexed header</summary>
        /// <param name="content">the values of the labels</param>
        public Header(IEnumerable<T> content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            _map = new Map<T, int>();
            int i = 0;
            foreach(T item in content)
            {
                _map.Add(item, i);
                i++;
            }
        }


        /// <summary>Builds a header based on a single item (for slices or series)</summary>
        /// <param name="unique">the unique value</param>
        public Header(T unique)
        {
            if (unique == null) throw new ArgumentNullException(nameof(unique));
            _map = new Map<T, int>();
            _map.Add(unique, 0);
        }

        /// <summary> Builds a header from a two-way dictionary</summary>
        /// <param name="map">the map</param>
        private Header(Map<T, int> map)
        {
            _map = map.Clone();
        }
        #endregion

        #region Accessors

        /// <summary>Gets the index associated with a label</summary>
        /// <param name="t">the label</param>
        /// <returns>an integer</returns>
        public int this[T t] => _map.Forward(t);

        /// <summary>Gets the number of labels in the header</summary>
        public int Count => _map.Count;

        /// <summary>Gets the labels</summary>
        public T[] Values => _map.Rights.OrderBy(i => i).Select(i => _map.Backward(i)).ToArray();
        #endregion

        #region Methods

        /// <summary>Removes a label from the header</summary>
        /// <param name="t">the label</param>
        public void Remove(T t)
        {
            int index = _map.Forward(t);
            _map.Remove(t, index);
            for (int j = index + 1; j <= _map.Count; j++)
                _map.SetForward(_map.Backward(j), j - 1);
        }

        /// <summary>Adds a label to the header</summary>
        /// <param name="t">the label</param>
        public void Add(T t)
        {
            _map.Add(t, _map.Count);
        }


        /// <summary>Adds a range of values to the header</summary>
        /// <param name="ts">the values</param>
        public void AddRange(params T[] ts)
        {
            foreach (T t in ts)
                Add(t);
        }

        /// <summary>Renames a label</summary>
        /// <param name="oldValue">the old value of the label</param>
        /// <param name="newValue">the new value of the label</param>
        public void Rename(T oldValue, T newValue)
        {
            _map.SetBackward(_map.Forward(oldValue), newValue);
        }

        /// <summary>Gets the i-th legend value</summary>
        /// <param name="index">the index</param>
        /// <returns>a label</returns>
        public T ElementAt(int index)
        {
            return _map.Backward(index);
        }

        /// <summary>Checks if the header contains a given label</summary>
        /// <param name="t">the target label</param>
        /// <returns>true if the label is in the header, false otherwise</returns>
        public bool Contains(T t)
        {
            return _map.ContainsForwardKey(t);
        }

        /// <summary>Clones the header</summary>
        /// <returns>a Header</returns>
        public Header<T> Clone()
        {
            return new Header<T>(_map);
        }
        #endregion
    }
}

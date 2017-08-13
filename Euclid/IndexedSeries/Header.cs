using Euclid.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.IndexedSeries
{
    /// <summary>Header class for the IIndexe</summary>
    /// <typeparam name="T">the type of label</typeparam>
    public class Header<T> : IEnumerable<T> where T : IEquatable<T>
    {
        private Map<T, int> _map;

        #region Constructors

        public Header()
        {
            _map = new Map<T, int>();
        }

        /// <summary>Builds a zero-indexed header</summary>
        /// <param name="content">the values of the labels</param>
        public Header(IList<T> content)
        {
            _map = new Map<T, int>();
            for (int i = 0; i < content.Count; i++)
                _map.Add(content[i], i);
        }

        /// <summary> Builds a header from a two-way dictionary</summary>
        /// <param name="map">the map</param>
        private Header(Map<T, int> map)
        {
            _map = map.Clone();
        }
        #endregion

        #region IEnumerable
        /// <summary>Gets an enumerator of the labels</summary>
        /// <returns>a IEnumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _map.ForwardEnumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Accessors
        /// <summary>Gets the index associated with a label</summary>
        /// <param name="t">the label</param>
        /// <returns>an integer</returns>
        public int this[T t]
        {
            get { return _map.Forward(t); }
        }

        /// <summary>Gets the number of labels in the header</summary>
        public int Count
        {
            get { return _map.Count; }
        }

        /// <summary>Gets the labels</summary>
        public T[] Values
        {
            get { return _map.Rights.OrderBy(i => i).Select(i => _map.Backward(i)).ToArray(); }
        }
        #endregion

        #region Methods

        /// <summary>Clones the header</summary>
        /// <returns>a Header</returns>
        public Header<T> Clone()
        {
            return new Header<T>(_map);
        }

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

        /// <summary>Returns the label at a given index</summary>
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

        #endregion

        #region Static Operators
        /// <summary>Equality operator</summary>
        /// <param name="h1">the left-hand-side header</param>
        /// <param name="h2">the right-hand-side header</param>
        /// <returns>true if the headers have the same content, false otherwise</returns>
        public static bool operator ==(Header<T> h1, Header<T> h2)
        {
            if (h1.Count != h2.Count) return false;
            return h1._map.Lefts.SequenceEqual(h2._map.Lefts);
        }

        /// <summary>Inequality operator</summary>
        /// <param name="h1">the left-hand-side header</param>
        /// <param name="h2">the right-hand-side header</param>
        /// <returns>true if the headers do not have the same content, false otherwise</returns>
        public static bool operator !=(Header<T> h1, Header<T> h2)
        {
            if (h1.Count != h2.Count) return true;
            return !h1._map.Lefts.SequenceEqual(h2._map.Lefts);
        }
        #endregion
    }
}

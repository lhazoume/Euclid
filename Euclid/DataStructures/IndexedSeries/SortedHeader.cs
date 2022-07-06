using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>
    /// Sorted Header class for the IIndexe
    /// </summary>
    /// <typeparam name="T">the type of label</typeparam>
    public sealed class SortedHeader<T> : IEnumerable<T>, IEquatable<SortedHeader<T>> where T : IEquatable<T>
    {
        #region Variables
        private Map<T, int> _map;
        #endregion


        #region Constructors
        /// <summary>Standard builder</summary>
        public SortedHeader()
        {
            _map = new SortedMap<T, int>();
        }

        /// <summary>Builds a zero-indexed header</summary>
        /// <param name="content">the values of the labels</param>
        public SortedHeader(IList<T> content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            _map = new SortedMap<T, int>();
            for (int i = 0; i < content.Count; i++)
                _map.Add(content[i], i);
        }

        /// <summary> Builds a header from a two-way dictionary</summary>
        /// <param name="map">the map</param>
        private SortedHeader(Map<T, int> map)
        {
            _map = map.Clone();
        }
        #endregion       

        #region Accessors
        /// <summary>Gets the labels</summary>
        public T[] Values => _map.Lefts;

        /// <summary>Gets the number of labels in the header</summary>
        public int Count => _map.Count;

        #endregion

        #region Methods

        #region IEnumerable
        /// <summary>Gets an enumerator of the labels</summary>
        /// <returns>a IEnumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _map.ForwardEnumerator;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IEquatable
        /// <summary>Overriden generic Equals</summary>
        /// <param name="obj">the compared object</param>
        /// <returns>true if obj matches this object, false otherwise</returns>
        public override bool Equals(object obj)
        {
            SortedHeader<T> item = obj as SortedHeader<T>;
            if (item == null)
                return false;
            return Equals(item);
        }

        /// <summary>Returns a hash code</summary>
        /// <returns>an <c>Integer</c></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>Equality comparer</summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SortedHeader<T> other)
        {
            if (other == null || this.Count != other.Count) return false;
            return _map.Lefts.SequenceEqual(other._map.Lefts) && _map.Rights.SequenceEqual(other._map.Rights);
        }


        /// <summary>Equality operator</summary>
        /// <param name="h1">the left-hand-side header</param>
        /// <param name="h2">the right-hand-side header</param>
        /// <returns>true if the headers have the same content, false otherwise</returns>
        public static bool operator ==(SortedHeader<T> h1, SortedHeader<T> h2)
        {
            if (h1 == null && h2 == null)
                return true;
            else if (h1 == null)
                return false;
            else if (h2 == null)
                return false;
            else
                return h1.Equals(h2);
        }

        /// <summary>Inequality operator</summary>
        /// <param name="h1">the left-hand-side header</param>
        /// <param name="h2">the right-hand-side header</param>
        /// <returns>true if the headers do not have the same content, false otherwise</returns>
        public static bool operator !=(SortedHeader<T> h1, SortedHeader<T> h2)
        {
            return !(h1 == h2);
        }
        #endregion

        /// <summary>Clones the header</summary>
        /// <returns>a Header</returns>
        public SortedHeader<T> Clone()
        {
            return new SortedHeader<T>(_map);
        }
        #endregion
    }
}

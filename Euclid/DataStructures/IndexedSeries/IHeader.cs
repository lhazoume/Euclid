using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.DataStructures.IndexedSeries
{
    /*
    /// <summary>
    /// Abstract Header class for the IIndexe
    /// </summary>
    /// <typeparam name="T">the type of label</typeparam>
    public abstract class I_Header<T> : IEnumerable<T>, IEquatable<IHeader<T>> where T : IEquatable<T>
    {
        #region vars
        /// <summary>
        /// map field
        /// </summary>
        protected IMap<T, int> _map;
        #endregion

        #region Accessors
        /// <summary>Gets the index associated with a label</summary>
        /// <param name="t">the label</param>
        /// <returns>an integer</returns>
        public int this[T t] => _map.Forward(t);

        /// <summary>Gets the number of labels in the header</summary>
        public int Count => _map.Count;

        /// <summary>Gets the labels</summary>
        public abstract T[] Values { get; }
        #endregion

        #region methods


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

        /// <summary>Clones the header</summary>
        /// <returns>a Header</returns>
        public abstract IHeader<T> Clone();

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

        #region Static Operators
        /// <summary>Overriden generic Equals</summary>
        /// <param name="obj">the compared object</param>
        /// <returns>true if obj matches this object, false otherwise</returns>
        public override bool Equals(object obj)
        {
            IHeader<T> item = obj as IHeader<T>;
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
        public bool Equals(IHeader<T> other)
        {
            if (other == null || this.Count != other.Count) return false;
            return _map.Lefts.SequenceEqual(other._map.Lefts) && _map.Rights.SequenceEqual(other._map.Rights);
        }


        /// <summary>Equality operator</summary>
        /// <param name="h1">the left-hand-side header</param>
        /// <param name="h2">the right-hand-side header</param>
        /// <returns>true if the headers have the same content, false otherwise</returns>
        public static bool operator ==(IHeader<T> h1, IHeader<T> h2)
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
        public static bool operator !=(IHeader<T> h1, IHeader<T> h2)
        {
            return !(h1 == h2);
        }
        #endregion
        #endregion
    }
    */
}

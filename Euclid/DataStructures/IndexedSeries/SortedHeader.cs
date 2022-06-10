using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>
    /// Sorted Header class for the IIndexe
    /// </summary>
    /// <typeparam name="T">the type of label</typeparam>
    public sealed class SortedHeader<T> : IHeader<T>, IEnumerable<T>, IEquatable<IHeader<T>> where T : IEquatable<T>
    {
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
        private SortedHeader(IMap<T, int> map)
        {
            _map = map.Clone();
        }
        #endregion       

        #region Accessors
        /// <summary>Gets the labels</summary>
        public override T[] Values => _map.Lefts;
        #endregion

        #region Methods

        /// <summary>Clones the header</summary>
        /// <returns>a Header</returns>
        public override IHeader<T> Clone()
        {
            return new SortedHeader<T>(_map);
        }
        #endregion
    }
}

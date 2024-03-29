﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>Header class for the IIndexe</summary>
    /// <typeparam name="T">the type of label</typeparam>
    public class Header<T> : IHeader<T>, IEnumerable<T>, IEquatable<IHeader<T>> where T : IEquatable<T>
    {
        #region Constructors

        /// <summary>Standard builder</summary>
        public Header()
        {
            _map = new Map<T, int>();
        }

        /// <summary>Builds a zero-indexed header</summary>
        /// <param name="content">the values of the labels</param>
        public Header(IList<T> content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            _map = new Map<T, int>();
            for (int i = 0; i < content.Count; i++)
                _map.Add(content[i], i);
        }

        /// <summary> Builds a header from a two-way dictionary</summary>
        /// <param name="map">the map</param>
        private Header(IMap<T, int> map)
        {
            _map = map.Clone();
        }
        #endregion       

        #region Accessors
        /// <summary>Gets the labels</summary>
        public override T[] Values => _map.Rights.OrderBy(i => i).Select(i => _map.Backward(i)).ToArray();
        #endregion

        #region Methods

        /// <summary>Clones the header</summary>
        /// <returns>a Header</returns>
        public override IHeader<T> Clone()
        {
            return new Header<T>(_map);
        }
        #endregion
    }
}

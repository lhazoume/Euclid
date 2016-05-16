using Euclid.Serialization;
using System;

namespace Euclid.IndexedSeries
{
    public interface IIndexedSeries<T, U, V> : IXmlable, ICSVable where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>
    {
        T[] Legends { get; }
        V[] Labels { get; }
        int Rows { get; }
        int Columns { get; }
    }
}

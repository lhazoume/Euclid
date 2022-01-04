using System;

namespace Euclid.Histograms
{
    /// <summary>
    /// Bound class : mainly used in intervals
    /// </summary>
    public class Bound : IComparable<Bound>, IEquatable<Bound>
    {
        #region Declarations
        private readonly double _value;
        private readonly bool _isIncluded;
        #endregion

        #region Constructors
        /// <summary>
        /// Builds a bound
        /// </summary>
        /// <param name="value">the value</param>
        /// <param name="isIncluded">specifies whether this value is included or not</param>
        public Bound(double value, bool isIncluded = true)
        {
            _value = value;
            if (double.IsPositiveInfinity(_value) || double.IsNegativeInfinity(_value))
                _isIncluded = false;
            else
                _isIncluded = value != double.PositiveInfinity && value != double.NegativeInfinity && isIncluded;
        }
        #endregion

        #region Accessors
        /// <summary>Gets the bound's value</summary>
        public double Value => _value;

        /// <summary>Gets the bound's status</summary>
        public bool IsIncluded => _isIncluded;
        #endregion

        #region Operators
        /// <summary>Checks the equality between two bounds (reference equality then null coincidence, then content coincidence)</summary>
        /// <param name="b1">the left hand side <c>Bound</c></param>
        /// <param name="b2">the right hand side <c>Bound</c></param>
        /// <returns>true if equal, false otherwise</returns>
        public static bool operator ==(Bound b1, Bound b2)
        {
            if (ReferenceEquals(b1, b2)) return true;
            if (b1 is null && b2 is null) return true;
            else if (b1 is null || b2 is null) return false;
            else return b1._isIncluded == b2._isIncluded && b1._value == b2._value;
        }

        /// <summary>Checks the inequality between two bounds (based on the equal comparer)</summary>
        /// <param name="b1">the left hand side <c>Bound</c></param>
        /// <param name="b2">the right hand side <c>Bound</c></param>
        /// <returns>false if equal, true otherwise</returns>
        public static bool operator !=(Bound b1, Bound b2)
        {
            return !(b1 == b2);
        }

        /// <summary>Compares two Bounds</summary>
        /// <param name="b1">the left hand side Bound</param>
        /// <param name="b2">the right hand side Bound</param>
        /// <returns><c>true</c> if b1 is lower, <c>false</c> otherwise</returns>
        public static bool operator <(Bound b1, Bound b2)
        {
            if (b1 == null) throw new ArgumentNullException(nameof(b1));
            if (b2 == null) throw new ArgumentNullException(nameof(b2));

            return b1._value < b2._value;
        }

        /// <summary>Compares two Bounds</summary>
        /// <param name="b1">the left hand side Bound</param>
        /// <param name="b2">the right hand side Bound</param>
        /// <returns><c>true</c> if b1 is higher, <c>false</c> otherwise</returns>
        public static bool operator >(Bound b1, Bound b2)
        {
            if (b1 == null) throw new ArgumentNullException(nameof(b1));
            if (b2 == null) throw new ArgumentNullException(nameof(b2));

            return b1._value > b2._value;
        }

        /// <summary>
        /// Compares two Bounds
        /// </summary>
        /// <param name="b1">the left hand side Bound</param>
        /// <param name="b2">the right hand side Bound</param>
        /// <returns><c>true</c> if b1 is lower or equal, <c>false</c> otherwise</returns>
        public static bool operator <=(Bound b1, Bound b2)
        {
            return !(b1 > b2);
        }

        /// <summary>
        /// Compares two Bounds
        /// </summary>
        /// <param name="b1">the left hand side Bound</param>
        /// <param name="b2">the right hand side Bound</param>
        /// <returns><c>true</c> if b1 is greater or equal, <c>false</c> otherwise</returns>
        public static bool operator >=(Bound b1, Bound b2)
        {
            return !(b1 < b2);
        }
        #endregion

        #region IEquatable
        /// <summary>Checks the equality to another <c>Bound</c></summary>
        /// <param name="other">the other <c>Bound</c></param>
        /// <returns>a bool</returns>
        public bool Equals(Bound other)
        {
            return this == other;
        }

        /// <summary>Checks the equality to an object (inherited from IEquatable)</summary>
        /// <param name="other">the object </param>
        /// <returns>a bool</returns>
        public override bool Equals(object other)
        {
            return this == (Bound)other;
        }

        /// <summary>Returns this instance's hashcode</summary>
        /// <returns>an int</returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode() ^ _isIncluded.GetHashCode();
        }
        #endregion

        #region IComparable
        /// <summary> Compares this instance to another <c>Bound</c>
        /// </summary>
        /// <param name="other">the <c>Bound</c> to compare to</param>
        /// <returns>-1 if &lt;, +1 if &gt;, 0 otherwise</returns>
        public int CompareTo(Bound other)
        {
            if (this < other) return -1;
            else if (this > other) return +1;
            else return 0;
        }
        #endregion
    }
}

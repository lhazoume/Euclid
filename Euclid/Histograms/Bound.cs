using System;

namespace Euclid.Histograms
{
    /// <summary>
    /// Bound class : mainly used in intervals
    /// </summary>
    public class Bound : IComparable<Bound>, IEquatable<Bound>
    {
        #region Declarations
        private double _value;
        private bool _isIncluded;
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
                _isIncluded = (value == double.PositiveInfinity || value == double.NegativeInfinity) ? false : isIncluded;
        }
        #endregion

        #region Accessors
        /// <summary>
        /// Gets the bound's value
        /// </summary>
        public double Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the bound's status
        /// </summary>
        public bool IsIncluded
        {
            get { return _isIncluded; }
        }
        #endregion

        #region Operators
        /// <summary>
        /// Compares a <c>Bound</c> to a double
        /// </summary>
        /// <param name="b">the left hand side <c>Bound</c></param>
        /// <param name="x">the right hand side double</param>
        /// <returns><c>true</c> if the Bound is lower, <c>false</c> otherwise</returns>
        public static bool operator <(Bound b, double x)
        {
            return b._value < x;
        }

        /// <summary>
        /// Compares a double to a <c>Bound</c>
        /// </summary>
        /// <param name="x">the left hand side double</param>
        /// <param name="b">the right hand side <c>Bound</c> </param>
        /// <returns><c>true</c> if the Bound if higher, <c>false</c> otherwise</returns>
        public static bool operator <(double x, Bound b)
        {
            return b > x;
        }

        /// <summary>
        /// Compares a Bound to a double
        /// </summary>
        /// <param name="b">the left hand side <c>Bound</c></param>
        /// <param name="x">the right hand side double</param>
        /// <returns><c>true</c> if the Bound is higher, <c>false</c> otherwise </returns>
        public static bool operator >(Bound b, double x)
        {
            return b.Value > x;
        }

        /// <summary>
        /// Compares a Bound to a double
        /// </summary>
        /// <param name="x">the left hand side double</param>
        /// <param name="b">the right hand side Bound</param>
        /// <returns><c>true</c> if the Bound is lower, <c>false</c> otherwise</returns>
        public static bool operator >(double x, Bound b)
        {
            return b < x;
        }

        /// <summary>
        /// Compares a Bound to a double
        /// </summary>
        /// <param name="b">the left hand side Bound</param>
        /// <param name="x">the right hand side double</param>
        /// <returns><c>true</c> if Bound is lower or equal, <c>false</c> otherwise</returns>
        public static bool operator <=(Bound b, double x)
        {
            return b._isIncluded ? b._value < x : b._value <= x;
        }

        /// <summary>
        /// Compares a Bound to a double
        /// </summary>
        /// <param name="b">the left hand side Bound</param>
        /// <param name="x">the right hand side double</param>
        /// <returns><c>true</c> if Bound is greater or equal, <c>false</c> otherwise </returns>
        public static bool operator >=(Bound b, double x)
        {
            return b._isIncluded ? b._value > x : b._value >= x;
        }

        /// <summary>
        /// Compares a double to a Bound
        /// </summary>
        /// <param name="x">the left hand side double</param>
        /// <param name="b">the right hand side Bound</param>
        /// <returns><c>true</c> if the Bound is greater or equal, <c>false</c> otherwise</returns>
        public static bool operator <=(double x, Bound b)
        {
            return b >= x;
        }

        /// <summary>
        /// Compares a double to a Bound
        /// </summary>
        /// <param name="x">the left hand side double</param>
        /// <param name="b">the right hand side Bound</param>
        /// <returns><c>true</c> if the Bound is lower or equal, <c>false</c> otherwise</returns>
        public static bool operator >=(double x, Bound b)
        {
            return b <= x;
        }

        /// <summary>
        /// Checks if a bound equals a double
        /// </summary>
        /// <param name="b">the left hand side Bound</param>
        /// <param name="x">the right hand side double</param>
        /// <returns><c>true</c> if they equal, <c>false</c> otherwise</returns>
        public static bool operator ==(Bound b, double x)
        {
            return b._isIncluded ? false : b._value == x;
        }

        /// <summary>
        /// Checks if a bound equals a double
        /// </summary>
        /// <param name="b">the right hand side Bound</param>
        /// <param name="x">the left hand side double</param>
        /// <returns><c>true</c> if they equal, <c>false</c> otherwise</returns>
        public static bool operator ==(double x, Bound b)
        {
            return b == x;
        }

        /// <summary>
        /// Compares a Bound to a double
        /// </summary>
        /// <param name="b">the left hand side Bound</param>
        /// <param name="x">the right hand side double</param>
        /// <returns><c>true</c> if the Bound doesnot match, <c>false</c> otherwise</returns>
        public static bool operator !=(Bound b, double x)
        {
            return b._value != x;
        }

        /// <summary>
        /// Compares a Bound to a double
        /// </summary>
        /// <param name="x">the left hand side double</param>
        /// <param name="b">the right hand side Bound</param>
        /// <returns><c>true</c> if the Bound doesnt match, <c>false</c> otherwise</returns>
        public static bool operator !=(double x, Bound b)
        {
            return b != x;
        }

        /// <summary>
        /// Checks the equality between two bounds (reference equality then null coincidence, then content coincidence)
        /// </summary>
        /// <param name="b1">the left hand side <c>Bound</c></param>
        /// <param name="b2">the right hand side <c>Bound</c></param>
        /// <returns>a bool</returns>
        public static bool operator ==(Bound b1, Bound b2)
        {
            if (Object.ReferenceEquals(b1, b2)) return true;
            if ((object)b1 == null && (object)b2 == null) return true;
            else if ((object)b1 == null || (object)b2 == null) return false;
            else return b1._isIncluded == b2._isIncluded && b1._value == b2._value;
        }

        /// <summary>
        /// Checks the inequality between two bounds (based on the equal comparer)
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
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
            return b1._value < b2._value;
        }

        /// <summary>
        /// Compares two Bounds
        /// </summary>
        /// <param name="b1">the left hand side Bound</param>
        /// <param name="b2">the right hand side Bound</param>
        /// <returns><c>true</c> if b1 is higher, <c>false</c> otherwise</returns>
        public static bool operator >(Bound b1, Bound b2)
        {
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
        /// <summary>
        /// Checks the equality to another <c>Bound</c>
        /// </summary>
        /// <param name="other">the other <c>Bound</c></param>
        /// <returns>a bool</returns>
        public bool Equals(Bound other)
        {
            return this == other;
        }

        /// <summary>
        /// Checks the equality to an object (inherited from IEquatable)
        /// </summary>
        /// <param name="other">the object </param>
        /// <returns>a bool</returns>
        public override bool Equals(object other)
        {
            return this == (Bound)other;
        }

        /// <summary>
        /// Returns this instance's hashcode
        /// </summary>
        /// <returns>an int</returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode() ^ _isIncluded.GetHashCode();
        }
        #endregion

        #region IComparable
        /// <summary>
        /// Compares this instance to another <c>Bound</c>
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

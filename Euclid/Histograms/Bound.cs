using System;

namespace Euclid.Histograms
{
    public class Bound
    {
        #region Declarations
        private double _value;
        private bool _isIncluded;
        #endregion  

        public Bound(double value, bool isIncluded = true)
        {
            _value = value;
            _isIncluded = (value == double.PositiveInfinity || value == double.NegativeInfinity) ? false : isIncluded;
        }

        #region Accessors
        public double Value
        {
            get { return _value; }
        }
        public bool IsIncluded
        {
            get { return _isIncluded; }
        }
        #endregion

        #region Operators
        public static bool operator <(Bound b, double x)
        {
            return b._value < x;
        }
        public static bool operator <(double x, Bound b)
        {
            return b > x;
        }
        public static bool operator >(Bound b, double x)
        {
            return b.Value > x;
        }
        public static bool operator >(double x, Bound b)
        {
            return b < x;
        }

        public static bool operator <=(Bound b, double x)
        {
            return b._isIncluded ? b._value < x : b._value <= x;
        }
        public static bool operator >=(Bound b, double x)
        {
            return b._isIncluded ? b._value > x : b._value >= x;
        }
        public static bool operator <=(double x, Bound b)
        {
            return b >= x;
        }
        public static bool operator >=(double x, Bound b)
        {
            return b <= x;
        }

        public static bool operator ==(Bound b, double x)
        {
            return b._isIncluded ? false : b._value == x;
        }
        public static bool operator ==(double x, Bound b)
        {
            return b == x;
        }
        public static bool operator !=(Bound b, double x)
        {
            return b._value != x;
        }
        public static bool operator !=(double x, Bound b)
        {
            return b != x;
        }

        public static bool operator ==(Bound b1, Bound b2)
        {
            if (Object.ReferenceEquals(b1, b2)) return true;
            if ((object)b1 == null && (object)b2 == null) return true;
            else if ((object)b1 == null || (object)b2 == null) return false;
            else return b1._isIncluded == b2._isIncluded && b1._value == b2._value;
        }
        public static bool operator !=(Bound b1, Bound b2)
        {
            return !(b1 == b2);
        }

        public static bool operator <(Bound b1, Bound b2)
        {
            return b1._value < b2._value;
        }
        public static bool operator >(Bound b1, Bound b2)
        {
            return b1._value > b2._value;
        }
        public static bool operator <=(Bound b1, Bound b2)
        {
            return !(b1._value > b2._value);
        }
        public static bool operator >=(Bound b1, Bound b2)
        {
            return !(b1 < b2);
        }
        #endregion

        #region Equals
        public bool Equals(Bound other)
        {
            return this == other;
        }
        public override bool Equals(object other)
        {
            return this == (Bound)other;
        }
        public override int GetHashCode()
        {
            return 0;
        }
        #endregion
    }
}

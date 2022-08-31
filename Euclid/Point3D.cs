using System;

namespace Euclid
{
    /// <summary>Represents a 2-dimensional point</summary>
    public struct Point3D : IEquatable<Point3D>
    {
        #region Private & public fields
        /// <summary>Gets and sets the point's abscissa</summary>
        public double X;
        /// <summary>Gets and sets the point's ordinate</summary>
        public double Y;
        /// <summary>Gets and sets the point's depth</summary>
        public double Z;
        #endregion

        #region Constructors
        /// <summary>Builds a 2-dimensional point</summary>
        /// <param name="x">the abscissa</param>
        /// <param name="y">the ordinate</param>
        /// <param name="z">the depth</param>
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        #endregion

        #region Methods
        /// <summary>Checks if two points are congruent</summary>
        /// <param name="other">the alternate point</param>
        /// <returns><c>true</c> if congruent, <c>false</c> otherwise</returns>
        public bool Equals(Point3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        #endregion
    }
}

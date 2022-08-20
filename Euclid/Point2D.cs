using System;

namespace Euclid
{
    /// <summary>Represents a 2-dimensional point</summary>
    public struct Point2D : IEquatable<Point2D>
    {
        #region Private & public fields
        /// <summary>Gets and sets he point's abscissa</summary>
        public double X;
        /// <summary>Gets and sets he point's ordinate</summary>
        public double Y;
        #endregion

        #region Constructors
        /// <summary>Builds a 2-dimensional point</summary>
        /// <param name="x">the abscissa</param>
        /// <param name="y">the ordinate</param>
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }
        #endregion

        #region Methods
        /// <summary>Checks if two points are congruent</summary>
        /// <param name="other">the alternate point</param>
        /// <returns><c>true</c> if congruent, <c>false</c> otherwise</returns>
        public bool Equals(Point2D other)
        {
            return X == other.X && Y == other.Y;
        }
        #endregion
    }
}

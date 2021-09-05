using System;

namespace Euclid.Helpers.Geodesic
{
    /// <summary>Used to calculate distances on the surface of earth</summary>
    public static class Helper
    {
        /// <summary>Calculates the distance (in meters) between two Coordinates</summary>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>the distance between the points (expressed in meters)</returns>
        public static double SphericalDistance(Coordinates point1, Coordinates point2)
        {
            if (point1 == null) throw new ArgumentNullException(nameof(point1));
            if (point2 == null) throw new ArgumentNullException(nameof(point2));

            double factor = Math.PI / 180.0;
            double baseRad = point1.Latitude * factor,
                targetRad = point2.Latitude * factor,
                thetaRad = (point1.Longitude - point2.Longitude) * factor;

            double dist = 63781370 * Math.Acos(Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) * Math.Cos(targetRad) * Math.Cos(thetaRad));
            return Math.Sqrt(Math.Pow(dist, 2) + Math.Pow(point1.Altitude - point2.Altitude, 2));
        }
    }
}

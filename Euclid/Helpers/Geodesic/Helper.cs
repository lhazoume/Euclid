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
            double factor = Math.PI / 180.0;
            double baseRad = point1.Latitude * factor,
                targetRad = point2.Latitude * factor,
                thetaRad = (point1.Longitude - point2.Longitude) * factor;

            double dist = 63781370 * Math.Acos(Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) * Math.Cos(targetRad) * Math.Cos(thetaRad));
            return Math.Sqrt(Math.Pow(dist, 2) + Math.Pow(point1.Altitude - point2.Altitude, 2));
        }

        /// <summary>Calculates the distance (in meters) between two Coordinates</summary>
        /// <param name="ellipsoid">The ellipsoid representation of the Earth</param>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>the distance between the points (expressed in meters)</returns>
        public static double EllipsoidDistance(this Ellipsoid ellipsoid, Coordinates point1, Coordinates point2)
        {
            if (point1 == null) throw new ArgumentNullException("point1", "the point1 is null");
            if (point2 == null) throw new ArgumentNullException("point2", "the point1 is null");

            double factor = Math.PI / 180.0;
            // get constants
            double a = ellipsoid.EquatorialRadius;
            double b = ellipsoid.PolarRadius;
            double f = ellipsoid.Flattening;
            // get parameters as radians
            double phi1 = point1.Latitude * factor,
                lambda1 = point1.Longitude * factor,
                phi2 = point2.Latitude * factor,
                lambda2 = point2.Longitude * factor;
            // calculations
            double a2b2b2 = Math.Pow(a / b, 2) - 1;
            double omega = lambda2 - lambda1;
            double tanphi1 = Math.Tan(phi1),
                tanphi2 = Math.Tan(phi2);
            double tanU1 = (1.0 - f) * tanphi1,
                tanU2 = (1.0 - f) * tanphi2;
            double U1 = Math.Atan(tanU1),
                U2 = Math.Atan(tanU2);
            double sinU1 = Math.Sin(U1), cosU1 = Math.Cos(U1),
                sinU2 = Math.Sin(U2), cosU2 = Math.Cos(U2);
            double sinU1sinU2 = sinU1 * sinU2,
                cosU1sinU2 = cosU1 * sinU2,
                sinU1cosU2 = sinU1 * cosU2,
                cosU1cosU2 = cosU1 * cosU2;
            // eq. 13
            double lambda = omega,
                lambda0,
                A = 0.0, sigma = 0.0, deltasigma = 0.0;

            for (int i = 0; i < 10; i++)
            {
                lambda0 = lambda;
                double sinlambda = Math.Sin(lambda), coslambda = Math.Cos(lambda);
                // eq. 14
                double sin2sigma = (cosU2 * sinlambda * cosU2 * sinlambda) + (cosU1sinU2 - sinU1cosU2 * coslambda) * (cosU1sinU2 - sinU1cosU2 * coslambda);
                double sinsigma = Math.Sqrt(sin2sigma);
                // eq. 15
                double cossigma = sinU1sinU2 + (cosU1cosU2 * coslambda);
                // eq. 16
                sigma = Math.Atan2(sinsigma, cossigma);
                // eq. 17 Careful! sin2sigma might be almost 0!
                double sinalpha = (sin2sigma == 0) ? 0.0 : cosU1cosU2 * sinlambda / sinsigma;
                double alpha = Math.Asin(sinalpha),
                    cosalpha = Math.Cos(alpha),
                    cos2alpha = cosalpha * cosalpha;
                // eq. 18 Careful! cos2alpha might be almost 0!
                double cos2sigmam = cos2alpha == 0.0 ? 0.0 : cossigma - 2 * sinU1sinU2 / cos2alpha;
                double u2 = cos2alpha * a2b2b2;
                double cos2sigmam2 = cos2sigmam * cos2sigmam;
                // eq. 3
                A = 1.0 + u2 / 16384 * (4096 + u2 * (-768 + u2 * (320 - 175 * u2)));
                // eq. 4
                double B = u2 / 1024 * (256 + u2 * (-128 + u2 * (74 - 47 * u2)));
                // eq. 6
                deltasigma = B * sinsigma * (cos2sigmam + B / 4 * (cossigma * (-1 + 2 * cos2sigmam2) - B / 6 * cos2sigmam * (-3 + 4 * sin2sigma) * (-3 + 4 * cos2sigmam2)));
                // eq. 10
                double C = f / 16 * cos2alpha * (4 + f * (4 - 3 * cos2alpha));
                // eq. 11 (modified)
                lambda = omega + (1 - C) * f * sinalpha * (sigma + C * sinsigma * (cos2sigmam + C * cossigma * (-1 + 2 * cos2sigmam2)));
                // see how much improvement we got
                double change = Math.Abs((lambda - lambda0) / lambda);
                if (i > 1 && change < 1e-10)
                    break;
            }
            // eq. 19
            double s = b * A * (sigma - deltasigma);
            return Math.Sqrt(Math.Pow(s, 2) + Math.Pow(point1.Altitude - point2.Altitude, 2));
        }
    }
}

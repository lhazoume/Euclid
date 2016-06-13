namespace Euclid.Helpers.Geodesic
{
    /// <summary>
    /// Coordinates class, used to carry GPS coordinates and altitude in meters
    /// </summary>
    public class Coordinates
    {
        private readonly double _latitude, _longitude, _altitude;

        /// <summary>Builds a <c>Coordinates</c></summary>
        /// <param name="latitude">the latitude</param>
        /// <param name="longitude">the longitude</param>
        /// <param name="altitude">the altitude</param>
        public Coordinates(double latitude, double longitude, double altitude)
        {
            _latitude = latitude;
            _longitude = longitude;
            _altitude = altitude;
        }

        #region Accessors
        /// <summary>Gets he geodesic latitude (negative in the southern hemisphere)</summary>
        public double Latitude
        {
            get { return _latitude; }
        }
        /// <summary>Gets the geodesic longitude</summary>
        public double Longitude
        {
            get { return _longitude; }
        }
        /// <summary>Gets the altitude in meters</summary>
        public double Altitude
        {
            get { return _altitude; }
        }
        #endregion
    }
}

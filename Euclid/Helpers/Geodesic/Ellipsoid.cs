namespace Euclid.Helpers.Geodesic
{
    /// <summary>Ellipsoid class, used for distance calculations</summary>
    public class Ellipsoid
    {
        private readonly double _semiMajorAxis, _semiMinorAxis, _flattening;

        private Ellipsoid(double radius, double flattening)
        {
            _semiMajorAxis = radius;
            _flattening = flattening;
            _semiMinorAxis = (1 - flattening) * radius;
        }

        /// <summary>The World Geodetic System 1984</summary>
        static public readonly Ellipsoid WorldGeodeticSystem84 = Create(6378137.0, 298.257223563);

        /// <summary>The Geodesic Reference System 1980</summary>
        static public readonly Ellipsoid GeodeticReferenceSystem80 = Create(6378137.0, 298.257222101);

        /// <summary>The Geodesic Reference System 1967</summary>
        static public readonly Ellipsoid GeodeticReferenceSystem67 = Create(6378160.0, 298.25);

        /// <summary>The Clarke1880 ellipsoid parameters</summary>
        static public readonly Ellipsoid Clarke1880 = Create(6378249.145, 293.465);

        #region Create
        static private Ellipsoid Create(double semiMajor, double inverseFlattening)
        {
            return new Ellipsoid(semiMajor, 1.0 / inverseFlattening);
        }
        #endregion

        /// <summary>Gets the semi-major axis of the ellipsoid</summary>
        public double SemiMajorAxis
        {
            get { return _semiMajorAxis; }
        }

        /// <summary>Gets the semi-minor axis of the ellipsoid</summary>
        public double SemiMinorAxis
        {
            get { return _semiMinorAxis; }
        }

        /// <summary>Gets the flattening of the ellipsoid</summary>
        public double Flattening
        {
            get { return _flattening; }
        }
    }
}

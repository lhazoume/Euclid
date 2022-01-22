namespace Euclid.Numerics
{
    /// <summary>Represents a finite difference scheme</summary>
    public enum FiniteDifferenceScheme
    {
        /// <summary>Implicit scheme</summary>
        Implicit = 0,
        /// <summary>Explicit scheme</summary>
        Explicit = 1,
        /// <summary>Crank Nicholson scheme</summary>
        CrankNicholson = 2
    }
}

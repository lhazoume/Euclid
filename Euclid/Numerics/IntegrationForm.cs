namespace Euclid.Numerics
{
    /// <summary>
    /// The integration method
    /// </summary>
    public enum IntegrationForm
    {
        /// <summary>Left-point rule</summary>
        Left = 0,
        /// <summary>Right-point rule</summary>
        Right = 1,
        /// <summary>Middle-point rule</summary>
        Middle = 2,
        /// <summary>Trapeze rule</summary>
        Trapeze = 3,
        /// <summary>Simpson rule</summary>
        Simpson = 4
    }
}

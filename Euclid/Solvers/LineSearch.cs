namespace Euclid.Solvers
{
    /// <summary>
    /// Line search methods used for the gradient descent
    /// </summary>
    public enum LineSearch
    {
        /// <summary>Naïve line search by dividing the alpha until the functional is lowered</summary>
        Naive = 0,
        /// <summary>Armijo criteria</summary>
        Armijo = 1,
        /// <summary>Armijo Goldstein criteria </summary>
        ArmijoGoldStein = 2,
        /// <summary>Strong Wolfe criteria </summary>
        StrongWolfe = 3
    }
}

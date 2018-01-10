namespace Euclid.Solvers
{
    /// <summary>
    /// Line search methods used for the gradient descent
    /// </summary>
    public enum LineSearch
    {
        /// <summary>Naïve line search by dividing the alpha until the functional is lowered</summary>
        Naive = 0,
        Lowest =1,
        /// <summary>Armijo criteria</summary>
        Armijo = 2,
        /// <summary>Armijo Goldstein criteria </summary>
        ArmijoGoldStein = 3,
        /// <summary>Strong Wolfe criteria </summary>
        StrongWolfe = 4
    }
}

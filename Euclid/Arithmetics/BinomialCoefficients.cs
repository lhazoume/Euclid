namespace Euclid.Arithmetics
{
    /// <summary>Binomial coefficients class</summary>
    public sealed class BinomialCoefficients
    {
        private readonly int[] _coefficients;
        private readonly int _degree;

        /// <summary>Calculates all the binomial coefficients for a given degree</summary>
        /// <param name="n">the degree</param>
        public BinomialCoefficients(int n)
        {
            _degree = n;
            _coefficients = new int[1 + n];
            Calculate();
        }

        private void Calculate()
        {
            for (int i = 0; 2 * i <= _degree; i++)
            {
                _coefficients[i] = i == 0 ? 1 : (_coefficients[i - 1] * (_degree - i + 1)) / i;
                _coefficients[_degree - i] = _coefficients[i];
            }
        }

        /// <summary>Returns the binomial coefficient for a given index</summary>
        /// <param name="index">the index</param>
        /// <returns>an <c>int</c></returns>
        public int this[int index] => _coefficients[index];

        /// <summary>Gets all the binomial coefficients</summary>
        public int[] Coefficients => _coefficients;
    }
}

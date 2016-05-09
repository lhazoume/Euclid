namespace Euclid.Arithmetics
{
    public class PascalTriangle
    {
        private int[] _coefficients;
        private int _degree;

        public PascalTriangle(int n)
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

        public int this[int index]
        {
            get { return _coefficients[index]; }
        }
    }
}

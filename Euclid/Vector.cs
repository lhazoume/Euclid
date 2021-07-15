using Euclid.Distributions.Continuous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid
{
    /// <summary>Vector of double class</summary>
    public class Vector
    {
        #region Declarations
        private readonly int _size;
        private readonly double[] _data;
        #endregion

        #region Constructors
        private Vector(IEnumerable<double> data)
        {
            _data = data.ToArray();
            _size = data.Count();
        }

        private Vector(double[] data)
        {
            _size = data.Length;
            _data = new double[_size];
            Array.Copy(data, _data, _size);
        }

        private Vector(int size)
        {
            _data = new double[size];
            _size = size;
        }

        private Vector(int size, double value)
        {
            _data = new double[size];
            _size = size;
            for (int i = 0; i < _size; i++)
                _data[i] = value;
        }
        #endregion

        #region Accessors
        /// <summary>Returns the Vector's size</summary>
        public int Size => _size;

        /// <summary>Gets the Vector's components</summary>
        public double[] Data => _data;

        /// <summary>Gets a component of the Vector</summary>
        /// <param name="i">the index</param>
        /// <returns>a double</returns>
        public double this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        /// <summary>Returns a deep copy of the Vector</summary>
        public Vector Clone => new Vector(_data);
        #endregion

        #region Norms and sums

        /// <summary>Returns the sum of the absolute values</summary>
        public double Norm1
        {
            get
            {
                double result = 0;
                for (int k = 0; k < _data.Length; k++)
                    result += Math.Abs(_data[k]);
                return result;
            }
        }

        /// <summary>Returns the square root of the sum of squares</summary>
        public double Norm2 => Math.Sqrt(this.SumOfSquares);

        /// <summary>Returns the largest value of the <c>Matrix</c> in absolute value</summary>
        public double NormSup => _data.Max(Math.Abs);

        /// <summary>Returns the sum of the squared values</summary>
        public double SumOfSquares
        {
            get
            {
                double result = 0;
                for (int k = 0; k < _data.Length; k++)
                    result += _data[k] * _data[k];
                return result;
            }
        }

        /// <summary>Returns the sum of the values</summary>
        public double Sum => _data.Sum();

        /// <summary>Builds a Vector as a linear combination of vectors</summary>
        /// <param name="factors">the factors</param>
        /// <param name="vectors">the vectors</param>
        /// <returns>the Vector result of Sum i  fi*mi</returns>
        public static Vector LinearCombination(double[] factors, Vector[] vectors)
        {
            if (factors.Length != vectors.Length) throw new ArgumentException("the vectors do not match the factors");
            if (vectors.Any(m => m is null)) throw new ArgumentNullException(nameof(vectors));
            if (vectors.Any(m => m.Size != vectors[0].Size)) throw new ArgumentException("Vectors must have the same dimensions!");

            Vector r = Vector.Create(vectors[0].Size);
            Parallel.For(0, r.Size, k =>
            {
                for (int i = 0; i < factors.Length; i++)
                    r[k] += factors[i] * vectors[i][k];
            });

            return r;
        }
        #endregion

        #region Operators

        #region Multiplications / divisions

        /// <summary>Multiplies a <c>Vector</c> by a scalar</summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(Vector v, double f)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));
            Vector tmp = new Vector(v._size);
            for (int k = 0; k < v._size; k++)
                tmp._data[k] = f * v._data[k];
            return tmp;
        }

        /// <summary>Multiplies a <c>Vector</c> by a scalar</summary>
        /// <param name="f">the scalar</param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(double f, Vector v)
        {
            return v * f;
        }

        /// <summary>Divides all the coefficients of a <c>Vector</c> by a scalar</summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Vector</c> result of the division</returns>
        public static Vector operator /(Vector v, double f)
        {
            return v * (1 / f);
        }

        /// <summary>Multiplies a Matrix by a Vector</summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(Matrix m, Vector v)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (v == null) throw new ArgumentNullException(nameof(v));
            if (m.Columns != v.Size) throw new Exception("Wrong dimensions of matrix!");

            Vector result = new Vector(m.Rows);
            for (int i = 0; i < result.Size; i++)
                for (int k = 0; k < m.Columns; k++)
                    result._data[i] += m[i, k] * v._data[k];
            return result;
        }

        /// <summary>Multiplies a Vector by a Matrix</summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(Vector v, Matrix m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (v == null) throw new ArgumentNullException(nameof(v));
            if (m.Rows != v.Size) throw new Exception("Wrong dimensions of matrix!");

            Vector result = new Vector(m.Columns);
            for (int i = 0; i < result.Size; i++)
                for (int k = 0; k < m.Rows; k++)
                    result._data[i] += m[k, i] * v._data[k];
            return result;
        }

        /// <summary>Multiplies a Vector by a Vector's transpose</summary>
        /// <param name="v1">the left hand side <c>Vector</c></param>
        /// <param name="v2">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(Vector v1, Vector v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));

            Matrix result = Matrix.Create(v1._size, v2._size);
            for (int i = 0; i < v1._size; i++)
                for (int j = 0; j < v2._size; j++)
                    result[i, j] = v1._data[i] * v2._data[j];
            return result;
        }

        #endregion

        #region Additions / substractions

        /// <summary>Performs a Vector addition, after going through dimension compatibility verifications.</summary>
        /// <param name="v1">First Vector</param>
        /// <param name="v2">Second Vector</param>
        /// <returns>The sum of m1 and m2</returns>
        private static Vector Add(Vector v1, Vector v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));
            if (v1._size != v2._size) throw new ArgumentException("Vectors must have the same dimensions!");
            Vector r = new Vector(v1._size);
            for (int k = 0; k < v1._size; k++)
                r._data[k] = v1._data[k] + v2._data[k];
            return r;
        }

        /// <summary>Performs a Vector substraction, after going through dimension compatibility verifications.</summary>
        /// <param name="v1">First Vector</param>
        /// <param name="v2">Second Vector</param>
        /// <returns>The difference of m1 and m2</returns>
        private static Vector Substract(Vector v1, Vector v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));

            if (v1._size != v2._size) throw new ArgumentException("Vectors must have the same dimensions!");
            Vector r = new Vector(v1._size);
            for (int k = 0; k < v1._size; k++)
                r._data[k] = v1._data[k] - v2._data[k];
            return r;
        }

        /// <summary>Adds a scalar to all the coefficients of a <c>Vector</c></summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Vector</c> result of the addition</returns>
        public static Vector operator +(Vector v, double c)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));
            Vector tmp = new Vector(v._size);
            for (int i = 0; i < v._size; i++)
                tmp._data[i] = v._data[i] + c;
            return tmp;
        }

        /// <summary>Adds a scalar to all the coefficients of a <c>Vector</c></summary>
        /// <param name="c">the scalar</param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the addition</returns>
        public static Vector operator +(double c, Vector v)
        {
            return v + c;
        }

        /// <summary>Substracts a scalar to all the coefficients of a <c>Vector</c></summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Vector</c> result of the substraction</returns>
        public static Vector operator -(Vector v, double c)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));
            Vector tmp = new Vector(v._size);
            for (int i = 0; i < v.Size; i++)
                tmp._data[i] = v._data[i] - c;
            return tmp;
        }

        /// <summary>Adds a scalar to the opposite of a <c>Vector</c></summary>
        /// <param name="c">the scalar</param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the substraction</returns>
        public static Vector operator -(double c, Vector v)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));
            Vector tmp = new Vector(v._size);
            for (int i = 0; i < v.Size; i++)
                tmp._data[i] = c - v._data[i];
            return tmp;
        }

        /// <summary>Returns the opposite of the <c>Vector</c></summary>
        /// <param name="v">the input Vector</param>
        /// <returns>the <c>Vector</c> opposite</returns>
        public static Vector operator -(Vector v)
        {
            return v * -1;
        }

        /// <summary>Performs the Vector addition</summary>
        /// <param name="v1">the left hand side Vector</param>
        /// <param name="v2">the right hand side Vector</param>
        /// <returns>a <c>Vector</c></returns>
        public static Vector operator +(Vector v1, Vector v2)
        {
            return Vector.Add(v1, v2);
        }

        /// <summary>Performs a Vector substraction</summary>
        /// <param name="v1">the left hand side</param>
        /// <param name="v2">the right hand side</param>
        /// <returns>the <c>Vector</c> result of the substraction</returns>
        public static Vector operator -(Vector v1, Vector v2)
        {
            return Vector.Substract(v1, v2);
        }

        #endregion

        /// <summary>Returns the scalar product of the Vectors</summary>
        /// <param name="v1">the left hand side</param>
        /// <param name="v2">the right hand side</param>
        /// <returns>a double value</returns>
        public static double Scalar(Vector v1, Vector v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));
            if (v1.Size != v2.Size) throw new ArgumentException("The scalar product of two matrices can only be performed if they are the same size");

            double result = 0;
            for (int k = 0; k < v1.Size; k++)
                result += v1[k] * v2[k];
            return result;
        }

        /// <summary>Aggregates a list of <c>Vector</c></summary>
        /// <param name="vectors">the vectors to sum</param>
        /// <returns>a <c>Vector</c></returns>
        public static Vector AggregateSum(IList<Vector> vectors)
        {
            if (vectors == null) throw new ArgumentNullException(nameof(vectors));

            if (vectors.Count == 0)
                return Vector.Create(1, 0.0);
            else if (vectors.Count == 1)
                return vectors[0].Clone;
            else
            {
                int size = vectors[0].Size;
                double[] array = new double[size];
                Parallel.For(0, size, s =>
                {
                    for (int i = 0; i < vectors.Count; i++)
                        array[s] += vectors[i][s];
                });
                return new Vector(array);
            }
        }

        /// <summary>Applies a function on the fields on a Vector</summary>
        /// <param name="method"></param>
        /// <returns>a Vector</returns>
        public Vector Apply(Func<double, double> method)
        {
            return Vector.Create(_data.Select(d => method(d)));
        }

        /// <summary>Computes a quadratic form product of two Vectors </summary>
        /// <param name="x">the left hand side Vector</param>
        /// <param name="a">the matrix</param>
        /// <param name="y">the right hand side Vector</param>
        /// <returns>a double</returns>
        public static double Quadratic(Vector x, Matrix a, Vector y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (a.IsSquare && a.Rows == x.Size && a.Columns == y.Size)
            {
                double result = 0;
                for (int i = 0; i < x._size; i++)
                    for (int j = 0; j < y._size; j++)
                        result += x[i] * a[i, j] * y[j];
                return result;
            }
            throw new ArgumentException("the sizes do not fit");
        }

        /// <summary>Returns the Hadamard product</summary>
        /// <param name="v1">the left hand side</param>
        /// <param name="v2">the right hand side</param>
        /// <returns>a <c>Vector</c> containing the Hadamard product</returns>
        public static Vector Hadamard(Vector v1, Vector v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));
            if (v1.Size != v2.Size) throw new ArgumentException("The Hadamard product of two Vectors can only be performed if they are the same size");

            Vector result = new Vector(v1.Size);
            for (int k = 0; k < v1.Size; k++)
                result[k] = v1[k] * v2[k];
            return result;
        }

        /// <summary>Builds a Vector made of the highest values </summary>
        /// <param name="v1">a Vector</param>
        /// <param name="v2">a Vector</param>
        /// <returns>a Vector</returns>
        public static Vector Max(Vector v1, Vector v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));

            if (v1.Size == v2.Size)
            {
                Vector result = new Vector(v1.Size);
                for (int k = 0; k < v1.Size; k++)
                    result[k] = Math.Max(v1[k], v2[k]);
                return result;
            }
            throw new RankException("The vectors have different sizes.");
        }

        /// <summary>Builds a Vector made of the smallest values </summary>
        /// <param name="v1">a Vector</param>
        /// <param name="v2">a Vector</param>
        /// <returns>a Vector</returns>
        public static Vector Min(Vector v1, Vector v2)
        {
            if (v1 == null)
                throw new ArgumentNullException(nameof(v1));
            if (v2 == null)
                throw new ArgumentNullException(nameof(v2));
            if (v1.Size == v2.Size)
            {
                Vector result = new Vector(v1.Size);
                for (int k = 0; k < v1.Size; k++)
                    result[k] = Math.Min(v1[k], v2[k]);
                return result;
            }
            throw new RankException("The vectors have different sizes.");
        }

        /// <summary>Builds a Vector made of the values bounded</summary>
        /// <param name="lowBound">the lower bounds</param>
        /// <param name="upBound">the upper bounds</param>
        /// <param name="x">the vector to bound</param>
        /// <returns>a <c>Vector</c></returns>
        public static Vector Bound(Vector lowBound, Vector upBound, Vector x)
        {
            if (lowBound == null) throw new ArgumentNullException(nameof(lowBound));
            if (upBound == null) throw new ArgumentNullException(nameof(upBound));
            if (x == null) throw new ArgumentNullException(nameof(x));

            if (lowBound.Size == upBound.Size && x.Size == lowBound.Size)
            {
                Vector result = new Vector(lowBound.Size);
                for (int k = 0; k < lowBound.Size; k++)
                    result[k] = Math.Max(lowBound[k], Math.Min(x[k], upBound[k]));
                return result;
            }
            throw new RankException("The vectors have different sizes.");
        }

        /// <summary>Creates a Vector made from the linear combination of two vectors</summary>
        /// <param name="f1">the left hand side factor</param>
        /// <param name="v1">the left hand side vector</param>
        /// <param name="f2">the right hand side factor</param>
        /// <param name="v2">the right hand side vector</param>
        /// <returns>a <c>Vector</c> containing the linear combination of the input</returns>
        public static Vector Create(double f1, Vector v1, double f2, Vector v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));
            if (v1.Size != v2.Size) throw new ArgumentException("Vectors must have the same dimensions!");
            Vector r = new Vector(v1._size);
            for (int k = 0; k < r.Size; k++)
                r[k] = f1 * v1[k] + f2 * v2[k];
            return r;
        }

        /// <summary>Creates a Vector made from the linear combination of three vectors</summary>
        /// <param name="f1">the first factor</param>
        /// <param name="v1">the first vector</param>
        /// <param name="f2">the second factor</param>
        /// <param name="v2">the second vector</param>
        /// <param name="f3">the third factor</param>
        /// <param name="v3">the third vector</param>
        /// <returns>a <c>Vector</c> containing the linear combination of the input</returns>
        public static Vector Create(double f1, Vector v1, double f2, Vector v2, double f3, Vector v3)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));
            if (v3 == null) throw new ArgumentNullException(nameof(v3));

            if (v1.Size != v2.Size || v2.Size != v3.Size) throw new ArgumentException("Vectors must have the same dimensions!");
            Vector r = new Vector(v1._size);
            for (int k = 0; k < r.Size; k++)
                r[k] = f1 * v1[k] + f2 * v2[k] + f3 * v3[k];
            return r;
        }
        #endregion

        #region Create

        /// <summary>Creates a Vector from a set of data</summary>
        /// <param name="data">the data set</param>
        /// <returns>a Vector</returns>
        public static Vector Create(params double[] data)
        {
            return new Vector(data);
        }

        /// <summary>Creates a Vector from a list of data</summary>
        /// <param name="data">the data set</param>
        /// <returns>a Vector</returns>
        public static Vector Create(IEnumerable<double> data)
        {
            return new Vector(data);
        }

        /// <summary>Creates an Vector full of zeros </summary>
        /// <param name="size">the Vector's size</param>
        /// <returns>a Vector</returns>
        public static Vector Create(int size)
        {
            return new Vector(size);
        }

        /// <summary>Creates a Vector </summary>
        /// <param name="size">the Vector's size</param>
        /// <param name="value">the value for all components</param>
        /// <returns>a Vector</returns>
        public static Vector Create(int size, double value)
        {
            return new Vector(size, value);
        }

        /// <summary>Creates a Vector full of random variables  </summary>
        /// <param name="size">the vector's size</param>
        /// <param name="distribution">the distribution</param>
        /// <returns></returns>
        public static Vector CreateRandom(int size, ContinuousDistribution distribution)
        {
            if (distribution == null) throw new ArgumentNullException(nameof(distribution));
            return new Vector(distribution.Sample(size));
        }

        #endregion

        #region Interface Implementations
        /// <summary>Determines whether the specified object is equal to the current object</summary>
        /// <param name="other">the object to compare with the current object</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c></returns>
        public bool Equals(Vector other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null || _size != other._size)
                return false;

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
                return true;

            // If all else fails, perform elementwise comparison.
            for (int i = 0; i < _size; i++)
                if (_data[i] != other._data[i])
                    return false;

            return true;
        }

        /// <summary>Returns a string that represents the vector</summary>
        /// <returns>a string that represents the vector</returns>
        public override string ToString()
        {
            return string.Join(";", _data);
        }

        /// <summary>Returns a string that represents the vector</summary>
        /// <returns>a string that represents the vector</returns>
        public string ToString(string format)
        {
            return string.Join(";", _data.Select(d => d.ToString(format)));
        }
        #endregion
    }
}

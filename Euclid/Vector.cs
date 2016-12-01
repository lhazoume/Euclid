using Euclid.Distributions.Continuous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid
{
    /// <summary>
    /// Vector of double class
    /// </summary>
    public class Vector
    {
        #region Declarations
        private int _size;
        private double[] _data;
        #endregion

        /// <summary>Applies a function to transform the data of the vector</summary>
        /// <param name="v">the vector to transform</param>
        /// <param name="func">the transforming function</param>
        /// <returns>a <c>Vector</c></returns>
        public static Vector Apply(Vector v, Func<double, double> func)
        {
            Vector result = Vector.Create(v._size);
            Parallel.For(0, result.Size, k => { result[k] = func(v._data[k]); });
            return result;
        }

        #region Constructors
        private Vector(IEnumerable<double> data)
        {
            _data = data.ToArray();
            _size = data.Count();
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
        public int Size
        {
            get { return _size; }
        }

        /// <summary>Gets the Vector's components</summary>
        public double[] Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Gets a component of the Vector
        /// </summary>
        /// <param name="i">the index</param>
        /// <returns>a double</returns>
        public double this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        /// <summary>Returns a deep copy of the Vector</summary>
        public Vector Clone
        {
            get { return new Vector(_data); }
        }
        #endregion

        #region Norms and sums

        /// <summary>
        /// Return the sum of the absolute values
        /// </summary>
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

        /// <summary>
        /// Returns the square root of the sum of squares
        /// </summary>
        public double Norm2
        {
            get { return Math.Sqrt(this.SumOfSquares); }
        }

        /// <summary>
        /// Returns the largest value of the <c>Matrix</c> in absolute value
        /// </summary>
        public double NormSup
        {
            get
            {
                double result = 0;
                foreach (double x in _data) result = Math.Max(result, Math.Abs(x));
                return result;
            }
        }

        /// <summary>
        /// Returns the sum of the squared values
        /// </summary>
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

        /// <summary>
        /// Returns the sum of the values
        /// </summary>
        public double Sum
        {
            get
            {
                double result = 0;
                for (int k = 0; k < _data.Length; k++)
                    result += _data[k];
                return result;
            }
        }

        #endregion

        #region Operators

        #region Multiplications / divisions

        /// <summary>
        /// Multiplies a <c>Vector</c> by a scalar
        /// </summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(Vector v, double f)
        {
            Vector tmp = v.Clone;
            for (int k = 0; k < v.Size; k++)
                tmp[k] = f * v[k];
            return tmp;
        }

        /// <summary>
        /// Multiplies a <c>Vector</c> by a scalar
        /// </summary>
        /// <param name="f">the scalar</param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(double f, Vector v)
        {
            return v * f;
        }

        /// <summary>
        /// Divides all the coefficients of a <c>Vector</c> by a scalar
        /// </summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>the <c>Vector</c> result of the division</returns>
        public static Vector operator /(Vector v, double f)
        {
            return v * (1 / f);
        }

        /// <summary>
        /// Multiplies a Matrix by a Vector
        /// </summary>
        /// <param name="m">the left hand side <c>Matrix</c></param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(Matrix m, Vector v)
        {
            if (m.Columns != v.Size) throw new Exception("Wrong dimensions of matrix!");

            Vector result = new Vector(m.Rows);
            for (int i = 0; i < result.Size; i++)
                for (int k = 0; k < m.Columns; k++)
                    result._data[i] += m[i, k] * v._data[k];
            return result;
        }

        /// <summary>
        /// Multiplies a Vector by a Matrix
        /// </summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="m">the right hand side <c>Matrix</c></param>
        /// <returns>the <c>Vector</c> result of the multiplication</returns>
        public static Vector operator *(Vector v, Matrix m)
        {
            if (m.Rows != v.Size) throw new Exception("Wrong dimensions of matrix!");

            Vector result = new Vector(m.Columns);
            for (int i = 0; i < result.Size; i++)
                for (int k = 0; k < m.Rows; k++)
                    result._data[i] += m[k, i] * v._data[k];
            return result;
        }

        /// <summary>
        /// Multiplies a Vector by a Vector's transpose
        /// </summary>
        /// <param name="v1">the left hand side <c>Vector</c></param>
        /// <param name="v2">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Matrix</c> result of the multiplication</returns>
        public static Matrix operator *(Vector v1, Vector v2)
        {
            Matrix result = Matrix.Create(v1.Size, v2.Size);
            for (int i = 0; i < v1.Size; i++)
                for (int j = 0; j < v2.Size; j++)
                    result[i, j] = v1[i] * v2[j];
            return result;
        }

        #endregion

        #region Additions / substractions

        /// <summary>
        /// Performs a Vector addition, after going through dimension compatibility verifications.
        /// </summary>
        /// <param name="v1">First Vector</param>
        /// <param name="v2">Second Vector</param>
        /// <returns>The sum of m1 and m2</returns>
        private static Vector Add(Vector v1, Vector v2)
        {
            if (v1.Size != v2.Size) throw new ArgumentException("Vectors must have the same dimensions!");
            Vector r = new Vector(v1.Size);
            for (int k = 0; k < r.Size; k++)
                r[k] = v1[k] + v2[k];
            return r;
        }

        /// <summary>
        /// Adds a scalar to all the coefficients of a <c>Vector</c>
        /// </summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Vector</c> result of the addition</returns>
        public static Vector operator +(Vector v, double c)
        {
            Vector tmp = v.Clone;
            for (int i = 0; i < v.Size; i++)
                tmp[i] = v[i] + c;
            return tmp;
        }

        /// <summary>
        /// Adds a scalar to all the coefficients of a <c>Vector</c>
        /// </summary>
        /// <param name="c">the scalar</param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the addition</returns>
        public static Vector operator +(double c, Vector v)
        {
            Vector tmp = v.Clone;
            for (int i = 0; i < v.Size; i++)
                tmp[i] = v[i] + c;
            return tmp;
        }

        /// <summary>
        /// Substracts a scalar to all the coefficients of a <c>Vector</c>
        /// </summary>
        /// <param name="v">the left hand side <c>Vector</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>the <c>Vector</c> result of the substraction</returns>
        public static Vector operator -(Vector v, double c)
        {
            Vector tmp = v.Clone;
            for (int i = 0; i < v.Size; i++)
                tmp[i] = v[i] - c;
            return tmp;
        }

        /// <summary>
        /// Adds a scalar to the opposite of a <c>Vector</c>
        /// </summary>
        /// <param name="c">the scalar</param>
        /// <param name="v">the right hand side <c>Vector</c></param>
        /// <returns>the <c>Vector</c> result of the substraction</returns>
        public static Vector operator -(double c, Vector v)
        {
            Vector tmp = v.Clone;
            for (int i = 0; i < v.Size; i++)
                tmp[i] = v[i] - c;
            return tmp;
        }

        /// <summary>
        /// Returns the opposite of the <c>Vector</c>
        /// </summary>
        /// <param name="v">the input Vector</param>
        /// <returns>the <c>Vector</c> opposite</returns>
        public static Vector operator -(Vector v)
        {
            return v * -1;
        }

        /// <summary>
        /// Performs the Vector addition
        /// </summary>
        /// <param name="v1">the left hand side Vector</param>
        /// <param name="v2">the right hand side Vector</param>
        /// <returns>a <c>Vector</c></returns>
        public static Vector operator +(Vector v1, Vector v2)
        {
            return Vector.Add(v1, v2);
        }

        /// <summary>
        /// Performs a Vector substraction
        /// </summary>
        /// <param name="v1">the left hand side</param>
        /// <param name="v2">the right hand side</param>
        /// <returns>the <c>Vector</c> result of the substraction</returns>
        public static Vector operator -(Vector v1, Vector v2)
        {
            return Vector.Add(v1, -v2);
        }

        #endregion

        /// <summary>
        /// Returns the scalar product of the Vectors
        /// </summary>
        /// <param name="v1">the left hand side</param>
        /// <param name="v2">the right hand side</param>
        /// <returns>a double value</returns>
        public static double Scalar(Vector v1, Vector v2)
        {
            if (v1.Size == v2.Size)
            {
                double result = 0;
                for (int k = 0; k < v1.Size; k++)
                    result += v1[k] * v2[k];
                return result;
            }
            throw new ArgumentException("The scalar product of two matrices can only be performed if they are the same size");
        }

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

        /// <summary>
        /// Returns the Hadamard product
        /// </summary>
        /// <param name="v1">the left hand side</param>
        /// <param name="v2">the right hand side</param>
        /// <returns>a <c>Vector</c> containing the Hadamard product</returns>
        public static Vector Hadamard(Vector v1, Vector v2)
        {
            if (v1.Size == v2.Size)
            {
                Vector result = new Vector(v1.Size);
                for (int k = 0; k < v1.Size; k++)
                    result[k] = v1[k] * v2[k];
                return result;
            }
            throw new ArgumentException("The Hadamard product of two Vectors can only be performed if they are the same size");
        }

        /// <summary>
        /// Creates a Vector made from the linear combination of two vectors
        /// </summary>
        /// <param name="f1">the left hand side factor</param>
        /// <param name="v1">the left hand side vector</param>
        /// <param name="f2">the right hand side factor</param>
        /// <param name="v2">the right hand side vector</param>
        /// <returns>a <c>Vector</c> containing the linear combination of the input</returns>
        public static Vector Create(double f1, Vector v1, double f2, Vector v2)
        {
            if (v1.Size != v2.Size) throw new ArgumentException("Vectors must have the same dimensions!");
            Vector r = Vector.Create(v1.Size);
            for (int k = 0; k < r.Size; k++)
                r[k] = f1 * v1[k] + f2 * v2[k];
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
            return new Vector(distribution.Sample(size));
        }

        #endregion

        #region Interface Implementations
        /// <summary>
        /// Determines whether the specified object is equal to the current object
        /// </summary>
        /// <param name="other">the object to compare with the current object</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c></returns>
        public bool Equals(Vector other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null)
                return false;
            if (_size != other._size)
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

        /// <summary>
        /// Returns a string that represents the matrix
        /// </summary>
        /// <returns>a string that represents the matrix</returns>
        public override string ToString()
        {
            return string.Join(";", _data);
        }
        #endregion
    }
}

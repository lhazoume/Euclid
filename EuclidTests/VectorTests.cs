using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Euclid.Tests
{
    [TestClass()]
    public class VectorTests
    {
        private static int _n = 10;
        private static Random _rnd = new Random(Guid.NewGuid().GetHashCode());
        private static double[] _data = Enumerable.Range(0, _n).Select(i => _rnd.NextDouble()).ToArray();

        private static Vector StandardBuilder()
        {
            return Vector.Create(_data);
        }

        #region Create
        [TestMethod()]
        public void CreateParamsTest()
        {
            Vector vector = Vector.Create(1.0, 2.0, 3.0, 4.0);
            Assert.IsTrue(vector.Size == 4 &&
                vector[0] == 1.0 &&
                vector[1] == 2.0 &&
                vector[2] == 3.0 &&
                vector[3] == 4.0);
        }

        [TestMethod()]
        public void CreateArrayTest()
        {
            double[] array = new double[10];
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < array.Length; i++)
                array[i] = rnd.NextDouble();

            Vector vector = Vector.Create(array);
            Assert.IsTrue(vector.Size == array.Length &&
                array.Except(vector.Data).Count() == 0);
        }

        [TestMethod()]
        public void CreateEmptyTest()
        {
            Vector vector = Vector.Create(10);
            Assert.IsTrue(vector.Size == 10 && vector.Data.All(d => d == 0));
        }

        [TestMethod()]
        public void CreateConstantTest()
        {
            Vector vector = Vector.Create(10, 5.4);
            Assert.IsTrue(vector.Size == 10 && vector.Data.All(d => d == 5.4));
        }

        [TestMethod()]
        public void CreateRandomTest()
        {
            Vector vector = Vector.CreateRandom(10, new Distributions.Continuous.NormalDistribution(0, 1));
            Assert.IsTrue(vector.Size == 10);
        }
        #endregion

        #region Accessors
        [TestMethod()]
        public void SizeTest()
        {
            Vector vector = StandardBuilder();
            Assert.IsTrue(vector.Size == _n);
        }

        [TestMethod()]
        public void DataTest()
        {
            Vector vector = StandardBuilder();
            double[] data = vector.Data;
            Assert.IsTrue(vector.Size == _data.Length && data.Except(_data).Count() == 0);
        }

        [TestMethod()]
        public void BracketsAccessorTest()
        {
            Vector vector = StandardBuilder();
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => _data[i] == vector[i]));
        }

        [TestMethod()]
        public void CloneTest()
        {
            Vector vector = StandardBuilder(),
                clone = vector.Clone;
            Assert.IsTrue(vector.Equals(clone));
        }
        #endregion

        #region Norms and sums
        [TestMethod()]
        public void Norm1Test()
        {
            Vector vector = StandardBuilder();
            Assert.IsTrue(vector.Norm1 == _data.Sum(d => Math.Abs(d)));
        }

        [TestMethod()]
        public void Norm2Test()
        {
            Vector vector = StandardBuilder();
            Assert.IsTrue(vector.Norm2 == Math.Sqrt(_data.Sum(d => d * d)));
        }

        [TestMethod()]
        public void NormSupTest()
        {
            Vector vector = StandardBuilder();
            Assert.IsTrue(vector.NormSup == _data.Max(d => Math.Abs(d)));
        }

        [TestMethod()]
        public void SumOfSquaresTest()
        {
            Vector vector = StandardBuilder();
            Assert.IsTrue(vector.SumOfSquares == _data.Sum(d => d * d));
        }

        [TestMethod()]
        public void SumTest()
        {
            Vector vector = StandardBuilder();
            Assert.IsTrue(vector.Sum == _data.Sum());
        }
        #endregion

        #region Operators

        [TestMethod()]
        public void AddTest()
        {
            Vector v1 = StandardBuilder(),
                v2 = Vector.Create(_n, 1.0),
                v3 = v2 + v1;
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v3[i] == _data[i] + 1));
        }

        [TestMethod()]
        public void AddNumRightTest()
        {
            Vector v1 = StandardBuilder(),
                v2 = v1 + 1;
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v2[i] == _data[i] + 1));
        }

        [TestMethod()]
        public void AddNumLeftTest()
        {
            Vector v1 = StandardBuilder(),
                v2 = 1 + v1;
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v2[i] == _data[i] + 1));
        }

        [TestMethod()]
        public void SubstractNumRightTest()
        {
            Vector v1 = StandardBuilder(),
                v2 = v1 - 1;
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v2[i] == _data[i] - 1));
        }

        [TestMethod()]
        public void SubstractNumLeftTest()
        {
            Vector v1 = StandardBuilder(),
                v2 = 1 - v1;
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v2[i] == 1 - _data[i]));
        }

        [TestMethod()]
        public void MultiplyByNumRightTest()
        {
            Vector v = StandardBuilder() * 3;
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v[i] == _data[i] * 3));
        }

        [TestMethod()]
        public void MultiplyByNumLeftTest()
        {
            Vector v = 3 * StandardBuilder();
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v[i] == _data[i] * 3));
        }

        [TestMethod()]
        public void DivideByNumTest()
        {
            Vector v = StandardBuilder() / 4;
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => v[i] == _data[i] / 4));
        }

        [TestMethod()]
        public void ScalarTest()
        {
            Vector v1 = StandardBuilder(),
                v2 = 3 * StandardBuilder();
            Assert.AreEqual(Vector.Scalar(v1, v2), 3.0 * v1.SumOfSquares, 1e-5);
        }

        [TestMethod()]
        public void ApplyTest()
        {
            Func<double, double> transformer = d => Math.Exp(d);
            Vector vector = StandardBuilder().Apply(transformer),
                altVector = Vector.Create(_data.Select(transformer));

            Assert.IsTrue(vector.Equals(altVector));
        }

        [TestMethod()]
        public void HadamardTest()
        {
            Vector v1 = StandardBuilder(),
                v2 = 2 * StandardBuilder(),
                v3 = Vector.Hadamard(v1, v2);

            Assert.IsTrue(v3.Size == v1.Size && Enumerable.Range(0, _n).All(i => v3[i] == 2 * _data[i] * _data[i]));
        }

        [TestMethod()]
        public void MaxTest()
        {
            double[] r1 = Enumerable.Range(0, _n).Select(i => _rnd.NextDouble()).ToArray(),
                r2 = Enumerable.Range(0, _n).Select(i => _rnd.NextDouble()).ToArray(),
                r3 = Enumerable.Range(0, _n).Select(i => Math.Max(r1[i], r2[i])).ToArray();

            Vector v1 = Vector.Create(r1),
                v2 = Vector.Create(r2),
                vMax = Vector.Max(v1, v2),
                v3 = Vector.Create(r3);
            Assert.IsTrue(vMax.Equals(v3));
        }

        [TestMethod()]
        public void MinTest()
        {
            double[] r1 = Enumerable.Range(0, _n).Select(i => _rnd.NextDouble()).ToArray(),
                r2 = Enumerable.Range(0, _n).Select(i => _rnd.NextDouble()).ToArray(),
                r3 = Enumerable.Range(0, _n).Select(i => Math.Min(r1[i], r2[i])).ToArray();

            Vector v1 = Vector.Create(r1),
                v2 = Vector.Create(r2),
                vMin = Vector.Min(v1, v2),
                v3 = Vector.Create(r3);
            Assert.IsTrue(vMin.Equals(v3));
        }

        [TestMethod()]
        public void QuadraticTest()
        {
            Vector v = StandardBuilder();
            Matrix m = Matrix.CreateDiagonalMatrix(v.Data);
            Assert.AreEqual(Vector.Quadratic(v, m, v), v.Data.Sum(d => d * d * d), 1e-5);
        }
        #endregion

        #region Interface Implementations
        [TestMethod()]
        public void EqualsTest()
        {
            Vector vector = StandardBuilder(),
                copy = vector,
                sameVector = StandardBuilder();
            Assert.IsTrue(vector.Equals(copy) && vector.Equals(sameVector));
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Vector v = StandardBuilder();
            Assert.AreEqual(v.ToString(), string.Join(";", _data));
        }

        [TestMethod()]
        public void ToStringFormatTest()
        {
            Vector v = StandardBuilder();
            Assert.AreEqual(v.ToString("0.0"), string.Join(";", _data.Select(d => d.ToString("0.0"))));
        }
        #endregion
    }
}
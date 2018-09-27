using Euclid.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Euclid.Helpers.Tests
{
    [TestClass()]
    public class ArraysTests
    {
        [TestMethod()]
        public void Clone1DArrayTest()
        {
            double[] data = new double[] { 1.2, 3.4, 5.6, 7.8, 9.0 },
                clone = data.Clone<double>();

            double sum = 0;
            for (int i = 0; i < data.Length; i++)
                sum += data[i] - clone[i];
            Assert.IsTrue(sum == 0);
        }

        [TestMethod()]
        public void Clone2DArrayTest()
        {
            double[,] data = new double[,] { { 1.2, 3.4 }, { 5.6, 7.8 } },
                clone = data.Clone<double>();

            double sum = 0;
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                    sum += Math.Abs(data[i, j] - clone[i, j]);
            Assert.IsTrue(sum == 0);
        }

        [TestMethod()]
        public void SubArrayTest()
        {
            int[] data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                subArray = data.SubArray(1, 3);

            double sum = 0;
            for (int i = 0; i < subArray.Length; i++)
                sum += Math.Abs(data[i + 1] - subArray[i]);
            Assert.IsTrue(subArray.Length == 3 && sum == 0);
        }

        [TestMethod()]
        public void ApplyTest()
        {
            double[] data = new double[] { 1.2, 3.4, 5.6, 7.8, 9.0 },
                square = data.Apply(x => x * x);

            double sum = 0;
            for (int i = 0; i < data.Length; i++)
                sum += data[i] * data[i] - square[i];
            Assert.IsTrue(sum == 0);
        }
    }
}
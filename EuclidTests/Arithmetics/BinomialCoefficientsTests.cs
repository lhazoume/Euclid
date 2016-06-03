using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.Arithmetics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Arithmetics.Tests
{
    [TestClass()]
    public class BinomialCoefficientsTests
    {
        [TestMethod()]
        public void BinomialCoefficientsTest()
        {
            BinomialCoefficients bc = new BinomialCoefficients(5);
            int[] coefficients = bc.Coefficients;
            int[] expected = new int[6] { 1, 5, 10, 10, 5, 1 };

            int sum = 0;
            for (int i = 0; i < 5; i++)
                sum += (coefficients[i] - expected[i]);
            Assert.IsTrue(sum == 0);
        }

        [TestMethod()]
        public void BinomialCoefficientsAccessorTest()
        {
            BinomialCoefficients bc = new BinomialCoefficients(5);
            int[] expected = new int[6] { 1, 5, 10, 10, 5, 1 };

            int sum = 0;
            for (int i = 0; i < 5; i++)
                sum += (bc[i] - expected[i]);
            Assert.IsTrue(sum == 0);
        }
    }
}
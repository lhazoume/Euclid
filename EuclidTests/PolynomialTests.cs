using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Euclid.Arithmetics;

namespace Euclid.Tests
{
    [TestClass()]
    public class PolynomialTests
    {
        [TestMethod()]
        public void PolynomialTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PolynomialTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PolynomialTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void NormalizeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void EvaluateTest()
        {
            Polynomial p = new Polynomial(1, 2, 3, 4, 5);
            Assert.AreEqual(129, p.Evaluate(2), 1e-10);
        }

        [TestMethod()]
        public void EvaluateComplexTest()
        {
            Polynomial p = new Polynomial(1, 1, 1, 1, 1);
            double theta = 7 * Math.PI / 8;
            Complex eit = Complex.Exp(theta * Complex.I),
                target = eit * eit * (Math.Sin(2.5 * theta) / Math.Sin(0.5 * theta)),
                value = p.Evaluate(eit);
            Assert.AreEqual(0, (p.Evaluate(eit) - target).Modulus(), 1e-10);
        }

        [TestMethod()]
        public void ComplexRootsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RootsTest()
        {
            Polynomial r1 = new Polynomial(-5, 1),
                r2 = new Polynomial(-6, 1),
                r3 = new Polynomial(-Math.PI, 1);
            Polynomial p = (r1 ^ 2) * (r2 ^ 3) * r3;
            List<Tuple<double, int>> roots = p.Roots();

            Assert.IsTrue(roots.Contains(new Tuple<double, int>(1, 2)) && roots.Contains(new Tuple<double, int>(2, 3)) && roots.Contains(new Tuple<double, int>(Math.PI, 1)));
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ToStringTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PowerTest()
        {
            int n = 5;
            Polynomial r = new Polynomial(1, 1),
                rn = r ^ n;
            BinomialCoefficients bc = new BinomialCoefficients(5);
            Polynomial t = new Polynomial(bc.Coefficients.Select(i => (1.0) * i).ToArray());

            Assert.AreEqual(0, (t - rn).Degree);
        }
    }
}
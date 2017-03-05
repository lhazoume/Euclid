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
            Polynomial p1 = new Polynomial(10);
            Assert.IsTrue(p1.Degree == 10 && p1[10] == 1);
        }

        [TestMethod()]
        public void PolynomialTest1()
        {
            Polynomial p1 = new Polynomial(1, 2, 3);
            Assert.IsTrue(p1[0] == 1 && p1[1] == 2 && p1[2] == 3);
        }

        [TestMethod()]
        public void PolynomialTest2()
        {
            Polynomial p1 = new Polynomial(new double[] { 1, 2, 3 });
            Assert.IsTrue(p1[0] == 1 && p1[1] == 2 && p1[2] == 3);
        }

        [TestMethod()]
        public void PolynomialDerivativeTest()
        {
            Polynomial p = new Polynomial(1, 1, 1),
                d = p.Derivative;
            Assert.IsTrue(d.Equals(new Polynomial(1, 2)));
        }

        [TestMethod()]
        public void NormalizeTest()
        {
            Polynomial p = new Polynomial(1, 2, 3, 4, 5),
                n = p.Clone;
            n.Normalize();
            n = n * 5;
            Assert.IsTrue(p.Equals(n));
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
            Polynomial p = new Polynomial(1, 1, 1, 1, 1);
            List<Tuple<Complex, int>> roots = p.ComplexRoots();
            double sum = roots.Select(t => p.Evaluate(t.Item1).Modulus()).Sum();
            Assert.AreEqual(sum, 0, 0.001);
        }

        [TestMethod()]
        public void RootsTest()
        {
            Polynomial r1 = new Polynomial(-5, 1),
                r2 = new Polynomial(-6, 1),
                r3 = new Polynomial(-Math.PI, 1);
            Polynomial p = (r1 ^ 2) * (r2 ^ 3) * r3;
            List<double> roots = p.Roots().Select(t => t.Item1).ToList();

            Assert.IsTrue(roots.Contains(5) && roots.Contains(6) && roots.Contains(Math.PI));
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Polynomial p = new Polynomial(1, 1, 1);
            Assert.AreEqual("1+x+x^2", p.ToString());
        }

        [TestMethod()]
        public void ToStringTest1()
        {
            Polynomial p = new Polynomial(1, 2, 3);
            Assert.AreEqual("1.00+2.00x+3.00x^2", p.ToString("0.00"));
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

        [TestMethod()]
        public void EqualsTest()
        {
            Polynomial p1 = new Polynomial(1, 2, 3, 4, 5, 6),
                p2 = new Polynomial(1, 2, 3, 4, 5, 6);
            Assert.IsTrue(p1.Equals(p2));
        }
    }
}
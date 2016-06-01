using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Tests
{
    [TestClass()]
    public class ComplexTests
    {
        #region Constructors
        [TestMethod()]
        public void ComplexTest()
        {
            Complex c1 = new Complex(10, 5);
            Assert.IsTrue(c1.Re == 10 && c1.Im == 5);
        }

        [TestMethod()]
        public void ComplexTest1()
        {
            Complex c1 = new Complex();
            Assert.IsTrue(c1.Re == 0 && c1.Im == 0);
        }
        #endregion

        #region Modulus

        [TestMethod()]
        public void ModuleTest()
        {
            Complex c = new Complex(1, 1);
            Assert.IsTrue(c.Modulus() == Math.Sqrt(2));
        }

        [TestMethod()]
        public void SquareModuleTest()
        {
            Complex c = new Complex(1, 1);
            Assert.IsTrue(c.SquareModulus() == 2);
        }

        #endregion

        #region Argument

        [TestMethod()]
        public void ArgumentTestI()
        {
            Complex i = Complex.I;
            Assert.IsTrue(i.Argument() == Math.PI / 2);
        }

        [TestMethod()]
        public void ArgumentTestMinusI()
        {
            Complex minusI = -Complex.I;
            Assert.IsTrue(minusI.Argument() == -Math.PI / 2);
        }

        [TestMethod()]
        public void ArgumentTestOne()
        {
            Complex o = Complex.One;
            Assert.AreEqual(o.Argument(), 0, 1e-5, string.Format("Argument for 1 {0}", o.Argument()));
        }

        [TestMethod()]
        public void ArgumentTestMinusOne()
        {
            Complex o = -Complex.One;
            Assert.AreEqual(o.Argument(), Math.PI, 1e-5, string.Format("Argument for 1 {0}", o.Argument()));
        }

        [TestMethod()]
        public void ArgumentTestZero()
        {
            Complex zero = Complex.Zero;
            Assert.IsTrue(zero.Argument() == 0);
        }

        [TestMethod()]
        public void ArgumentTestQ1()
        {
            Complex q1 = new Complex(1, 1);
            Assert.IsTrue(q1.Argument() == Math.PI / 4);
        }

        [TestMethod()]
        public void ArgumentTestQ2()
        {
            Complex q2 = new Complex(-1, 1);
            Assert.IsTrue(q2.Argument() == 3 * Math.PI / 4);
        }

        [TestMethod()]
        public void ArgumentTestQ3()
        {
            Complex q3 = new Complex(-1, -1);
            Assert.IsTrue(q3.Argument() == -3 * Math.PI / 4);
        }

        [TestMethod()]
        public void ArgumentTestQ4()
        {
            Complex q4 = new Complex(1, -1);
            Assert.IsTrue(q4.Argument() == -Math.PI / 4);
        }

        #endregion

        #region ToString

        [TestMethod()]
        public void ToStringTest()
        {
            Complex c = new Complex(1, 1);
            Assert.AreEqual("1+i", c.ToString());
        }

        [TestMethod()]
        public void ToStringTest1()
        {
            Complex c = new Complex(1, 1);
            Assert.AreEqual("1.0+i", c.ToString("0.0"));
        }

        #endregion

        [TestMethod()]
        public void ExpTest()
        {
            Complex e = Complex.Exp(Complex.I * (Math.PI / 4));
            Assert.AreEqual((new Complex(Math.Sqrt(2) / 2, Math.Sqrt(2) / 2) - e).Modulus(), 0, 1e-9);
        }
    }
}
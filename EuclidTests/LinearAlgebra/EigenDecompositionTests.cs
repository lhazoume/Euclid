using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.LinearAlgebra.Tests
{
    [TestClass()]
    public class EigenDecompositionTests
    {
        [TestMethod()]
        public void EigenDecompositionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SolveTest()
        {
            int n = 10;
            Matrix rand = Matrix.RandomMatrix(n).SymmetricPart;
            EigenDecomposition decomp = new EigenDecomposition(rand);
            Complex[] eig = decomp.EigenValues;
            Matrix eigenVectors = decomp.EigenVectors;
            double cumulatedNorm = 0;
            for (int i = 0; i < eig.Length; i++)
            {
                Matrix diff = (rand * eigenVectors.Column(i)) - (eig[i].Re * eigenVectors.Column(i));
                cumulatedNorm += diff.Norm2;
            }
            Assert.AreEqual(cumulatedNorm/(n* n), 0 , 1e-9,"The EigenDecomposition does not behave as expected");
        }
    }
}
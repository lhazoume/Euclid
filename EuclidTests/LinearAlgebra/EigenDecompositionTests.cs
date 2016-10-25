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
        public void DiagonalizeSymmetricMatrixTest()
        {
            int n = 10;
            Matrix rand = Matrix.CreateSquareRandom(n).SymmetricPart;
            EigenDecomposition decomp = new EigenDecomposition(rand);
            Complex[] eig = decomp.EigenValues;
            Vector[] eigenVectors = decomp.EigenVectors;
            double cumulatedNorm = 0;
            for (int i = 0; i < eig.Length; i++)
            {
                Vector diff = (rand * eigenVectors[i]) - (eig[i].Re * eigenVectors[i]);
                cumulatedNorm += diff.Norm2;
            }
            Assert.AreEqual(cumulatedNorm/(n* n), 0 , 1e-9,"The EigenDecomposition does not behave as expected");
        }

        [TestMethod()]
        public void DiagonalizeNonSymmetricMatrixTest()
        {
            int n = 10;
            Matrix rand = Matrix.CreateSquareRandom(n);
            EigenDecomposition decomp = new EigenDecomposition(rand);
            double[] eig = decomp.RealEigenValues;
            Vector[] eigenVectors = decomp.RealEigenVectors;

            double cumulatedNorm = 0;
            for (int i = 0; i < eig.Length; i++)
            {
                Vector diff = (rand * eigenVectors[i]) - (eig[i] * eigenVectors[i]);
                cumulatedNorm += diff.Norm2;
            }
            Assert.AreEqual(cumulatedNorm / (n * n), 0, 1e-9, "The EigenDecomposition does not behave as expected");
        }
    }
}
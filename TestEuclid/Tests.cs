using Euclid;
using Euclid.Arithmetics;
using Euclid.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEuclid
{
    public static class Tests
    {
        public delegate bool TestMethod();

        public static bool AlwaysPassed()
        {
            return true;
        }
        public static bool AlwaysFailed()
        {
            return false;
        }

        public static bool MatrixDecomposition()
        {
            int n = 100;
            Matrix rand = Matrix.RandomMatrix(n).SymmetricPart;
            EigenDecomposition decomp = new EigenDecomposition(rand);
            Complex[] eig = decomp.EigenValues;
            Matrix eigenVectors = decomp.EigenVectors;
            double cumulatedNorm = 0;
            for (int i = 0; i < eig.Length; i++)
            {
                Matrix diff = (rand * eigenVectors.Column(i) - eig[i].Re * eigenVectors.Column(i));
                cumulatedNorm += diff.NormSup;
            }
            return cumulatedNorm <= n * n * 1e-12;
        }
    }
}

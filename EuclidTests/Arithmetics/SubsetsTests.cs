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
    public class SubsetsTests
    {
        [TestMethod()]
        public void AllSubsetsTest()
        {
            int totalSize = 8;
            List<int> data = Enumerable.Range(0, totalSize).ToList();
            List<List<int>> subsets = Subsets.AllSubsets<int>(data);

            Assert.AreEqual(subsets.Count, Math.Pow( 2, totalSize));
        }

        [TestMethod()]
        public void SubsetsOfSizeTest()
        {
            int totalSize = 10,
                tested1 = 1,
                tested2 = 5;
            List<int> data = Enumerable.Range(0, totalSize).ToList();
            List<List<int>> subsetsTest1 = Subsets.SubsetsOfSize<int>(data, tested1),
                subsetsTest2 = Subsets.SubsetsOfSize<int>(data, tested2);

            BinomialCoefficients bc = new BinomialCoefficients(totalSize);

            Assert.AreEqual(Math.Abs(bc[tested1] - subsetsTest1.Count) + Math.Abs(bc[tested2] - subsetsTest2.Count), 0);
        }

        [TestMethod()]
        public void SubsetsOfSizeTest2()
        {
            int totalSize = 10,
                size = 5;
            List<int> data = Enumerable.Range(0, totalSize).ToList();
            List<List<int>> subset = Subsets.SubsetsOfSize<int>(data, size);
            BinomialCoefficients bc = new BinomialCoefficients(totalSize);

            Assert.IsTrue(subset.All(l => l.Count == size));
        }
    }
}
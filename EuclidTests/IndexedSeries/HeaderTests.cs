using Euclid.DataStructures.IndexedSeries;
using Euclid.IndexedSeries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Euclid.IndexedSeries.Tests
{
    [TestClass()]
    public class HeaderTests
    {
        private Header<string> BuildHeader()
        {
            string[] name = new string[] { "C0", "C1", "C2", "C3", "C4", "C5" };
            Header<string> header = new Header<string>(name);
            return header;
        }

        [TestMethod()]
        public void HeaderTest()
        {
            Header<string> result = BuildHeader();
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void AccessorTest()
        {
            Header<string> result = BuildHeader();
            Assert.IsTrue(result["C0"] == 0 && result["C5"] == 5);
        }

        [TestMethod()]
        public void CountTest()
        {
            Header<string> result = BuildHeader();
            Assert.IsTrue(result.Count == 6);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            Header<string> result = BuildHeader();
            result.Remove("C0");
            Assert.IsTrue(result.Count == 5 && result["C1"] == 0);

        }

        [TestMethod()]
        public void AddTest()
        {
            Header<string> result = BuildHeader();
            result.Add("C6");
            Assert.IsTrue(result.Count == 7 && result["C6"] == 6);
        }

        /*[TestMethod()]
        public void GetEnumeratorTest()
        {
            Header<string> result = BuildHeader();
            IEnumerator<string> enumerator = result.GetEnumerator();
            enumerator.MoveNext();
            string first = enumerator.Current;
            enumerator.MoveNext();
            string second = enumerator.Current;
            Assert.AreEqual(first + second, "C0C1");
        }*/

        [TestMethod()]
        public void CloneTest()
        {
            Header<string> result = BuildHeader(),
                clone = result.Clone();
            Assert.AreEqual(result["C0"] + "_" + result["C1"], clone["C0"] + "_" + clone["C1"]);
        }

        [TestMethod()]
        public void RenameTest()
        {
            Header<string> result = BuildHeader();
            result.Rename("C0", "D0");
            Assert.AreEqual(result.ElementAt(0), "D0");
        }

        [TestMethod()]
        public void ElementAtTest()
        {
            Header<string> result = BuildHeader();
            Assert.AreEqual(result.ElementAt(3), "C3");
        }

        [TestMethod()]
        public void ContainsTest()
        {
            Header<string> result = BuildHeader();
            Assert.IsTrue(result.Contains("C0") && !result.Contains("C11"));
        }
    }
}
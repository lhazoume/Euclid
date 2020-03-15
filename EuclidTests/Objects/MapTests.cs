using Microsoft.VisualStudio.TestTools.UnitTesting;
using Euclid.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Euclid.DataStructures;

namespace Euclid.Objects.Tests
{
    [TestClass()]
    public class MapTests
    {
        private static readonly int _n = 100;
        private static readonly Tuple<string, int>[] _values = Enumerable.Range(0, _n).Select(i => Tuple.Create(i.ToString(), i)).ToArray();

        private static Map<string, int> StandardBuilder()
        {
            return new Map<string, int>(_values);
        }

        #region Constructor
        [TestMethod()]
        public void MapTest()
        {
            Map<string, int> map = StandardBuilder();
            Assert.IsTrue(map != null &&
               Enumerable.Range(0, _n).All(i => map.Backward(i) == i.ToString()) &&
               Enumerable.Range(0, _n).All(i => map.Forward(i.ToString()) == i));
        }
        #endregion

        #region Enumerators
        [TestMethod()]
        public void ForwardEnumeratorTest()
        {
            Map<string, int> map = StandardBuilder();
            IEnumerator<string> enumerator = map.ForwardEnumerator;
            int i = 0;
            while (enumerator.MoveNext())
                i++;
            Assert.AreEqual(i, _n);
        }

        [TestMethod()]
        public void BackwardEnumeratorTest()
        {
            Map<string, int> map = StandardBuilder();
            IEnumerator<int> enumerator = map.BackwardEnumerator;
            int i = 0;
            while (enumerator.MoveNext())
                i++;
            Assert.AreEqual(i, _n);
        }
        #endregion

        #region Add / remove
        [TestMethod()]
        public void AddTest()
        {
            Map<string, int> map = StandardBuilder();
            map.Add("Joe", -1);
            Assert.IsTrue(map.ContainsForwardKey("Joe") && map.ContainsBackwardKey(-1));
        }

        [TestMethod()]
        public void RemoveTest()
        {
            Map<string, int> map = StandardBuilder();
            map.Remove("5", 5);
            Assert.IsTrue(!map.ContainsForwardKey("5") && !map.ContainsBackwardKey(5));
        }

        [TestMethod()]
        public void SetForwardTest()
        {
            Map<string, int> map = StandardBuilder();
            map.SetForward("5", -1);
            Assert.IsTrue(map.ContainsForwardKey("5") &&
                map.Forward("5") == -1 &&
                map.Backward(-1) == "5");
        }

        [TestMethod()]
        public void SetBackwardTest()
        {
            Map<string, int> map = StandardBuilder();
            map.SetBackward(5, "Joe");
            Assert.IsTrue(!map.ContainsForwardKey("5") &&
                map.Forward("Joe") == 5 &&
                map.Backward(5) == "Joe");
        }
        #endregion

        #region Contains
        [TestMethod()]
        public void ContainsForwardKeyTest()
        {
            Map<string, int> map = StandardBuilder();
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => map.ContainsForwardKey(i.ToString())));
        }

        [TestMethod()]
        public void ContainsBackwardTest()
        {
            Map<string, int> map = StandardBuilder();
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => map.ContainsBackwardKey(i)));
        }
        #endregion

        #region Acccess
        [TestMethod()]
        public void ForwardTest()
        {
            Map<string, int> map = new Map<string, int>();
            map.Add("Joe", 1982);
            map.Add("Joey", 1983);
            Assert.IsTrue(map.Forward("Joe") == 1982);
        }

        [TestMethod()]
        public void BackwardTest()
        {
            Map<string, int> map = new Map<string, int>();
            map.Add("Joe", 1982);
            map.Add("Joey", 1983);
            Assert.IsTrue(map.Backward(1983) == "Joey");
        }

        [TestMethod()]
        public void LeftsTest()
        {
            Map<string, int> map = StandardBuilder();
            Assert.IsTrue(map.Lefts.Length == _n &&
                map.Lefts.Except(Enumerable.Range(0, _n).Select(i => i.ToString())).Count() == 0);
        }

        [TestMethod()]
        public void RightsTest()
        {
            Map<string, int> map = StandardBuilder();
            Assert.IsTrue(map.Rights.Length == _n &&
                map.Rights.Except(Enumerable.Range(0, _n)).Count() == 0);
        }

        [TestMethod()]
        public void CountTest()
        {
            Map<string, int> map = StandardBuilder();
            Assert.AreEqual(map.Count, _n);
        }
        #endregion


        [TestMethod()]
        public void CloneTest()
        {
            Map<string, int> map = StandardBuilder(),
                clone = map.Clone();
            Assert.IsTrue(Enumerable.Range(0, _n).All(i => map.Backward(i) == clone.Backward(i)) &&
                Enumerable.Range(0, _n).All(i => map.Forward(i.ToString()) == clone.Forward(i.ToString())));
        }
    }
}
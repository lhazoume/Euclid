﻿using Euclid.Analytics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euclid.IndexedSeries.Analytics.Tests
{
    [TestClass()]
    public class LinearModelTests
    {
        [TestMethod()]
        public void LinearModelTest()
        {
            LinearModel lm = LinearModel.Create();
            Assert.IsTrue(!lm.Succeeded);
        }

        [TestMethod()]
        public void LinearModelTest1()
        {
            LinearModel lm = LinearModel.Create(1.0, 10, 1);
            Assert.IsTrue(lm.Succeeded && lm.Constant == 1.0 && lm.R2 == 0);
        }

        [TestMethod()]
        public void LinearModelTest2()
        {
            LinearModel lm = LinearModel.Create(1.0, new double[] { 1.0, 2.0, 3.5, 4.5 }, new double[] { 0.8, 0.65, 0.55, 0.95 }, 100, 1.0, 1.0);
            Assert.IsTrue(lm.Succeeded &&
                lm.Constant == 1.0 &&
                lm.Correlations[0] == 0.8 &&
                lm.R2 == 0.5);
        }

        [TestMethod()]
        public void PredictTest()
        {
            LinearModel lm = LinearModel.Create(5, new double[] { 1, 2, 3, 4, 5, 6 },
                new double[] { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 }, 100, 1, 1);
            double result = lm.Predict(new double[] { 1, 2, 3, 4, 5, 6 });
            Assert.AreEqual(96, result, 1e-10);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Euclid.Interpolations.Interpolator1D;

namespace Euclid.Tests
{
    [TestClass]
    public class TransformedInterpolatorTests
    {
        private const double Tolerance = 1e-9;

        #region Identity Transform
        [TestMethod]
        public void IdentityTransform_MatchesCore()
        {
            List<double> x = new List<double> { 0.0, 1.0, 2.0, 3.0 };
            List<double> y = new List<double> { 0.0, 2.0, 4.0, 6.0 };

            Hyman interpalator = new Hyman(false);
            interpalator.SetData(x, y);
            //Identite 
            TransformedInterpolator trans = new TransformedInterpolator(interpolator: new Hyman(false),forward: (t, v) => v,backward: (t, v) => v);

            trans.SetData(x, y);

            for (int i = 0; i <= 100; i++)
            {
                double xi = 3.0 * i / 100.0;
                double yin = interpalator.ValueAt(xi);
                double yt = trans.ValueAt(xi);
                Assert.AreEqual(yin, yt, Tolerance, $"x={xi:F3}: interpolator={yin:F9} vs trans={yt:F9}");
            }
        }
        #endregion

        #region Square‐via‐Transform
        [TestMethod]
        public void SquareFunction_InterpolatesViaSqrtSquareTransform()
        {
            List<double> x = new List<double> { 0.0, 1.0, 2.0, 3.0, 4.0 };
            List<double> y = x.Select(xx => xx * xx).ToList();

            LinearInterpolation interpolator = new LinearInterpolation(false);
            TransformedInterpolator transformed = new TransformedInterpolator(interpolator: interpolator,forward: (t, yy) => Math.Sqrt(yy),backward: (t, zz) => zz * zz);

            transformed.SetData(x, y);

            for (int i = 0; i <= 500; i++)
            {
                double xi = 4.0 * i / 500.0;
                double yf = transformed.ValueAt(xi);
                double ytrue = xi * xi;
                Assert.AreEqual(ytrue, yf,Tolerance, $"x={xi:F3}: attendu={ytrue:F6}, obtenu={yf:F6}");
            }
        }
        #endregion

        #region Performance Benchmark
        [TestMethod]
        public void PerformanceBenchmark_ValueAt()
        {
            const int N = 100000;
            List<double> x = new List<double> { 0.0, 1.0, 2.0, 3.0, 4.0 };
            List<double> y = x.Select(xx => Math.Exp(-0.03 * xx)).ToList();

            TransformedInterpolator transformed = new TransformedInterpolator(interpolator: new Hyman(false), forward: (t, df) => -Math.Log(df), backward: (t, zt) => Math.Exp(-zt));
            transformed.SetData(x, y);
            transformed.ValueAt(2.3);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < N; i++)
            {
                double t = 4.0 * (i % 5000) / 5000.0; // [0,4] cycle
                transformed.ValueAt(t);
            }
            sw.Stop();

            Assert.IsTrue(sw.ElapsedMilliseconds < 20, $"Trop lent : {sw.ElapsedMilliseconds} ms pour {N} appels");
        }

        #endregion

        #region Error
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetData_DuplicateX()
        {
            List<double> x = new List<double> { 0.0, 1.0, 1.0, 2.0 };
            List<double> y = new List<double> { 1.0, 2.0, 3.0, 4.0 };

            TransformedInterpolator transformed = new TransformedInterpolator(interpolator: new Hyman(false),forward: (t, v) => v,backward: (t, v) => v);
            transformed.SetData(x, y);
        }
        #endregion
    }
}

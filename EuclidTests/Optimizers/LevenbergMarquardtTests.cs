using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Euclid.Optimizers;
using Euclid.Solvers;

namespace Euclid.Tests
{
    [TestClass]
    public class LevenbergMarquardtTests
    {
        #region Tolerances
        private const double TolExact = 1e-12;
        private const double TolLoose = 1e-8;
        private const double TolFiniteDiff = 1e-4;
        private const double TolSolution = 1e-6;
        #endregion

        #region Quadratic Problem (Analytic Jacobian)
        private static Func<Vector, Vector> QuadraticResiduals()
        {
            return parameters =>
            {
                Vector r = Vector.Create(2);
                r[0] = parameters[0] - 2.0;
                r[1] = parameters[1] + 3.0;
                return r;
            };
        }

        private static Func<Vector, Matrix> QuadraticJacobian()
        {
            return _ =>
            {
                Matrix J = Matrix.Create(2, 2);
                J[0, 0] = 1.0; J[0, 1] = 0.0;
                J[1, 0] = 0.0; J[1, 1] = 1.0;
                return J;
            };
        }

        [TestMethod]
        public void Optimize_QuadraticProblem_ExactSolution()
        {
            Vector guess = Vector.Create(2.0, 2.0);
            LevenbergMarquardt lm = new LevenbergMarquardt(
                initialGuess: guess,
                residuals: QuadraticResiduals(),
                jacobian: QuadraticJacobian(),
                maxIter: 50,
                gradientThreshold: TolExact,
                functionThreshold: TolExact
            );

            lm.Optimize();

            Assert.IsTrue(lm.Status == SolverStatus.FunctionConvergence || lm.Status == SolverStatus.GradientConvergence, $"Statut inattendu : {lm.Status}");

            Vector sol = lm.Result;
            Assert.AreEqual(2.0, sol[0], TolSolution, "x0");
            Assert.AreEqual(-3.0, sol[1], TolSolution, "x1");
            Assert.AreEqual(0.0, lm.Error, TolExact, "Erreur finale");
        }
        #endregion

        #region Finite‐Difference vs Analytic
        [TestMethod]
        public void Optimize_WithoutAnalyticJacobian_ConvergesSameAsAnalytic()
        {
            Vector guessA = Vector.Create(5.0, -5.0);
            LevenbergMarquardt lmNum = new LevenbergMarquardt(
                initialGuess: guessA,
                residuals: QuadraticResiduals(),
                jacobian: null,
                bump: Vector.Create(2, 1e-6),
                maxIter: 100,
                gradientThreshold: TolExact,
                functionThreshold: TolExact
            );
            lmNum.Optimize();

            Vector guessB = Vector.Create(5.0, -5.0);
            LevenbergMarquardt lmAna = new LevenbergMarquardt(
                initialGuess: guessB,
                residuals: QuadraticResiduals(),
                jacobian: QuadraticJacobian(),
                maxIter: 100,
                gradientThreshold: TolExact,
                functionThreshold: TolExact
            );
            lmAna.Optimize();

            Assert.IsTrue(lmNum.Status == SolverStatus.FunctionConvergence || lmNum.Status == SolverStatus.GradientConvergence, $"Num statut inattendu : {lmNum.Status}");
            Assert.IsTrue(lmAna.Status == SolverStatus.FunctionConvergence || lmAna.Status == SolverStatus.GradientConvergence, $"Ana statut inattendu : {lmAna.Status}");

            Vector solNum = lmNum.Result;
            Vector solAna = lmAna.Result;
            Assert.AreEqual(solAna[0], solNum[0], TolFiniteDiff, "x0 FD vs ANA");
            Assert.AreEqual(solAna[1], solNum[1], TolFiniteDiff, "x1 FD vs ANA");
        }
        #endregion

        #region Adaptive vs Classic
        [TestMethod]
        public void OptimizeAdaptive_MatchesOptimize()
        {
            Vector guess = Vector.Create(2.5, -1.0);
            LevenbergMarquardt lmClassic = new LevenbergMarquardt(
                initialGuess: guess,
                residuals: QuadraticResiduals(),
                jacobian: QuadraticJacobian(),
                maxIter: 50,
                gradientThreshold: TolExact,
                functionThreshold: TolExact
            );
            lmClassic.Optimize();

            Vector guess2 = Vector.Create(2.5, -1.0);
            LevenbergMarquardt lmAdapt = new LevenbergMarquardt(
                initialGuess: guess2,
                residuals: QuadraticResiduals(),
                jacobian: QuadraticJacobian(),
                maxIter: 50,
                gradientThreshold: TolExact,
                functionThreshold: TolExact
            );
            lmAdapt.OptimizeAdaptive();

            Assert.IsTrue(lmAdapt.Status == SolverStatus.FunctionConvergence || lmAdapt.Status == SolverStatus.GradientConvergence, $"Adaptive statut inattendu : {lmAdapt.Status}");

            Vector solF = lmClassic.Result;
            Vector solA = lmAdapt.Result;
            Assert.AreEqual(solF[0], solA[0], TolLoose, "x0 classic vs adapt");
            Assert.AreEqual(solF[1], solA[1], TolLoose, "x1 classic vs adapt");
        }
        #endregion

        #region Convergence History
        [TestMethod]
        public void Errors_MonotonicNonIncreasing()
        {
            Vector guess = Vector.Create(5.0, 5.0);
            LevenbergMarquardt lm = new LevenbergMarquardt(
                initialGuess: guess,
                residuals: QuadraticResiduals(),
                maxIter: 20
            );
            lm.Optimize();

            IList<double> errs = lm.Errors.ToList();
            for (int i = 1; i < errs.Count; i++)
            {
                Assert.IsTrue(errs[i] <= errs[i - 1] + TolExact, $"Erreur non monotone i={i}: {errs[i - 1]} → {errs[i]}");
            }
        }
        #endregion

        #region Beale's Function Problem 

        private static Func<Vector, Vector> BealeResiduals()
        {
            return p =>
            {
                double x = p[0];
                double y = p[1];
                Vector r = Vector.Create(3);
                r[0] = 1.5 - x + x * y;
                r[1] = 2.25 - x + x * y * y;
                r[2] = 2.625 - x + x * y * y * y;
                return r;
            };
        }

        [TestMethod]
        public void Optimize_BealeFunction_FindsKnownMinimum()
        {
            Vector guess = Vector.Create(1.0, 1.0);
            LevenbergMarquardt lm = new LevenbergMarquardt(
                initialGuess: guess,
                residuals: BealeResiduals(),
                maxIter: 100,
                gradientThreshold: TolExact,
                functionThreshold: TolExact
            );

            lm.Optimize();

            Assert.IsTrue(lm.Status == SolverStatus.FunctionConvergence || lm.Status == SolverStatus.GradientConvergence, $"Statut de convergence inattendu : {lm.Status}");

            Vector expectedSolution = Vector.Create(3.0, 0.5);
            Assert.AreEqual(expectedSolution[0], lm.Result[0], TolSolution, "x0 (Beale)");
            Assert.AreEqual(expectedSolution[1], lm.Result[1], TolSolution, "x1 (Beale)");
            Assert.AreEqual(0.0, lm.Error, TolExact, "Erreur finale (Beale)");
        }
        #endregion

        #region Rosenbrock Function Problem

        private static Func<Vector, Vector> RosenbrockResiduals()
        {
            return p =>
            {
                double x = p[0];
                double y = p[1];
                Vector r = Vector.Create(2);
                r[0] = 1.0 - x;
                r[1] = 10.0 * (y - x * x);
                return r;
            };
        }

        [TestMethod]
        public void OptimizeAdaptive_RosenbrockFunction_FindsGlobalMinimum()
        {

            Vector guess = Vector.Create(0.0, 0.0);
            LevenbergMarquardt lm = new LevenbergMarquardt(
                initialGuess: guess,
                residuals: RosenbrockResiduals(),
                jacobian: null,
                maxIter: 100,
                gradientThreshold: TolLoose,
                functionThreshold: TolLoose
            );

            lm.OptimizeAdaptive();

            Assert.IsTrue(lm.Status == SolverStatus.FunctionConvergence || lm.Status == SolverStatus.GradientConvergence, $"Statut de convergence inattendu : {lm.Status}");

            Vector expectedSolution = Vector.Create(1.0, 1.0);
            Assert.AreEqual(expectedSolution[0], lm.Result[0], TolSolution, "x0 (Rosenbrock)");
            Assert.AreEqual(expectedSolution[1], lm.Result[1], TolSolution, "x1 (Rosenbrock)");
            Assert.AreEqual(0.0, lm.Error, TolExact, "Erreur finale (Rosenbrock)");
        }
        #endregion

        #region Performance Benchmark

        [TestMethod]
        public void PerformanceBenchmark()
        {
            Func<Vector, Vector> zeroRes = v => Vector.Create(v.Size, 0.0);
            Vector guess = Vector.Create(10, 1.0);
            LevenbergMarquardt lm = new LevenbergMarquardt(
                initialGuess: guess,
                residuals: zeroRes,
                maxIter: 1000
            );

            Stopwatch sw = Stopwatch.StartNew();
            lm.Optimize();
            sw.Stop();

            Assert.IsTrue(sw.ElapsedMilliseconds < 2, $"Trop lent : {sw.ElapsedMilliseconds} ms pour 1000 iters");
        }
        #endregion
    }
}
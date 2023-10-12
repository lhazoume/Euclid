﻿using Euclid.DataStructures.IndexedSeries;
using Euclid.Extensions;
using Euclid.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Regressions
{
    /// <summary>
    /// Total Least Squares Regression class
    /// </summary>
    public class TotalLeastSquaresRegression
    {
        #region vars
        /// <summary>
        /// used Constant into error measurement
        /// </summary>
        public bool WithConstant { get; private set; }

        /// <summary>
        /// Enable error computation
        /// </summary>
        public bool ComputeErr { get; private set; }

        /// <summary>
        /// Regression status
        /// </summary>
        public RegressionStatus Status { get; private set; }

        /// <summary>
        /// Data to regress
        /// </summary>
        public Matrix X { get; private set; }

        /// <summary>
        /// Coefficient of the regression, bo is intercept
        /// </summary>
        public Vector Beta { get; private set; }

        /// <summary>
        /// Singular Value Decomposition method
        /// </summary>
        public SVDType SVDMethod { get; private set; }

        /// <summary>
        /// Error tolerance for SVD
        /// </summary>
        public double Epsilon { get; private set; }

        /// <summary>
        /// Linear attributes of the regression
        /// </summary>
        public LinearModel Linear { get; private set; }
        /// <summary>
        /// Apply/Applied centering over data matrix
        /// </summary>
        public bool Centering { get; private set; }
        /// <summary>
        /// Apply/Applied scaling over data matrix
        /// </summary>
        public bool Scaling { get; private set; }
        #endregion

        #region constructor
        /// <summary>
        /// TLS constructor
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        /// <param name="svdType">Singular Value Decomposition type</param>
        /// <param name="epsilon">Tolerance for SVD</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        private TotalLeastSquaresRegression(Matrix x, bool withConstant, bool computeErr, SVDType svdType, double epsilon, bool centering, bool scaling)
        {
            Epsilon = epsilon;
            SVDMethod = svdType;
            WithConstant = withConstant;
            ComputeErr = computeErr;
            Status = RegressionStatus.NotRan;
            X = x;
            Centering = centering;
            Scaling = scaling;
        }
        #endregion

        #region methods

        #region create
        /// <summary>
        /// Create a TLS from a matrix
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        /// <param name="svdType">Singular Value Decomposition type</param>
        /// <param name="epsilon">Tolerance for SVD</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <returns>TLS objects</returns>
        public static TotalLeastSquaresRegression Create(Matrix x, bool withConstant = true, bool computeErr = true, SVDType svdType = SVDType.POWER_ITERATION, double epsilon = 1e-10, bool centering = true, bool scaling = true) 
        { return new TotalLeastSquaresRegression(x, withConstant, computeErr, svdType, epsilon, centering, scaling); }

        /// <summary>
        /// Create a TLS from a jagged array
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        /// <param name="svdType">Singular Value Decomposition type</param>
        /// <param name="epsilon">Tolerance for SVD</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <returns>TLS objects</returns>
        public static TotalLeastSquaresRegression Create(double[][] x, bool withConstant = true, bool computeErr = true, SVDType svdType = SVDType.POWER_ITERATION, double epsilon = 1e-10, bool centering = true, bool scaling = true) 
        { return new TotalLeastSquaresRegression(Matrix.Create(x), withConstant, computeErr, svdType, epsilon, centering, scaling); }

        /// <summary>
        /// Create a TLS from a dataframe
        /// </summary>
        /// <typeparam name="T">Legend</typeparam>
        /// <typeparam name="TV">Label</typeparam>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        /// <param name="svdType">Singular Value Decomposition type</param>
        /// <param name="epsilon">Tolerance for SVD</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <returns>TLS objects</returns>
        public static TotalLeastSquaresRegression Create<T, TV>(IDataFrame<T, double, TV> x, bool withConstant = true, bool computeErr = true, SVDType svdType = SVDType.POWER_ITERATION, double epsilon = 1e-10, bool centering = true, bool scaling = true) where T : IEquatable<T>, IComparable<T> where TV : IEquatable<TV>, IConvertible
        {
            #region requirements
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (x.Columns == 0 || x.Rows == 0) throw new ArgumentException("the data is not consistent");
            #endregion

            return Create(x.Data, withConstant, computeErr, svdType, epsilon, centering, scaling); 
        }
        #endregion


        /// <summary>
        /// Run the Total Least Squares regression
        /// </summary>
        public void Regress()
        {
            #region pre-process data by scaling
            //Scaling xScaler = Regressions.Scaling.CreateZScore(X);
            //if (xScaler == null)
            //{
            //    Status = RegressionStatus.BadData;
            //    return;
            //}

            //Matrix Xs = xScaler.Reduce(X, Centering, Scaling);
            Scaling[] scalings = Regressions.Scaling.MatrixColumnWiseScaler(X);
            Matrix Xs = Regressions.Scaling.MatrixColumnWiseReducer(X, scalings, Centering, Scaling);
            #endregion

            SingularValueDecomposition svd = SingularValueDecomposition.Run(Xs, SVDMethod, Epsilon);

            Matrix V = svd.V;
            int n = V.Rows - 1, p = V.Columns - 1;

            Beta = Vector.Create(n);

            for(int i = 0; i < n; i++) Beta[i] = (-1*V[i,p]) / V[n,p];

            #region compute error
            if (ComputeErr)
            {
                double ssr = 0, sse = 0, y_ = 0, x_ = 0;
                double[] e = new double[Xs.Rows];

                for (int i = 0; i < X.Rows; i++)
                {
                    for (int j = 0; j < Xs.Columns - 1; j++) x_ += Xs[i, j];
                    y_ += Xs[i, p];
                }
                y_ /= (Xs.Rows * 1.0);
                x_ /= (Xs.Rows * 1.0);

                double x_squares = 0, y_squares = 0;
                for (int i = 0; i < Xs.Rows; i++)
                {
                    double yhat = 0;
                    for (int j = 0; j < Xs.Columns - 1; j++)
                    {
                        yhat += Xs[i, j] * Beta[j];
                        x_squares += Math.Pow(Xs[i, j] - x_, 2.0);
                    }

                    y_squares += Math.Pow(Xs[i, p] - y_, 2.0);

                    ssr += Math.Pow(yhat - y_, 2);
                    sse += Math.Pow(Xs[i, p] - yhat, 2);
                    e[i] = yhat;
                }

                Matrix corr = Matrix.Corr(X);

                int w = 0;
                double[] correls = new double[Beta.Size];
                for (int i = 1; i < corr.Rows; i++)
                    for (int j = 0; j < corr.Columns - 1; j++)
                    {
                        correls[w] = corr[i, j];
                        w++;
                    }

                #region compute constant
                double alpha = 0;
                if (WithConstant)
                {
                    for (int j = 0; j < X.Columns - 1; j++) alpha -= Beta[j] * x_;
                    alpha += y_;
                }
                #endregion

                Linear = LinearModel.Create(alpha, Beta.Data, correls, Xs.Rows, sse, ssr, Vector.Create(e));
            }
            #endregion

            Status = RegressionStatus.Normal;
        }

        #endregion
    }
}

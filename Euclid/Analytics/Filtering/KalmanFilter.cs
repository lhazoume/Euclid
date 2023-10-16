using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Filtering
{
    /// <summary>
    /// Implements the kalman filter (following description from https://en.wikipedia.org/wiki/Kalman_filter#Example_application,_technical)
    /// </summary>
    public class KalmanFilter
    {
        #region vars
        /// <summary>
        /// State transition matrix
        /// </summary>
        public Matrix F { get; private set; }
        /// <summary>
        /// Control input matrix
        /// </summary>
        public Matrix B { get; private set; }
        /// <summary>
        /// Observation matrix
        /// </summary>
        public Matrix H { get; private set; }
        /// <summary>
        /// Covariance matrix of the process noise (covariance of transition noise / noise in the evolution system) - describe how the states change over time
        /// </summary>
        public Matrix Q { get; private set; }
        /// <summary>
        /// Covariance matrix of the measurement noise
        /// </summary>
        public Matrix R { get; private set; }
        /// <summary>
        /// State covariance
        /// </summary>
        public Matrix P { get; private set; }
        /// <summary>
        /// Measurements (Vector)
        /// </summary>
        public Vector Y { get; private set; }
        /// <summary>
        /// State mean matrix
        /// </summary>
        public Matrix X { get; private set; }
        /// <summary>
        /// Control vector, representing the controlling input into control input
        /// </summary>
        public Matrix U { get; private set; }
        /// <summary>
        /// Error
        /// </summary>
        public string Err { get; private set; }
        /// <summary>
        /// Nb of time steps (observations)
        /// </summary>
        public int N { get; private set; }
        /// <summary>
        /// Nb of dimension states
        /// </summary>
        public int M { get; private set; }
        /// <summary>
        /// Collection of Filtered state covariance
        /// </summary>
        public Matrix[] Pfs { get; private set; }
        /// <summary>
        /// Collection of Predicted state covariance
        /// </summary>
        public Matrix[] Pps { get; private set; }
        /// <summary>
        /// Collection of Smoothed state covariance
        /// </summary>
        public Matrix[] Psm { get; private set; }
        /// <summary>
        /// Collection of Filtered state mean
        /// </summary>
        public Matrix[] Xfs { get; private set; }
        /// <summary>
        /// Collection of Predicted state mean
        /// </summary>
        public Matrix[] Xps { get; private set; }
        /// <summary>
        /// Collection of Smoothed state mean
        /// </summary>
        public Matrix[] Xsm { get; private set; }
        /// <summary>
        /// Loglikelihood fit
        /// </summary>
        public double Loglikelihood { get; private set; }
        /// <summary>
        /// R² of the pre fit residual(s)
        /// </summary>
        public double R2Pre { get; private set; }
        /// <summary>
        /// R² of the post fit residual(s)
        /// </summary>
        public double R2Post { get; private set; }
        /// <summary>
        /// Measurement pre-fit & post-fit prediction matrix
        /// </summary>
        private double _yhatPreSumSqrt, _yhatPostSumSqrt;
        /// <summary>
        /// Measurement variance prediction
        /// </summary>
        public Matrix Shat { get; private set; }
        /// <summary>
        /// Enable state covaraince and mean recording for smoothing
        /// </summary>
        public bool Smoother { get; private set; }

        private double _log2Pi;
        #endregion

        #region constructor
        /// <summary>
        /// Kalman filter constructor
        /// </summary>
        /// <param name="f">State transition matrix</param>
        /// <param name="b">Control input matrix</param>
        /// <param name="h">Observation matrix</param>
        /// <param name="q">Covariance matrix of the process noise</param>
        /// <param name="r">Covariance matrix of the measurement noise</param>
        /// <param name="p">State covariance</param>
        /// <param name="y">Measurements (Vector)</param>
        /// <param name="x0">State mean</param>
        /// <param name="u">Control vector</param>
        /// <param name="smoother">True, record state covaraince and mean otherwise not</param>
        private KalmanFilter(Matrix f, Matrix b, Matrix h, Matrix q, Matrix r, Matrix p, Vector y, Matrix x0, Matrix u, bool smoother)
        {
            F = f;
            H = h;
            Y = y;

            M = H.Columns;
            N = H.Rows;

            U = u == null ? Matrix.Create(M, 1, 0) : u;
            B = b == null ? Matrix.CreateSquare(M) : b;

            Q = q == null ? Matrix.CreateIdentityMatrix(M, M) : q;
            R = r == null ? Matrix.CreateIdentityMatrix(M, M) : r;
            P = p == null ? Matrix.CreateIdentityMatrix(M, M) : p;

            X = x0 == null ? Matrix.Create(M, 1, 0) : x0;

            Pfs = new Matrix[N];
            Pps = new Matrix[N];
            Xfs = new Matrix[N];
            Xps = new Matrix[N];

            _yhatPostSumSqrt = _yhatPreSumSqrt = 0;
            Shat = Matrix.Create(N, 1, 0);

            _log2Pi = Math.Log(2.0 * Math.PI);

            Smoother = smoother;
        }
        #endregion

        #region methods
        #region creator
        /// <summary>
        /// Create a Kalman filter object
        /// </summary>
        /// <param name="y">Measurements (Vector)</param>
        /// <param name="f">State transition matrix</param>
        /// <param name="h">Observation matrix</param>
        /// <param name="b">Control input matrix</param>
        /// <param name="q">Covariance matrix of the process noise</param>
        /// <param name="r">Covariance matrix of the measurement noise</param>
        /// <param name="p">State covariance</param>
        /// <param name="x0">State mean</param>
        /// <param name="u">Control vector</param>
        /// <param name="smoother">True, record state covaraince and mean otherwise not</param>
        /// <returns></returns>
        public static KalmanFilter Create(Vector y,Matrix f, Matrix h, Matrix b = null, Matrix q = null, Matrix r = null, Matrix p = null, Matrix x0 = null, Matrix u = null, bool smoother = false) 
        {
            try
            {
                #region requiremen(s)
                if (f == null || f.Rows <= 0 || f.Columns <= 0) throw new Exception("State transition matrix is missing");
                if (h == null || h.Rows <= 0 || h.Columns <= 0) throw new Exception("Observation matrix is missing");

                if (y.Size != h.Rows) throw new Exception($"Wrong dimension, the number of observations is not equal to the number of measurement Y [{y.Size}] != H[{h.Rows},{h.Columns}]");
                
                if (q != null && (!q.IsSquare || q.Columns > h.Columns)) throw new Exception($"Q[{q.Rows},{q.Columns}] has not acceptable dimension!");
                if (r != null && (!r.IsSquare || r.Columns > h.Columns)) throw new Exception($"R[{q.Rows},{q.Columns}] has not acceptable dimension!");
                if (p != null && (!p.IsSquare || p.Columns > h.Columns)) throw new Exception($"P[{q.Rows},{q.Columns}] has not acceptable dimension!");
                if (x0 != null && (x0.Columns > h.Columns)) throw new Exception($"x[{x0.Rows},{x0.Columns}] has not acceptable dimension!");
                #endregion

                return new KalmanFilter(f, b, h, q, r, p, y, x0, u, smoother);
            }

            catch(Exception ex) { throw ex; }
        }
        #endregion

        #region filtering
        /// <summary>
        /// Predict equation at t
        /// </summary>
        /// <param name="t">Indice t</param>
        public void Predict(int t)
        {
            X = F * X + B* U; // predicted (a priori) state estimate
            P = F * P * F.FastTranspose + Q; // predicted (a priori) estimate covariance

            if (!Smoother) return;

            Xps[t] = X.Clone;
            Pps[t] = P.Clone;
        }

        /// <summary>
        /// Update equation at t
        /// </summary>
        /// <param name="t">Indice t</param>
        public void Update(int t)
        {
            #region requirement
            Matrix Ht = H.RowMatrix(t), Ht_ = Ht.FastTranspose;
            #endregion

            Matrix y = Y[t] - Ht * X; // innovation or measurement pre-fit residual
            Matrix S = Ht * P * Ht_ + R; // innovation (or pre-fit residual) covariance
            Matrix K = P * Ht_ * S.FastInverse; // optimal kaman gain
            X = X + K * y; // updated (a posteriori) state estimate
            Matrix I = Matrix.CreateIdentityMatrix(M, M);
            P = (I - K * Ht) * P * (I - K * Ht).Transpose + K * R * K.Transpose; // Updated (a posteriori) estimate covariance

            Loglikelihood += 0.5 * (-1.0 * _log2Pi - Math.Log(S[0, 0]) - ((y[0, 0] * y[0, 0]) / S[0, 0]));

            #region measurement metrics
            Shat[t, 0] = S[0, 0];
            _yhatPreSumSqrt += Math.Pow(y[0, 0], 2);
            _yhatPostSumSqrt += Math.Pow((Y[t] - Ht * X)[0,0], 2); // measurement post-fit residual
            #endregion

            if (!Smoother) return;

            Xfs[t] = X.Clone;
            Pfs[t] = P.Clone;
        }

        /// <summary>
        /// Run the iterative Kalman filter
        /// </summary>
        public void Filter()
        {
            try
            {
                #region requirement
                Loglikelihood = 0;
                #endregion

                for (int t = 0; t < N; t++)
                {
                    if(t == 0)
                    {
                        Xps[t] = X.Clone;
                        Pps[t] = P.Clone;
                    }

                    else Predict(t);

                    Update(t);
                }

                #region compute fitting metric(s)
                double sstot = (Y - (Y.Sum * (1.0 / Y.Size))).SumOfSquares;
                R2Pre = 1 - (_yhatPreSumSqrt / sstot);
                R2Post = 1 - (_yhatPostSumSqrt / sstot);
                #endregion
            }

            catch (Exception ex)  { Err = $"KalmanFilter.Filter: {ex.Message}"; }
        }

        /// <summary>
        /// Kalman smoother for hidden states. It uses the backward RTS algorithm (Rauch-Tung-Striebel smoother).
        /// </summary>
        public void Smooth()
        {
            try
            {
                #region initialization
                Psm = new Matrix[N];
                Xsm = new Matrix[N];

                int N_1 = N - 1;
                Xsm[N_1] = Xfs[N_1];
                Psm[N_1] = Pfs[N_1];
                #endregion

                for(int t = N_1 - 1; t >= 0; t--)
                {
                    #region requirement
                    Matrix Pf = Pfs[t], Pp = Pps[t + 1], Xf = Xfs[t], Xp = Xps[t + 1];
                    #endregion

                    Matrix C = Pf * F.Transpose * Pp.FastInverse;
                    Xsm[t] = Xf + C * (Xsm[t + 1] - Xp);
                    Psm[t] = Pf + C * (Psm[t + 1] - Pp) * C.FastTranspose;
                }
            }

            catch (Exception ex) { Err = $"KalmanFilter.Smooth: {ex.Message}"; }
        }
        #endregion
        #endregion
    }
}

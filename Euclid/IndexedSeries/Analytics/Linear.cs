using System;
using System.Collections.Generic;

namespace Euclid.IndexedSeries.Analytics
{
    public class LinearModel : IPredictor<double, double>
    {
        #region Declarations
        private readonly double _constant;
        private readonly double[] _factors;
        private readonly double _sse, _ssr, _sst;
        private readonly int _n, _p;
        private readonly bool _succeeded;
        #endregion

        #region Constructors

        private LinearModel(double constant, double[] factors, int sampleSize, double SSE, double SSR, bool succeeded)
        {
            _succeeded = succeeded;

            _p = factors.Length;
            _n = sampleSize;

            _sse = SSE;
            _ssr = SSR;
            _sst = _ssr + _sse;

            _constant = constant;
            _factors = new double[factors.Length];
            for (int i = 0; i < factors.Length; i++)
                _factors[i] = factors[i];
        }

        public LinearModel()
            : this(0, new double[] { 0 }, 0, 0, 0, false)
        { }

        public LinearModel(double constant, int sampleSize, double SSE)
            : this(constant, new double[] { 0 }, sampleSize, SSE, 0, true)
        { }

        public LinearModel(double constant, double[] factors, int sampleSize, double SSE, double SSR)
            : this(constant, factors, sampleSize, SSE, SSR, true)
        { }

        #endregion

        #region Accessors
        public double Constant
        {
            get { return _succeeded ? _constant : 0; }
        }
        public double[] Factors
        {
            get { return _succeeded ? _factors : new double[] { 0 }; }
        }
        public double R2
        {
            get { return _succeeded ? _ssr / _sst : 0; }
        }
        public double AdjustedR2
        {
            get
            {
                if (_succeeded)
                    return _sst == 0 ? 0 : 1 - _sse * (_n - 1) / (_sst * (_n - _p - 1));
                return 0;
            }
        }
        public bool Succeeded
        {
            get { return _succeeded; }
        }
        public double SSE
        {
            get { return _sse; }
        }
        public double SSR
        {
            get { return _ssr; }
        }
        public double SST
        {
            get { return _sst; }
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            string result = string.Join(";", "[R2=" + R2 + "]", "[AdjustedR2=" + AdjustedR2 + "]", "[Constant=" + _constant + "]");
            return result;
        }
        #endregion

        #region IPredictor
        public double Predict(IList<double> x)
        {
            if (_succeeded)
            {
                double y = _constant;
                for (int i = 0; i < Math.Min(_factors.Length, x.Count); i++)
                    y += _factors[i] * x[i];
                return y;
            }
            return _constant;
        }
        #endregion
    }
}

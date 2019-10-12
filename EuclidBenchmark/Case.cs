using Euclid.Serialization;
using System;
using System.Diagnostics;

namespace EuclidBenchmark
{
    public class Case
    {
        #region Variables
        private readonly Action<int> _action;
        private readonly string _name;
        private readonly int _iterations;
        private TimeSpan _result;
        #endregion

        public Case(string name, int iterations, Action<int> action)
        {
            _name = name;
            _result = new TimeSpan();
            _iterations = iterations;
            _action = action;
        }

        public void Run()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _action(_iterations);
            sw.Stop();
            _result = sw.Elapsed;
        }
        public string Name
        {
            get { return _name; }
        }
        public TimeSpan Result
        {
            get { return _result; }
        }

        public override string  ToString()
        {
            return string.Format("{0}({1})={2}", _name, _iterations, _result.ToShortString());
        }
    }
}

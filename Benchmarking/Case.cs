using System;
using System.Diagnostics;

namespace Benchmarking
{
    public class Case
    {
        #region Variables
        private readonly Action<int> _action;
        private readonly string _name;
        private readonly int _iterations;
        private TimeSpan _timeSpan;
        private long _memoryUsage;
        #endregion

        public Case(string name, int iterations, Action<int> action)
        {
            _name = name;
            _timeSpan = new TimeSpan();
            _iterations = iterations;
            _action = action;
        }

        public CaseResult Run()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = Process.GetCurrentProcess().VirtualMemorySize64;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            _action(_iterations);
            sw.Stop();

            long after = Process.GetCurrentProcess().VirtualMemorySize64;

            _timeSpan = sw.Elapsed;
            _memoryUsage = after - before;

            return CaseResult.Build(_name, _iterations, _timeSpan, _memoryUsage);
        }
        public string Name => _name; 
    }
}

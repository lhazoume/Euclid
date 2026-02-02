using Euclid.Benchmarking;
using System;
using System.Collections.Generic;

namespace EuclidBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            CaseSet caseSet = CaseSet();
            List<CaseResult> results = caseSet.Run();
            results.ForEach(cr => Console.WriteLine(cr.ToString()));
            Console.ReadLine();
        }

        private static CaseSet CaseSet()
        {
            List<Case> cases = new List<Case>
            {
                new Case("MultiplyScalar", 10000000, VectorCases.MultiplyScalar),
                new Case("MultiplyVector", 10000000, VectorCases.MultiplyVector),
                new Case("AddVector", 10000000, VectorCases.AddVector),
                new Case("AddVectorScalar", 10000000, VectorCases.AddVectorScalar),
                new Case("SubstractVectorScalar", 10000000, VectorCases.SubstractVectorScalar)
            };
            return new CaseSet(cases);
        }
    }
}

using Benchmarking;
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
            List<Case> cases = new List<Case>();
            cases.Add(new Case("MultiplyScalar", 10000000, VectorCases.MultiplyScalar));
            cases.Add(new Case("MultiplyVector", 10000000, VectorCases.MultiplyVector));
            cases.Add(new Case("AddVector", 10000000, VectorCases.AddVector));
            cases.Add(new Case("AddVectorScalar", 10000000, VectorCases.AddVectorScalar));
            cases.Add(new Case("SubstractVectorScalar", 10000000, VectorCases.SubstractVectorScalar));
            return new CaseSet(cases);
        }
    }
}

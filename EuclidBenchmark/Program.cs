using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuclidBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            CaseSet caseSet = Vector();
            Console.WriteLine(caseSet.Run());
            Console.ReadLine();

        }

        private static CaseSet Vector()
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

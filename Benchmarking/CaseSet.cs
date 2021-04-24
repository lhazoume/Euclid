using System;
using System.Collections.Generic;
using System.Linq;

namespace Benchmarking
{
    public class CaseSet
    {
        private readonly List<Case> _cases;

        public CaseSet(IEnumerable<Case> cases)
        {
            _cases = cases.ToList();
        }

        public List<CaseResult> Run()
        {
            List<CaseResult> results = new List<CaseResult>();

            for (int i = 0; i < _cases.Count; i++)
                results.Add(_cases[i].Run());

            return results;
        }
    }
}

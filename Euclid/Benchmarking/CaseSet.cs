﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Benchmarking
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

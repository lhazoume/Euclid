using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuclidBenchmark
{
    public class CaseSet
    {
        private readonly List<Case> _cases;

        public CaseSet(IEnumerable<Case> cases)
        {
            _cases = cases.ToList();
        }

        public string Run()
        {
            List<string> result = new List<string>();

            for (int i = 0; i < _cases.Count; i++)
            {
                _cases[i].Run();
                result.Add(_cases[i].ToString());
            }

            return string.Join(Environment.NewLine, result);
        }
    }
}

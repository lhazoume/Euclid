using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Arithmetics
{
    public static class Subsets
    {
        public static List<List<T>> SubSets<T>(List<T> data)
        {
            if (data.Count() == 0)
                return new List<List<T>>() { new List<T>() };

            List<T> element = data.Take(1).ToList();

            List<List<T>> haveNots = SubSets(data.Skip(1).ToList()),
                haves = haveNots.Select(set => element.Concat(set).ToList()).ToList();

            return haves.Concat(haveNots).ToList();
        }
    }
}

using Euclid;
using Euclid.Arithmetics;
using Euclid.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEuclid
{
    public static class Tests
    {
        public delegate bool TestMethod();

        public static bool AlwaysPassed()
        {
            return true;
        }
        public static bool AlwaysFailed()
        {
            return false;
        }

    }
}

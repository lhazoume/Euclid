using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Statistics.Tests
{
    /// <summary>Specifies the null hypothesis test</summary>
    public enum RejectionRegion
    {
        /// <summary>The test is left tailed</summary>
        Left = 0,
        /// <summary>The test is right tailed</summary>
        Right = 1,
        /// <summary>The test is two tailed</summary>
        Bilateral = 2,
    }
}

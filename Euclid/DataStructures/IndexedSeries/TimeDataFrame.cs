using Euclid.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>
    /// Class representing a DataFrame of synchronized data ordered by time 
    /// </summary>
    /// <typeparam name="DateTime">The legend type</typeparam>
    /// <typeparam name="TU">The data type</typeparam>
    /// <typeparam name="TV">The lable type</typeparam>
    public sealed class TimeDataFrame<DateTime, TU, TV> : DataFrame<DateTime, TU, TV> where DateTime : struct, IComparable<DateTime>, IEquatable<DateTime> where TV : IEquatable<TV>
    {
        #region vars

        #endregion

        #region constructor
        /// <summary>
        /// Protected constructor
        /// </summary>
        /// <param name="labels">Labels, serie of generic type</param>
        /// <param name="legends">Legends, serie of datetime type</param>
        /// <param name="data">Matric of generic type</param>
        protected TimeDataFrame(IList<TV> labels, IList<DateTime> legends, TU[,] data) : base(labels, legends, data) { }
        #endregion

        #region methods

        #endregion
        
    }
}

using Euclid.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Extensions
{
    /// <summary>
    /// Implements an helper for processing datetime
    /// </summary>
    public static class DateTimeCollectionHelper
    {
        #region method(s)
        /// <summary>
        /// return the intersection of both datetime collections
        /// </summary>
        /// <param name="candidats">Candidats</param>
        /// <param name="matches">Matches</param>
        /// <returns>Common</returns>
        public static ResultOutput<IReadOnlyList<DateTime>> Intersection(this IReadOnlyList<DateTime> candidats, IReadOnlyList<DateTime> matches)
        {
            try
            {
                #region prerequires
                List<DateTime> common = new List<DateTime>();
                int k = 0;
                #endregion

                for (int i = 0; i < candidats.Count; i++)
                {
                    DateTime candidat = candidats[i], attempt = matches[k];

                    if (candidat < attempt) continue;
                    else if (candidat > attempt)
                    {
                        while (k < matches.Count && matches[k] < candidat) k++;
                        attempt = matches[k];
                    }
                    if (candidat == attempt) common.Add(candidat);
                }

                return common;
            }

            catch (Exception ex) { return ResultOutput<IReadOnlyList<DateTime>>.CreateFailed($"DateTimeCollectionHelper.Intersection: {ex.Message}"); }
        }
        #endregion
    }
}

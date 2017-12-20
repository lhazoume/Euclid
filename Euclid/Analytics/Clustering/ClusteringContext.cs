using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Analytics.Clustering
{
    /*
    public enum VarType
    {
        Enum = 0,
        Continuous = 1
    }

    public class ClusteringContext<T> where T : IComparable<T>, IEquatable<T>
    {
        #region Private Vars

        private string[] _labels;
        private VarType[] _types;
        private Dictionary<int, double[]> _enumValues;

        #endregion

        /// <summary>
        /// Generates the clustering context from the header, the types of the variables and 
        /// </summary>
        /// <param name="headers">Names of the predictors (variables).</param>
        /// <param name="types">Types of the predictors (variables)</param>
        /// <param name="dictionaries">Dictionaries associated to the enum. Unused so far. May contain weights or counts.</param>
        public ClusteringContext(IEnumerable<string> headers, IEnumerable<VarType> types, Dictionary<int, double[]> dictionaries)
        {
            if (headers.Count() == types.Count() && headers.Count() == dictionaries.Count())
            {
                _labels = headers.ToArray();
                _types = types.ToArray();
                _enumValues = new Dictionary<int, double[]>(dictionaries);
            }
            else
            {
                _labels = new string[0];
                _types = new VarType[0];
                _enumValues = new Dictionary<int, double[]>();
            }
        }

        public ClusteringContext(DataFrame<T, double, string> data)
        {
            int n = data.Columns;
            _labels = data.Labels;
            _types = new VarType[n];
            _enumValues = new Dictionary<int, double[]>();
            for (int i = 0; i < n; i++)
            {
                double[] values = data.GetSeriesAt(_labels[i]).Data.Distinct().OrderBy(d => d).ToArray();
                if (values.Length > 10)
                    _types[i] = VarType.Continuous;
                else
                {
                    _types[i] = VarType.Enum;
                    _enumValues.Add(i, values);
                }
            }
        }

        /// <summary>
        /// Returns the VarType of the variable specified by the index.
        /// </summary>
        /// <param name="index">Index of the variable.</param>
        /// <returns>The VarType of the predictor (variable) with the specified index.</returns>
        public VarType Type(int index)
        {
            return _types[index];
        }

        /// <summary>
        /// Returns the name (specified by the header) of the variable specified by the index.
        /// </summary>
        /// <param name="index">Index of the variable.</param>
        /// <returns>The name of the predictor with the specified index.</returns>
        public string Labels(int index)
        {
            return _labels[index];
        }

        /// <summary>
        /// Returns the pair associated to the variable specified by the index.
        /// </summary>
        /// <param name="index">Index of the variable.</param>
        /// <returns>The Pair associated to the predictor (variable) with the specified index.</returns>
        public double[] Dictionary(int index)
        {
            if (_enumValues.ContainsKey(index))
                return _enumValues[index];
            else
                return new double[0];
        }

    }
    */
}

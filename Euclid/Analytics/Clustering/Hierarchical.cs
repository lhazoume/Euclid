using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Clustering
{
    public class Hierarchical<T, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IConvertible
    {
        private class Cluster
        {
            private List<int> _indices = new List<int>();

            public Cluster(IEnumerable<int> indices)
            {
                _indices.AddRange(indices);
            }

            public int Count
            {
                get { return _indices.Count; }
            }
            public List<int> Indices
            {
                get { return _indices; }
            }
        }

        private List<Vector> _predictors = new List<Vector>();
        private Vector _predicted;
        private List<Cluster> _clusters = new List<Cluster>();
        private double _totalAverage, _totalVariance;

        public Hierarchical(DataFrame<T, double, V> data, Series<T, double, V> output)
        {
            Slice<T, double, V>[] rows = data.GetSlices();
            for (int i = 0; i < rows.Length; i++)
                _predictors.Add(Vector.Create(rows[i].Data));
            _predicted = Vector.Create(output.Data);
            int n = _predicted.Size;
            _totalAverage = _predicted.Sum / n;
            _totalVariance = Math.Pow(_totalAverage, 2) - _predicted.SumOfSquares / n;
        }

        public void PerformOneStep()
        {
            int i0 = 0,
                j0 = 1;
            double minDistance = double.MaxValue;

            for (int i = 0; i < _clusters.Count - 1; i++)
                for (int j = i + 1; j < _clusters.Count; j++)
                {
                    Cluster ci = _clusters[i],
                        cj = _clusters[j];

                    double dissimilarity = 0;
                    #region Calculate the dissimilarity between the clusters
                    for (int k = 0; k< ci.Count;k++)
                        for(int p = 0; p < cj.Count; p++)
                        {
                            double localDistance = (_predictors[ci.Indices[k]] - _predictors[cj.Indices[p]]).Norm2;
                            if (dissimilarity < localDistance) dissimilarity = localDistance;
                        }
                    #endregion

                    if(dissimilarity <minDistance)
                    {
                        i0 = i;
                        j0 = j;
                        minDistance = dissimilarity;
                    }
                }
            _clusters[i0].Indices.AddRange(_clusters[j0].Indices);
            _clusters.RemoveAt(j0);
        }

        public double TotalVariance
        {
            get { return _totalVariance; }
        }

        public double BetweenGroupsVariance
        {
            get
            {
                double result = 0;
                for (int i = 0; i < _clusters.Count; i++)
                {
                    int count = _clusters[i].Count;
                    double localSum = 0;
                    for (int j = 0; j < count; j++)
                        localSum += _predicted[_clusters[i].Indices[j]];
                    localSum /= count;
                    result += count * Math.Pow(localSum - _totalAverage, 2);
                }
                return result;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Analytics.Clustering
{
    public class KMeans<T>
    {
        #region Private fields
        private readonly Func<IEnumerable<T>, T> _aggregator;
        private readonly Func<T, T, double> _distance;
        private int _maxIterations;
        #endregion

        public KMeans(Func<IEnumerable<T>, T> aggregator, Func<T, T, double> distance, int maxIterations)
        {
            _aggregator = aggregator;
            _distance = distance;
            _maxIterations = maxIterations;
        }

        /// <summary>Gets and sets the maximum number of iterations</summary>
        public int MaxIterations
        {
            get => _maxIterations;
            set => _maxIterations = value;
        }

        #region Methods

        /// <summary>Provides the centroïds for the </summary>
        /// <param name="data">the data to cluster</param>
        /// <param name="numberOfClasses">the target number of classes</param>
        /// <returns>an Enumerable of centroïds</returns>
        public IEnumerable<T> Cluster(IList<T> data, int numberOfClasses)
        {
            if (data.Count() <= numberOfClasses)
                throw new ArgumentOutOfRangeException("the sample is smaller than the expected number of classes");

            T[] previousCentroids = new T[numberOfClasses],
                newCentroids = new T[numberOfClasses];
            for (int i = 0; i < numberOfClasses; i++)
                previousCentroids[i] = data[i];
            bool shouldContinue = true;
            int iterations = 0;
            while (shouldContinue)
            {
                #region Assign classes to the data points
                List<T>[] classes = new List<T>[numberOfClasses];
                for (int i = 0; i < data.Count; i++)
                {
                    //Collects the distances from the data points to all centroïds
                    double[] distances = Enumerable.Range(0, numberOfClasses).Select(j => _distance(data[i], previousCentroids[j])).ToArray();

                    //Identifies the nearest centroïd 
                    double shortestDistance = distances.Min();
                    int nearestCentroid = Array.IndexOf(distances, shortestDistance);

                    //assigns the data point to a class
                    if (classes[nearestCentroid] == null)
                        classes[nearestCentroid] = new List<T>();
                    classes[nearestCentroid].Add(data[i]);
                }
                #endregion

                #region Build centroïds and mesure change 
                double centroidTotalDistance = 0;
                for (int j = 0; j < numberOfClasses; j++)
                {
                    newCentroids[j] = _aggregator(classes[j]);
                    centroidTotalDistance += _distance(newCentroids[j], previousCentroids[j]);
                }
                #endregion

                iterations++;

                if (centroidTotalDistance == 0 || iterations > _maxIterations)
                    shouldContinue = false;
                else
                    previousCentroids = newCentroids;
            }

            return newCentroids;
        }

        public Dictionary<int, IList<T>> Classes(IList<T> data, IList<T> centroids)
        {
            Dictionary<int, IList<T>> classes = new Dictionary<int, IList<T>>();
            foreach (T t in data)
            {
                //Collects the distances from the data points to all centroïds
                double[] distances = Enumerable.Range(0, centroids.Count).Select(j => _distance(t, centroids[j])).ToArray();

                //Identifies the nearest centroïd 
                double shortestDistance = distances.Min();
                int nearestCentroid = Array.IndexOf(distances, shortestDistance);

                //assigns the data point to a class
                if (!classes.ContainsKey(nearestCentroid))
                    classes.Add(nearestCentroid, new List<T>());

                classes[nearestCentroid].Add(t);
            }

            return classes;
        }

        #endregion
    }
}

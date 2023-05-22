using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Analytics.Clustering
{
    /// <summary>Represents a K-means clusterer</summary>
    /// <typeparam name="T">the underlying object clustered</typeparam>
    public class KMeans<T>
    {
        #region Private fields
        private readonly Func<IEnumerable<T>, T> _aggregator;
        private readonly Func<T, T, double> _distance;
        private int _maxIterations;
        #endregion

        /// <summary>Builds a K-means clusterer</summary>
        /// <param name="aggregator"></param>
        /// <param name="distance"></param>
        /// <param name="maxIterations"></param>
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

        /// <summary>Provides the centroïds for the K-means</summary>
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

        /// <summary>Returns the classes for all centroïds</summary>
        /// <param name="data">the data to cluster</param>
        /// <param name="centroids">the pre-calculated centroïds</param>
        /// <returns>a dictionary of item list per centroïd index</returns>
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


        /// <summary>Provides the optimal centroïds for the K-means using the silhouette criterion</summary>
        /// <param name="data">the data to cluster</param>
        /// <param name="maxNumberOfClasses">the maximum number of classes</param>
        /// <returns>an Enumerable of centroïds</returns>
        public IEnumerable<T> ClusterSilhouette(IList<T> data, int maxNumberOfClasses)
        {
            List<Tuple<double, List<T>>> results = new List<Tuple<double, List<T>>>();

            for (int k = 2; k <= maxNumberOfClasses; k++)
            {
                List<T> centroids = new List<T>(Cluster(data, k));
                Dictionary<int, IList<T>> classes = Classes(data, centroids);
                results.Add(new Tuple<double, List<T>>(SimplifiedSilhouette(classes, centroids), centroids));
            }

            double minSilhouette = results.Max(t => t.Item1);
            return results.Find(t => t.Item1 == minSilhouette).Item2;
        }

        private double SimplifiedSilhouette(Dictionary<int, IList<T>> classes, IList<T> centroids)
        {
            double sumSilhouettes = 0;
            int nbPoints = classes.Sum(kvp => kvp.Value.Count);

            for (int k = 0; k < centroids.Count; k++)
            {
                //the number of objects in the class
                int n = classes[k].Count;

                for (int i = 0; i < n; i++)
                {
                    //Distance to the natural centroïd
                    double a = _distance(classes[k][i], centroids[k]);

                    //Distance to the nearest centroïd
                    List<double> distancesToOtherCentroids = new List<double>();
                    for (int j = 0; j < centroids.Count; j++)
                        if (j != k)
                            distancesToOtherCentroids.Add(_distance(classes[k][i], centroids[j]));
                    double b = distancesToOtherCentroids.Min();

                    //Silhouette
                    double s = (b - a) / Math.Max(a, b);

                    sumSilhouettes += s;
                }
            }

            return sumSilhouettes / nbPoints;
        }
        #endregion
    }
}

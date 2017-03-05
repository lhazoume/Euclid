using Euclid.Distributions.Continuous;
using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Analytics.Clustering
{
    public class RandomForestGenerator<T> where T : IComparable<T>, IEquatable<T>
    {
        private Func<DataFrame<T, double, string>, Series<T, double, string>, IPredictor<double, double>> _modeller;

        #region Services
        private static int[] ShuffleAndSelect(int numberOfVariablesToPick, int numberOfVariablesToChooseAmong)
        {
            Random randomizer = new Random(Guid.NewGuid().GetHashCode());
            List<int> toTakeFrom = Enumerable.Range(0, numberOfVariablesToChooseAmong).ToList(),
                chosen = new List<int>();
            while (chosen.Count < numberOfVariablesToPick)
            {
                int index = randomizer.Next(toTakeFrom.Count),
                    value = toTakeFrom[index];
                chosen.Add(value);
                toTakeFrom.RemoveAt(index);
            }
            return chosen.ToArray();
        }
        private static Series<T, double, string> PredictedSeries(DataFrame<T, double, string> data, IPredictor<double, double> predictor)
        {
            return Series<T, double, string>.Create("predicted_Ŷ_true", new Header<T>(data.Legends), data.GetSlices().Select(s => predictor.Predict(s.Data)));
        }
        private static Tuple<DataFrame<T, double, string>[], Series<T, double, string>[]> SplitData(DataFrame<T, double, string> X, Series<T, double, string> Y, Predicate<Vector> predicate)
        {
            List<Slice<T, double, string>> sl1 = new List<Slice<T, double, string>>(),
                sl2 = new List<Slice<T, double, string>>();
            List<double> a1 = new List<double>(),
                a2 = new List<double>();

            Slice<T, double, string>[] slices = X.GetSlices();
            for (int i = 0; i < slices.Length; i++)
                if (predicate(Vector.Create(slices[i].Data)))
                {
                    sl1.Add(slices[i]);
                    a1.Add(Y[i]);
                }
                else
                {
                    sl2.Add(slices[i]);
                    a2.Add(Y[i]);
                }

            DataFrame<T, double, string> d1 = DataFrame<T, double, string>.Create(sl1),
                d2 = DataFrame<T, double, string>.Create(sl2);
            Series<T, double, string> s1 = Series<T, double, string>.Create("Ŷ1", new Header<T>(d1.Legends), a1),
                s2 = Series<T, double, string>.Create("Ŷ2", new Header<T>(d2.Legends), a2);
            return new Tuple<DataFrame<T, double, string>[], Series<T, double, string>[]>(new DataFrame<T, double, string>[] { d1, d2 }, new Series<T, double, string>[] { s1, s2 });
        }
        #endregion

        #region Split

        private PrettyPredicate<Vector> ProposeSplitForVariable(DataFrame<T, double, string> X, Series<T, double, string> Y,
            int varIndex,
            int minSize,
            ClusteringContext<T> context)
        {
            Series<T, double, string> variableSeries = X.GetSeriesAt(X.Labels[varIndex]);

            #region Check whether the data are the relevant sizes
            if (X.Rows < 2 * minSize)
                return null;

            double[] values = variableSeries.Data,
                distincts = values.Distinct().OrderBy(d => d).ToArray();

            if (distincts.Length == 1)
                return null;
            #endregion

            if (context.Type(varIndex) == VarType.Continuous)
            {
                #region set the barriers
                List<double> barriers = new List<double>();
                for (int i = 0; i < distincts.Length - 1; i++)
                    barriers.Add(0.5 * (distincts[i] + distincts[i + 1]));
                #endregion

                #region Split
                for (int i = 0; i < barriers.Count; i++)
                {
                    double barrier = barriers[i];
                    int above = values.Count(v => v > barrier);
                    if (above >= minSize && above <= values.Length - minSize && ShouldSplit(v => v[varIndex] > barrier, X, Y))
                        return new PrettyPredicate<Vector>(X.GetLabel(varIndex) + ">" + barrier, v => v[varIndex] > barrier);
                }
                #endregion
            }
            else // VarType.Enum
            {
                #region Split
                for (int i = 0; i < distincts.Length; i++)
                {
                    double barrier = distincts[i];
                    int equal = values.Count(v => v == barrier);
                    if (equal >= minSize && equal <= values.Length - minSize && ShouldSplit(v => v[varIndex] == barrier, X, Y))
                        return new PrettyPredicate<Vector>(X.GetLabel(varIndex) + "=" + barrier, v => v[varIndex] == barrier);
                }
                #endregion
            }

            return null;
        }

        private PrettyPredicate<Vector> ProposeSplit(DataFrame<T, double, string> X, Series<T, double, string> Y,
            int[] varIndices,
            int minSize,
            ClusteringContext<T> context)
        {
            foreach (int varIndex in varIndices)
            {
                PrettyPredicate<Vector> ppv = ProposeSplitForVariable(X, Y, varIndex, minSize, context);
                if (ppv != null) return ppv;
            }
            return null;
        }

        private bool ShouldSplit(Predicate<Vector> predicate, DataFrame<T, double, string> X, Series<T, double, string> Y)
        {
            Series<T, double, string> pY = PredictedSeries(X, _modeller(X, Y));
            double eC = (pY - Y).Sum(d => d * d);

            #region Split according to the predicate
            Tuple<DataFrame<T, double, string>[], Series<T, double, string>[]> splitDataFrame = SplitData(X, Y, predicate);

            DataFrame<T, double, string> d1 = splitDataFrame.Item1[0],
                d2 = splitDataFrame.Item1[1];
            Series<T, double, string> s1 = splitDataFrame.Item2[0],
                s2 = splitDataFrame.Item2[1];

            IPredictor<double, double> m1 = _modeller(d1, s1), m2 = _modeller(d2, s2);
            Series<T, double, string> pS1 = PredictedSeries(d1, m1), pS2 = PredictedSeries(d2, m2);
            double e1 = (pS1 - s1).Sum(d => d * d),
                e2 = (pS2 - s2).Sum(d => d * d);
            #endregion

            int k = X.Columns + 1,
                v1 = k,
                v2 = X.Rows - 2 * k;

            double F = (eC - e1 - e2) * v2 / (v1 * (e1 + e2));
            FisherDistribution fd = FisherDistribution.Create(v1, v2);
            double cd = fd.CumulativeDistribution(F);
            return cd <= 0.95;
        }

        #endregion

        #region Build node
        private IDecisionNode<Vector, IPredictor<double, double>> BuildNode(int[] varIndices,
            int minSize,
            DataFrame<T, double, string> X, Series<T, double, string> Y,
            ClusteringContext<T> context)
        {
            IDecisionNode<Vector, IPredictor<double, double>> result = new DecisionLeaf<Vector, IPredictor<double, double>>(_modeller(X, Y));

            PrettyPredicate<Vector> proposedSplit = ProposeSplit(X, Y, varIndices, minSize, context);
            if (proposedSplit != null)
            {
                Tuple<DataFrame<T, double, string>[], Series<T, double, string>[]> splitDataFrame = SplitData(X, Y, proposedSplit.Predicate);
                result = new DecisionNode<Vector, IPredictor<double, double>>(proposedSplit,
                    BuildNode(varIndices, minSize, splitDataFrame.Item1[0], splitDataFrame.Item2[0], context),
                    BuildNode(varIndices, minSize, splitDataFrame.Item1[1], splitDataFrame.Item2[1], context));
            }
            return result;
        }
        #endregion

        public DecisionTreePredictor[] Run(int nbTrees, int mininumLeafSize, int nbVars, ClusteringContext<T> context, DataFrame<T, double, string> X, Series<T, double, string> Y)
        {
            DecisionTreePredictor[] results = new DecisionTreePredictor[nbTrees];

            #region Build the trees
            for (int i = 0; i < nbTrees; i++)
            {
                int[] varIndices = ShuffleAndSelect(nbVars, X.Columns);
                IDecisionNode<Vector, IPredictor<double, double>> result = BuildNode(varIndices, mininumLeafSize, X, Y, context);
                results[i] = new DecisionTreePredictor(result);
            }
            #endregion

            return results;

        }

        public static RandomForestGenerator<T> Create(Func<DataFrame<T, double, string>, Series<T, double, string>, IPredictor<double, double>> modeller)
        {
            RandomForestGenerator<T> generator = new RandomForestGenerator<T>();
            generator._modeller = modeller;
            return generator;
        }
    }
}

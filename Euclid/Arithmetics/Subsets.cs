﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Arithmetics
{
    /// <summary>Subsets class</summary>
    public static class Subsets
    {
        /// <summary>Returns all possible subsets from a List</summary>
        /// <typeparam name="T">the template type</typeparam>
        /// <param name="data">the original data</param>
        /// <returns>the List of subsets</returns>
        public static List<List<T>> AllSubsets<T>(this List<T> data)
        {
            if (data.Count() == 0)
                return new List<List<T>>() { new List<T>() };

            List<T> element = data.Take(1).ToList();

            List<List<T>> haveNots = AllSubsets(data.Skip(1).ToList()),
                haves = haveNots.Select(set => element.Concat(set).ToList()).ToList();

            return haves.Concat(haveNots).ToList();
        }

        /// <summary>Returns all possible subsets of a given size from a list</summary>
        /// <typeparam name="T">the template type</typeparam>
        /// <param name="data">the original data</param>
        /// <param name="subsetSize">the subset size</param>
        /// <param name="startingIndex">the starting index</param>
        /// <returns>the list of subsets</returns>
        public static List<List<T>> SubsetsOfSize<T>(this List<T> data, int subsetSize, int startingIndex = 0)
        {
            List<List<T>> combinations = new List<List<T>>();
            if (subsetSize <= 0)
                return combinations;
            else if (subsetSize == 1)
            {
                foreach (T value in data)
                    combinations.Add(new List<T>() { value });
                return combinations;
            }
            else if (subsetSize == 2)
            {
                int combinationsListIndex = 0;
                for (int arrayIndex = startingIndex; arrayIndex < data.Count; arrayIndex++)
                    for (int i = arrayIndex + 1; i < data.Count; i++)
                    {
                        combinations.Add(new List<T>());                            //add new List in the list to hold the new combination
                        combinations[combinationsListIndex].Add(data[arrayIndex]);  //add the starting index element from "array"
                        while (combinations[combinationsListIndex].Count < subsetSize)
                            combinations[combinationsListIndex].Add(data[i]);       //add until we come to the length of the combination
                        combinationsListIndex++;
                    }
                return combinations;
            }
            else
            {
                List<List<T>> combinationsofMore = new List<List<T>>();
                for (int i = startingIndex; i < data.Count - subsetSize + 1; i++)
                {
                    combinations = SubsetsOfSize(data, subsetSize - 1, i + 1);   //generate combinations of lenght-1(if lenght > 2 we enter into recursion)
                                                                                 //add the starting index Elemetn in the begginnig of each newly generated list
                    for (int index = 0; index < combinations.Count; index++) combinations[index].Insert(0, data[i]);
                    for (int y = 0; y < combinations.Count; y++) combinationsofMore.Add(combinations[y]);
                }
                return combinationsofMore;
            }
        }

        /// <summary>Shuffles a list of template type</summary>
        /// <typeparam name="T">the template type<typeparam>
        /// <param name="list">the list to shuffle</param>
        /// <param name="randomNumberGenerator">the random number generator</param>
        public static void Shuffle<T>(this IList<T> list, Random randomNumberGenerator)
        {
            if (randomNumberGenerator == null) return;
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = randomNumberGenerator.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<int> PickRandomIndices(int maxExcludedValue, int numberOfPicks, Random randomNumberGenerator)
        {
            List<int> result = new List<int>();

            while(result.Count < numberOfPicks)
            {
                int randomNumber = randomNumberGenerator.Next(maxExcludedValue);
                if (!result.Contains(randomNumber))
                    result.Add(randomNumber);
            }

            return result;
        }

        public static List<T> CloneAndShuffle<T>(this IList<T> list, Random randomNumberGenerator)
        {
            List<int> indexes = Enumerable.Range(0, list.Count).ToList();
            indexes.Shuffle(randomNumberGenerator);
            return indexes.Select(i => list[i]).ToList();
        }
    }
}

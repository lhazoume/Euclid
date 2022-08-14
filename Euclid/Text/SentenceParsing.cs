using System;
using System.Linq;

namespace Euclid.Text
{
    /// <summary>Helps parsing sentences</summary>
    public static class SentenceParsing
    {
        /// <summary>Counts the words that are common to two sentences</summary>
        /// <param name="sentenceA">the first sentence</param>
        /// <param name="sentenceB">the second sentence</param>
        /// <returns>an int</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int CommonWords(string sentenceA, string sentenceB)
        {
            if (sentenceA == null) throw new ArgumentNullException(nameof(sentenceA));
            if (sentenceB == null) throw new ArgumentNullException(nameof(sentenceB));
            return sentenceA.Split(' ').Intersect(sentenceB.Split(' ')).Count();
        }
    }
}

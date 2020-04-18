﻿using System;
using System.Linq;

namespace Euclid.Text
{
    public static class SentenceParsing
    {
        public static int CommonWords(string sentenceA, string sentenceB)
        {
            if (sentenceA == null) throw new ArgumentNullException(nameof(sentenceA));
            if (sentenceB == null) throw new ArgumentNullException(nameof(sentenceB));
            return sentenceA.Split(' ').Intersect(sentenceB.Split(' ')).Count();
        }
    }
}

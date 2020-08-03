using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Text
{
    /// <summary>Helps computing string distances</summary>
    public static class Distance
    {
        /// <summary>Compute the distance between two strings</summary>
        public static int Levenshtein(string s, string t)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (t == null) throw new ArgumentNullException(nameof(t));

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            // Step 3
            for (int i = 1; i <= n; i++)
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            // Step 7
            return d[n, m];
        }

        /// <summary>Returns the anagram-distance between two strings</summary>
        /// <param name="stringA">one string</param>
        /// <param name="stringB"></param>
        /// <returns></returns>
        public static int Characters(string stringA, string stringB)
        {
            if (stringA == null) throw new ArgumentNullException(nameof(stringA));
            if (stringB == null) throw new ArgumentNullException(nameof(stringB));

            Dictionary<char, int> dictionaryA = stringA.ToCharArray().Distinct().ToDictionary(c => c, c => stringA.Count(ch => ch == c)),
                dictionaryB = stringB.ToCharArray().Distinct().ToDictionary(c => c, c => stringB.Count(ch => ch == c));
            char[] commonCharacters = dictionaryA.Keys.Intersect(dictionaryB.Keys).ToArray();

            int result = 0;
            foreach (char c in commonCharacters)
                result += Math.Abs(dictionaryA[c] - dictionaryB[c]);

            result += dictionaryA.Keys.Except(commonCharacters).Sum(c => dictionaryA[c]) + dictionaryB.Keys.Except(commonCharacters).Sum(c => dictionaryB[c]);

            return result;
        }

        #region Jaro Wrinkler

        /* The Winkler modification will not be applied unless the 
         * percent match was at or above the mWeightThreshold percent 
         * without the modification. 
         * Winkler's paper used a default value of 0.7
         */
        private const double mWeightThreshold = 0.7;

        /* Size of the prefix to be concidered by the Winkler modification. 
         * Winkler's paper used a default value of 4
         */
        private const int mNumChars = 4;


        /// <summary>
        /// Returns the Jaro-Winkler distance between the specified  
        /// strings. The distance is symmetric and will fall in the 
        /// range 0 (perfect match) to 1 (no match). 
        /// </summary>
        /// <param name="aString1">First String</param>
        /// <param name="aString2">Second String</param>
        /// <returns></returns>
        public static double JaroWrinklerDistance(string aString1, string aString2)
        {
            return 1.0 - JaroWrinklerProximity(aString1, aString2);
        }


        /// <summary>
        /// Returns the Jaro-Winkler distance between the specified  
        /// strings. The distance is symmetric and will fall in the 
        /// range 0 (no match) to 1 (perfect match). 
        /// </summary>
        /// <param name="aString1">First String</param>
        /// <param name="aString2">Second String</param>
        /// <returns></returns>
        public static double JaroWrinklerProximity(string aString1, string aString2)
        {
            if (aString1 == null) throw new ArgumentNullException(nameof(aString1));
            if (aString2 == null) throw new ArgumentNullException(nameof(aString2));

            int lLen1 = aString1.Length;
            int lLen2 = aString2.Length;
            if (lLen1 == 0)
                return lLen2 == 0 ? 1.0 : 0.0;

            int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

            // default initialized to false
            bool[] lMatched1 = new bool[lLen1];
            bool[] lMatched2 = new bool[lLen2];

            int lNumCommon = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                int lStart = Math.Max(0, i - lSearchRange);
                int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                for (int j = lStart; j < lEnd; ++j)
                {
                    if (lMatched2[j]) continue;
                    if (aString1[i] != aString2[j])
                        continue;
                    lMatched1[i] = true;
                    lMatched2[j] = true;
                    ++lNumCommon;
                    break;
                }
            }
            if (lNumCommon == 0) return 0.0;

            int lNumHalfTransposed = 0;
            int k = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                if (!lMatched1[i]) continue;
                while (!lMatched2[k]) ++k;
                if (aString1[i] != aString2[k])
                    ++lNumHalfTransposed;
                ++k;
            }
            // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
            int lNumTransposed = lNumHalfTransposed / 2;

            // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
            double lNumCommonD = lNumCommon;
            double lWeight = (lNumCommonD / lLen1
                             + lNumCommonD / lLen2
                             + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

            if (lWeight <= mWeightThreshold) return lWeight;
            int lMax = Math.Min(mNumChars, Math.Min(aString1.Length, aString2.Length));
            int lPos = 0;
            while (lPos < lMax && aString1[lPos] == aString2[lPos])
                ++lPos;
            if (lPos == 0) return lWeight;
            return lWeight + 0.1 * lPos * (1.0 - lWeight);

        }

        #endregion

        #region Metaphone

        const string VOWELS = "AEIOU";
        const string FRONTV = "EIY";   /* special cases for letters in FRONT of these */
        const string VARSON = "CSPTG"; /* variable sound--those modified by adding an "h"    */
        const string DOUBLE = ".";     /* let these double letters through */

        const char NULLCHAR = '\0';

        private static int strchr(string s, char c)
        {
            if (s.IndexOf(c) < 0)
                return NULLCHAR;
            else
                return 1; //dummy value to indicate found because we don't use non NULL return
        }

        private static void strncat(ref string s, char c)
        {
            s += new string(c, 1);
        }

        private static int strlen(char[] s)
        {
            var i = 0;
            foreach (var c in s)
            {
                if (c == NULLCHAR)
                    return i;
                i++;
            }
            return -1;
        }

        private static void ShiftLeftByOne(char[] chars, int firstDestIndex)
        {
            for (var i = firstDestIndex; i < chars.Length - 1; i++)
                chars[i] = chars[i + 1];
        }

        private static bool StartsWith(char[] chars, char c1, char c2)
        {
            return chars[0] == c1 && chars[1] == c2;
        }

        /// <summary>Computes the metaphone version of a string</summary>
        /// <param name="name">the string to convert</param>
        /// <returns>a <c>String</c></returns>
        public static string Metaphone(string name)//, int maxLen)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            string metaph = "";
            int ii, jj, Lng, lastChr;
            bool silent, hard;

            char curLtr, prevLtr, nextLtr, nextLtr2, nextLtr3;

            bool vowelAfter, vowelBefore, frontvAfter;

            char[] wname = new char[60];
            char[] ename = wname;

            jj = 0;
            for (ii = 0; ii < name.Length; ii++)
            {
                if (char.IsLetter(name[ii]))
                {
                    ename[jj] = char.ToUpper(name[ii]);
                    jj++;
                }
            }
            ename[jj] = NULLCHAR;

            if (strlen(ename) == 0)
                return null;

            /* if ae, gn, kn, pn, wr then drop the first letter */
            //char *chrptr, *chrptr1;
            //if ((chrptr = strchr2(excpPAIR, ename[0])) != NULLCHAR)
            //{
            //    chrptr1 = nextLTR + (chrptr - excpPAIR);
            //    if (*chrptr1 == ename[1])
            if (
                StartsWith(ename, 'A', 'E') ||
                StartsWith(ename, 'G', 'N') ||
                StartsWith(ename, 'K', 'N') ||
                StartsWith(ename, 'P', 'N') ||
                StartsWith(ename, 'W', 'R')
                )
                ShiftLeftByOne(ename, 0);

            /* change x to s */
            if (ename[0] == 'X')
                ename[0] = 'S';

            /* get rid of the "h" in "wh" */
            //if (strncmp(ename, "WH", 2) == 0)
            if (StartsWith(ename, 'W', 'H'))
                ShiftLeftByOne(ename, 1);

            Lng = strlen(ename);
            lastChr = Lng - 1;   /* index to last character in string makes code easier*/

            /* Remove an S from the end of the string */
            if (ename[lastChr] == 'S')
            {
                ename[lastChr] = NULLCHAR;
                Lng = strlen(ename);
                lastChr = Lng - 1;
            }

            for (ii = 0; ii < Lng; ii++)
            {
                curLtr = ename[ii];

                vowelBefore = false;
                prevLtr = ' ';
                if (ii > 0)
                {
                    prevLtr = ename[ii - 1];
                    if (strchr(VOWELS, prevLtr) != NULLCHAR)
                        vowelBefore = true;
                }
                /* if first letter is a vowel KEEP it */
                if (ii == 0 && (strchr(VOWELS, curLtr) != NULLCHAR))
                {
                    strncat(ref metaph, curLtr);
                    continue;
                }

                vowelAfter = false;
                frontvAfter = false;
                nextLtr = ' ';
                if (ii < lastChr)
                {
                    nextLtr = ename[ii + 1];
                    if (strchr(VOWELS, nextLtr) != NULLCHAR)
                        vowelAfter = true;
                    if (strchr(FRONTV, nextLtr) != NULLCHAR)
                        frontvAfter = true;
                }
                /* skip double letters except ones in list */
                if (curLtr == nextLtr && (strchr(DOUBLE, nextLtr) == NULLCHAR))
                    continue;

                nextLtr2 = ' ';
                if (ii < (lastChr - 1))
                    nextLtr2 = ename[ii + 2];

                nextLtr3 = ' ';
                if (ii < (lastChr - 2))
                    nextLtr3 = ename[ii + 3];

                switch (curLtr)
                {
                    case 'B':
                        silent = false;
                        if (ii == lastChr && prevLtr == 'M')
                            silent = true;
                        if (!silent)
                            strncat(ref metaph, curLtr);
                        break;

                    /*silent -sci-,-sce-,-scy-;  sci-, etc OK*/
                    case 'C':
                        if (!(ii > 1 && prevLtr == 'S' && frontvAfter))
                            if (ii > 0 && nextLtr == 'I' && nextLtr2 == 'A')
                                metaph += "X";
                            else
                                if (frontvAfter)
                                metaph += "S";
                            else
                                    if (ii > 1 && prevLtr == 'S' && nextLtr == 'H')
                                metaph += "K";
                            else
                                if (nextLtr == 'H')
                                if (ii == 0 && (strchr(VOWELS, nextLtr2) == NULLCHAR))
                                    metaph += "K";
                                else
                                    metaph += "X";
                            else
                                metaph += (prevLtr == 'C') ? "C" : "K";
                        break;

                    case 'D':
                        metaph += (nextLtr == 'G' && (strchr(FRONTV, nextLtr2) != NULLCHAR)) ? "J" : "T";
                        break;

                    case 'G':
                        silent = false;
                        /* SILENT -gh- except for -gh and no vowel after h */
                        if ((ii < (lastChr - 1) && nextLtr == 'H')
                            && (strchr(VOWELS, nextLtr2) == NULLCHAR))
                            silent = true;

                        if ((ii == (lastChr - 3))
                            && nextLtr == 'N' && nextLtr2 == 'E' && nextLtr3 == 'D')
                            silent = true;
                        else
                            if ((ii == (lastChr - 1)) && nextLtr == 'N')
                            silent = true;

                        if (prevLtr == 'D' && frontvAfter)
                            silent = true;

                        hard = prevLtr == 'G';

                        if (!silent)
                            metaph += (frontvAfter && (!hard)) ? "J" : "K";
                        break;

                    case 'H':
                        silent = false;
                        if (strchr(VARSON, prevLtr) != NULLCHAR)
                            silent = true;

                        if (vowelBefore && !vowelAfter)
                            silent = true;

                        if (!silent)
                            strncat(ref metaph, curLtr);
                        break;

                    case 'F':
                    case 'J':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'R':
                        strncat(ref metaph, curLtr);
                        break;

                    case 'K':
                        if (prevLtr != 'C')
                            strncat(ref metaph, curLtr);
                        break;

                    case 'P':
                        if (nextLtr == 'H')
                            metaph += "F";
                        else
                            metaph += "P";
                        break;

                    case 'Q':
                        metaph += "K";
                        break;

                    case 'S':
                        if (ii > 1 && nextLtr == 'I'
                            && (nextLtr2 == 'O' || nextLtr2 == 'A'))
                            metaph += "X";
                        else
                            if (nextLtr == 'H')
                            metaph += "X";
                        else
                            metaph += "S";
                        break;

                    case 'T':
                        if (ii > 1 && nextLtr == 'I'
                            && (nextLtr2 == 'O' || nextLtr2 == 'A'))
                            metaph += "X";
                        else
                            if (nextLtr == 'H')         /* The=0, Tho=T, Withrow=0 */
                            if (ii > 0 || (strchr(VOWELS, nextLtr2) != NULLCHAR))
                                metaph += "0";
                            else
                                metaph += "T";
                        else
                                if (!(ii < (lastChr - 2) && nextLtr == 'C' && nextLtr2 == 'H'))
                            metaph += "T";
                        break;

                    case 'V':
                        metaph += "F";
                        break;

                    case 'W':
                    case 'Y':
                        if (ii < lastChr && vowelAfter)
                            strncat(ref metaph, curLtr);
                        break;

                    case 'X':
                        metaph += "KS";
                        break;

                    case 'Z':
                        metaph += "S";
                        break;
                }
            }

            return metaph;
        }

        #endregion
    }
}

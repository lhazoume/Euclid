using System.Linq;

namespace Euclid.Text
{
    public static class SentenceParsing
    {
        public static int CommonWords(string sentenceA, string sentenceB)
        {
            return sentenceA.Split(' ').Intersect(sentenceB.Split(' ')).Count();
        }
    }
}

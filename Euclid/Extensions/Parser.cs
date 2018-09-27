using System;
using System.Reflection;

namespace Euclid.Extensions
{
    /// <summary>Parser helper class
    /// </summary>
    public static class Parser
    {
        /// <summary>Parses any data generic type when possible</summary>
        /// <typeparam name="T">the generic type</typeparam>
        /// <param name="text"></param>
        /// <returns>a T</returns>
        public static T Parse<T>(this string text)
        {
            Type t = typeof(T);
            MethodInfo m = t.GetMethod("Parse", new Type[] { typeof(string) });

            if (m != null)
                return (T)m.Invoke(null, new object[] { text });
            else
                return (T)Convert.ChangeType(text, t);
        }
    }
}

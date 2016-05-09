using System;
using System.Reflection;

namespace Euclid.Helpers
{
    public static class Parser
    {
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

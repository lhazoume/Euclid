using System;
using System.Linq;
using System.Reflection;

namespace Euclid.Analytics.NeuralNetworks.ActivationFunctions
{
    /// <summary>
    /// ActivationFunction helper
    /// </summary>
    public static class ActivationFunctionHelper
    {
        /// <summary>
        /// Builds an IActivationFunction object from its class name
        /// </summary>
        /// <param name="name">the name of the IActivationFunction class</param>
        /// <returns>an <c>IActivationFunction</c></returns>
        public static IActivationFunction ActivationFunction(this string name)
        {
            Type targetType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).First(t => t.Name == name);
            IActivationFunction result = (IActivationFunction)Activator.CreateInstance(targetType);
            return result;
        }

        /// <summary>Returns a list of all the classes assignable from IActivationList</summary>
        public static string[] ActivationFunctionsList
        {
            get
            {
                Type type = typeof(IActivationFunction);
                return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => p.IsClass && type.IsAssignableFrom(p)).Select(t => t.Name).ToArray();
            }
        }
    }
}

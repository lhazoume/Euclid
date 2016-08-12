using System;
using System.Linq;
using System.Reflection;

namespace Euclid.Analytics.NeuralNetworks
{
    public static class ActivationFunctionHelper
    {
        public static IActivationFunction ActivationFunction(this string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Type targetType = asm.GetTypes().First(t => t.Name == name);
            IActivationFunction result = (IActivationFunction)Activator.CreateInstance(targetType);
            return result;
        }
    }
}

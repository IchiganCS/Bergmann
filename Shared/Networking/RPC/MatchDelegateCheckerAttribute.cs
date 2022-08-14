using System.Reflection;

namespace Bergmann.Shared.Networking.RPC;

[AttributeUsage(AttributeTargets.Class)]
/// <summary>
/// Checks the class for all methods who declare a <see cref="MatchDelegateAttribute"/> and checks whether there has been an error.
/// </summary>
public class MatchDelegateCheckerAttribute : Attribute {
    /// <summary>
    /// Constructs the attribute.
    /// </summary>
    /// <param name="classType">The type of the class to match to.</param>
    /// <param name="throwOnError">Whether the function should throw when an error occurs, e.g. the methods are not correctly defined.</param>
    /// <exception cref="Exception">If any method does not match the specified attribute and the boolean is set.</exception>
    public MatchDelegateCheckerAttribute(Type classType, bool throwOnError = true) {        
        foreach(MethodInfo mi in classType.GetMethods()) {
            foreach (MatchDelegateAttribute attr in mi.GetCustomAttributes<MatchDelegateAttribute>()) {
                if (mi.GetParameters().Select(x => x.ParameterType).SequenceEqual(attr.Invoke.GetParameters().Select(x => x.ParameterType)) &&
                    mi.ReturnType == attr.Invoke.ReturnType)
                    continue;

                string message = $"Paramters of method {mi.Name} does not match Delegate {attr.Invoke.DeclaringType!.Name}";
                Logger.Error(message);
                if (throwOnError)
                    throw new Exception(message);
            }
        }
    }
}
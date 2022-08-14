using System.Reflection;

namespace Bergmann.Shared.Networking.RPC;

[AttributeUsage(AttributeTargets.Method)]
/// <summary>
/// Informs the user when the method to which this attribute is applied does not match a given delegate.
/// Make sure to apply <see cref="MatchDelegateCheckerAttribute"/> to the owning class.
/// </summary>
public class MatchDelegateAttribute : Attribute {
    public MethodInfo Invoke { get; set; }

    public MatchDelegateAttribute(Type del) {
        if (del.BaseType != typeof(MulticastDelegate))
            Logger.Error("Given argument is not a delegate");

        Invoke = del.GetMethod("Invoke")!;
    }
}
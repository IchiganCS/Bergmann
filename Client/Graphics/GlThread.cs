using System.Collections.Concurrent;
using Bergmann.Shared;

namespace Bergmann.Client.Graphics;

/// <summary>
/// Enables all components to execute code on the gl thread. It might change a lot over time the best performance.
/// Maybe 
/// </summary>
public static class GlThread {
    
    /// <summary>
    /// Holds a queue of all actions yet to be executed.
    /// </summary>
    private static ConcurrentQueue<Action> Queue { get; set; } = new();

    /// <summary>
    /// Guarantees that action is executed in foreseeable future on the gl thread.
    /// </summary>
    /// <param name="action">The action which is executed without any parameters sometime in the gl thread</param>
    public static void Invoke(Action action)
        => Queue.Enqueue(action);

    /// <summary>
    /// Executes all actions. Must be on the gl thread or some functionality in random components might not work.
    /// </summary>
    public static void DoAll() {
        while (Queue.TryDequeue(out Action? act))
            act?.Invoke();
    }
}
using System.Collections.Concurrent;

namespace Bergmann.Client.Graphics;

public static class GlThread {
    private static ConcurrentQueue<Action> Queue { get; set; } = new();

    public static void Invoke(Action action)
        => Queue.Enqueue(action);

    public static void DoAll() {
        while (Queue.TryDequeue(out Action? act))
            act?.Invoke();
    }
}
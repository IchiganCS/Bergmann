using Watch = System.Diagnostics.Stopwatch;

namespace Bergmann.Shared;

public class Stopwatch : IDisposable {
    public string Name { get; set; }

    public Watch Wrapped { get; set; }
    public long MillisecondsThreshold { get; set; }

    public Stopwatch(string name, long millisecondThreshold = 5) {
        Name = name;
        MillisecondsThreshold = millisecondThreshold;
        Wrapped = Watch.StartNew();
    }

    public void Dispose() {
        Wrapped.Stop();
        if (Wrapped.ElapsedMilliseconds >= MillisecondsThreshold)
            Logger.Info($"Clock {Name} took {Wrapped.Elapsed.Milliseconds} ms");
    }
}
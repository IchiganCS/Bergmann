using Watch = System.Diagnostics.Stopwatch;

namespace Bergmann.Shared;

/// <summary>
/// Wrap this stopwatch in a using block. Then, when the block is ending, the item is disposed and a <see cref="Logger.Info"/>
/// is called. Certain functionality may be added when needed. It is a wrapper for a <see cref="System.Diagnostics.Stopwatch"/>
/// </summary>
public sealed class Stopwatch : IDisposable {

    /// <summary>
    /// The name of the watch. It is output in the console for easier recognition.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A watch with the same functionality from the standard library.
    /// </summary>
    private Watch Wrapped { get; set; }

    /// <summary>
    /// The threshold when to output anything. E.g. if the clock meets the requirement, there is no need to log anything unusual.
    /// </summary>
    /// <value></value>
    public long MillisecondsThreshold { get; set; }


    /// <summary>
    /// Constructs a new stopwatch with a given name and threshold.
    /// </summary>
    /// <param name="name">The <see cref="Name"/> of the stopwatch.</param>
    /// <param name="millisecondThreshold">The <see cref="MillisecondsThreshold"/> of the stopwatch. In milliseconds.</param>
    public Stopwatch(string name, long millisecondThreshold = 5) {
        Name = name;
        MillisecondsThreshold = millisecondThreshold;
        Wrapped = Watch.StartNew();
    }

    /// <summary>
    /// Disposes the stopwatch and outputs a line if the clock is running too slowly.
    /// </summary>
    public void Dispose() {
        Wrapped.Stop();
        if (Wrapped.Elapsed.Milliseconds >= MillisecondsThreshold)
            Logger.Info($"{Name} took {Wrapped.Elapsed.Milliseconds} ms", "Stopwatch end");
    }
}
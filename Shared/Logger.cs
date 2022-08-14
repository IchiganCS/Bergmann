using System.Runtime.CompilerServices;

namespace Bergmann.Shared;

/// <summary>
/// A class useful for logging of any kind. For logging in this project, we generally try to keep the console clear non-informative commits.
/// There's no benefit in telling us when a chunk has loaded. Don't use warning sparingly though, those can be very useful.
/// </summary>
public static class Logger {

    /// <summary>
    /// Different levels of warnings. Those will be output at every call.
    /// </summary>
    public enum Level {
        Info, Error, Warning
    }

    /// <summary>
    /// Writes a message with the given parameters using a unified formatting.
    /// </summary>
    /// <param name="text">The text of the info.</param>
    /// <param name="level">The level of the warning. If it is <see cref="Level.Info"/> reconsider if it is necessary.</param>
    /// <param name="type">This is usually the place in the code, maybe the method name.</param>
    public static void Write(string text, Level level, [CallerMemberName] string type = "") {
        Console.WriteLine($"[{level.ToString()} in {type}] {text}");
    }

    /// <summary>
    /// Shorthand for calling <see cref="Write"/> with <see cref="Level.Info"/>
    /// </summary>
    public static void Info(string text, [CallerMemberName] string type = "")
        => Write(text, Level.Info, type);


    /// <summary>
    /// Shorthand for calling <see cref="Write"/> with <see cref="Level.Warning"/>
    /// </summary>
    public static void Warn(string text, [CallerMemberName] string type = "")
        => Write(text, Level.Warning, type);

    /// <summary>
    /// Shorthand for calling <see cref="Write"/> with <see cref="Level.Error"/>
    /// </summary>
    public static void Error(string text, [CallerMemberName] string type = "") {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Write(text, Level.Error, type);
        Console.ResetColor();
    }

}
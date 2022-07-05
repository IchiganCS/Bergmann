using System.Runtime.CompilerServices;

namespace Bergmann.Shared;

public static class Logger {
    public enum Level {
        Info, Error, Warning
    }

    public static void Write(string text, Level level, [CallerMemberName] string type = "") {
        Console.WriteLine($"[{level.ToString()} in {type}] {text}");
    }

    public static void Info(string text, [CallerMemberName] string type = "")
        => Write(text, Level.Info, type);
    public static void Warn(string text, [CallerMemberName] string type = "")
        => Write(text, Level.Warning, type);
    public static void Error(string text, [CallerMemberName] string type = "")
        => Write(text, Level.Error, type);

}
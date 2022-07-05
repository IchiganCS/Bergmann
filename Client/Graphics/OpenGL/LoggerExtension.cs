using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

public static class GlLogger {
    public static bool CallbackEnabled { get; private set; } = false;

    /// <summary>
    /// Reads an OpenGL specific error. If an error exists, it's printed as a Warning. This method does nothing if the callback is enabled.
    /// </summary>
    /// <param name="type">Filled by the compiler</param>
    /// <param name="line">Filled by the compiler</param>
    public static void WriteGLError([CallerMemberName] string type = "", [CallerLineNumber] int line = -1) {
        if (CallbackEnabled)
            return;

        ErrorCode ec = GL.GetError();
        if (ec != ErrorCode.NoError)
            Logger.Write("in line " + line + ": " + ec.ToString(), Logger.Level.Warning, type);
    }

    /// <summary>
    /// Disables the <see cref="WriteGLError"/> method since all errors automatically written.
    /// Only works in 4.3
    /// </summary>
    public static void EnableCallback() {
        CallbackEnabled = true;
        GL.DebugMessageCallback((s, t, id, sev, l, m, u) => {
            string text = Marshal.PtrToStringUTF8(m, l);
            Logger.Level level = sev switch {
                DebugSeverity.DebugSeverityHigh => Logger.Level.Error,
                DebugSeverity.DebugSeverityMedium => Logger.Level.Warning,
                _ => Logger.Level.Info
            };
            Logger.Write(text, level, "OpenGL");
        }, IntPtr.Zero);
    }
}
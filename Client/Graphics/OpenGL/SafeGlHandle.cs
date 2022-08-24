using System.Runtime.InteropServices;

namespace Bergmann.Client.Graphics.OpenGL;

public abstract class SafeGlHandle : SafeHandle {
    protected int HandleValue {
        get => (int)handle;
        set => handle = (IntPtr)value;
    }

    protected SafeGlHandle() : base((IntPtr)(-1), true) {
    }

    public override bool IsInvalid => HandleValue < 0;
}
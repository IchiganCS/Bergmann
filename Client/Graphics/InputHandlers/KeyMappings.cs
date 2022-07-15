using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.InputHandlers;

public static class KeyMappings {
    public static Keys Forward { get; set; } = Keys.W;
    public static Keys Backwards { get; set; } = Keys.S;
    public static Keys Left { get; set; } = Keys.A;
    public static Keys Right { get; set; } = Keys.D;
    public static Keys Up { get; set; } = Keys.Space;
    public static Keys Down { get; set; } = Keys.LeftControl;
}
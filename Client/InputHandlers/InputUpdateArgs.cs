using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = Bergmann.Client.Graphics.Window;

namespace Bergmann.Client.InputHandlers;


/// <summary>
/// A collection of every argument required for updating an <see cref="IInputHandler"/>.
/// </summary>
public class InputUpdateArgs {
    /// <summary>
    /// The current state of the keyboard.
    /// </summary>
    public KeyboardState KeyboardState { get; private set; }

    /// <summary>
    /// The current state of the mouse.
    /// </summary>
    public MouseState MouseState { get; private set; }

    /// <summary>
    /// The delta time; the time since the last update in seconds.
    /// </summary>
    public float DeltaTime { get; private set; }

    /// <summary>
    /// The text input of the window in unicode symbols.
    /// </summary>
    public string TextInput { get; private set; }

    /// <summary>
    /// Constructs a new instance from <see cref="Window.Instance"/> and the given delta time.
    /// This constructor shall only be called once per frame or the logic breaks.
    /// </summary>
    /// <param name="deltaTime">The delta time in seconds.</param>
    public InputUpdateArgs(float deltaTime) {
        if (!CallbackRegistered) {
            Window.Instance.TextInput += (e) => CachedText += e.AsString;
            CallbackRegistered = true;
        }

        KeyboardState = Window.Instance.KeyboardState;
        MouseState = Window.Instance.MouseState;

        DeltaTime = deltaTime;
        TextInput = CachedText;

        CachedText = "";
    }

    /// <summary>
    /// Helper: It stores the text read from the window input. It is passed to every <see cref="IInputHandler"/>
    /// and then cleared in the constructor. It is written to by the <see cref="Window.Instance.TextInput"/> event.
    /// </summary>
    private static string CachedText { get; set; } = "";

    /// <summary>
    /// Stores whether a callback to <see cref="Window.Instance.TextInput"/> has already been registered.
    /// Is just set once.
    /// </summary>
    private static bool CallbackRegistered { get; set; } = false;
}
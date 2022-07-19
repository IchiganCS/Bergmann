using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.InputHandlers;

/// <summary>
/// The model for a text field. It supports editing and a basic cursor.
/// </summary>
public class TextField {

    private int Cursor { get; set; } = 0;
    public string Value { get; set; } = "";


    public void HandleCursor(KeyboardState keyboard) {
        if (keyboard.IsKeyPressed(Keys.Backspace) && Cursor > 0) {
            Value = Value.Remove(Cursor - 1, 1);
            Cursor--;
        }
        if (keyboard.IsKeyPressed(Keys.Delete) && Cursor < Value.Length) {
            Value = Value.Remove(Cursor, 1);
        }
        if (keyboard.IsKeyPressed(Keys.Left) && Cursor > 0) {
            Cursor--;
        }
        if (keyboard.IsKeyPressed(Keys.Right) && Cursor < Value.Length) {
            Cursor++;
        }
        
        OnUpdate?.Invoke();
    }

    public void HandleTextInput(TextInputEventArgs e) {
        Value = Value.Insert(Cursor, e.AsString);
        Cursor += e.AsString.Length;
        OnUpdate?.Invoke();
    }


    public delegate void UpdateDelegate();
    /// <summary>
    /// Is called when the value of the text field has changed
    /// </summary>
    public event UpdateDelegate OnUpdate = default!;
}
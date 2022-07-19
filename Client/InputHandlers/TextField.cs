using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.InputHandlers;

/// <summary>
/// The model for a text field. It supports editing and a basic cursor.
/// It can be hooked up to a <see cref="Renderer.TextRenderer"/>
/// </summary>
public class TextField {

    /// <summary>
    /// The position of the cursor in the text. 0 means before the first character, 1 directly after the first character.
    /// </summary>
    public int Cursor { get; private set; } = 0;
    /// <summary>
    /// The current text.
    /// </summary>
    public string Text { get; private set; } = "";

    /// <summary>
    /// This method moves the cursor and deletes characters if necessary, marks text pastes and copies, but doesn't write plain text.
    /// </summary>
    /// <param name="keyboard">The current state of the keyboard</param>
    public void UpdateState(KeyboardState keyboard) {
        bool control = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);

        if (keyboard.IsKeyPressed(Keys.Backspace) && Cursor > 0) {
            Text = Text.Remove(Cursor - 1, 1);
            Cursor--;
        }
        if (keyboard.IsKeyPressed(Keys.Delete) && Cursor < Text.Length)
            Text = Text.Remove(Cursor, 1);
        if (keyboard.IsKeyPressed(Keys.Left) && Cursor > 0)
            Cursor--;
        if (keyboard.IsKeyPressed(Keys.Right) && Cursor < Text.Length)
            Cursor++;
        if (keyboard.IsKeyPressed(Keys.End))
            Cursor = Text.Length;
        if (keyboard.IsKeyPressed(Keys.Home))
            Cursor = 0;

        //TODO: clipboard, shift selection, maybe mouse?
            

        OnUpdate?.Invoke();
    }

    /// <summary>
    /// Clears the entire text
    /// </summary>
    public void Clear() {
        Text = "";
        Cursor = 0;
        OnUpdate?.Invoke();
    }

    /// <summary>
    /// Inserts text at the position of the cursor. This method is mostly used for subscribing to the text input 
    /// event of the window. It works very well and you may use it, but there seems to be little other functionality.
    /// </summary>
    /// <param name="e">The parameter of the fired event.</param>
    public void Insert(TextInputEventArgs e)
        => Insert(e.AsString);

    /// <summary>
    /// Inserts the text at the position of the cursor.
    /// </summary>
    /// <param name="t">The text to be inserted</param>
    public void Insert(string t) {
        Text = Text.Insert(Cursor, t);
        Cursor += t.Length;
        OnUpdate?.Invoke();
    }


    public delegate void UpdateDelegate();
    /// <summary>
    /// Is called when the value of the text field has changed
    /// </summary>
    public event UpdateDelegate OnUpdate = default!;

}
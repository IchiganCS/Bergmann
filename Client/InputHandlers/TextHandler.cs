using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.InputHandlers;

/// <summary>
/// The model for a text field. It supports editing and a basic cursor.
/// It can be hooked up to a <see cref="Renderer.TextRenderer"/>
/// </summary>
public class TextHandler : IInputHandler {

    /// <summary>
    /// The position of the cursor in the text. 0 means before the first character, 1 directly after the first character.
    /// </summary>
    public int Cursor { get; private set; } = 0;
    /// <summary>
    /// The current text.
    /// </summary>
    public string Text { get; private set; } = "";


    /// <summary>
    /// Actions executed when a specific key is pressed. It is called during <see cref="HandleInput(InputUpdateArgs)"/>
    /// </summary>
    public List<(Keys, Action<KeyboardState>)> SpecialActions { get; set; } = new();


    /// <summary>
    /// This method moves the cursor and deletes characters if necessary, marks text pastes and copies, and writes plain text.
    /// </summary>
    /// <param name="updateArgs">The update to exectue</param>
    public void HandleInput(InputUpdateArgs updateArgs) {
        KeyboardState keyboard = updateArgs.KeyboardState;

        foreach ((Keys, Action<KeyboardState>) pair in SpecialActions) {
            if (keyboard.IsKeyPressed(pair.Item1))
                pair.Item2(keyboard);
        }

        if (updateArgs.TypedText != "") {
            Insert(updateArgs.TypedText);
        }

        bool control = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);

        if (keyboard.IsKeyPressed(Keys.Backspace) && Cursor > 0) {
            Text = Text.Remove(Cursor - 1, 1);
            Cursor--;
        }
        if (keyboard.IsKeyPressed(Keys.Delete) && Cursor < Text.Length) {
            Text = Text.Remove(Cursor, 1);
        }
        if (keyboard.IsKeyPressed(Keys.Left) && Cursor > 0)
            Cursor--;
        if (keyboard.IsKeyPressed(Keys.Right) && Cursor < Text.Length)
            Cursor++;
        if (keyboard.IsKeyPressed(Keys.End))
            Cursor = Text.Length;
        if (keyboard.IsKeyPressed(Keys.Home))
            Cursor = 0;


        OnUpdate?.Invoke();
        //TODO: clipboard, shift selection, ctrl movement, maybe mouse?
    }

    /// <summary>
    /// Sets the entire text and moves the cursor to the end.
    /// </summary>
    public void SetText(string newText) {
        Text = newText;
        Cursor = newText.Length;
        OnUpdate?.Invoke();
    }


    /// <summary>
    /// Inserts a given text at the position of the cursor.
    /// </summary>
    /// <param name="t">The text to be inserted</param>
    public void Insert(string t) {
        Text = Text.Insert(Cursor, t);
        Cursor += t.Length;
        OnUpdate?.Invoke();
    }


    public delegate void TextChangeDelegate();
    /// <summary>
    /// Is called when the value of the text field has changed
    /// </summary>
    public event TextChangeDelegate OnUpdate = default!;
}
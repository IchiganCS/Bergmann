using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.InputHandlers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers.UI;

/// <summary>
/// Renders a text on top of a box. It makes use of <see cref="BoxRenderer.ApplyLayout"/> method, so there's no need to call it.
/// Additionally, when specifying the layout of the box, you only have to specify the y coordinate of 
/// <see cref="BoxRenderer.Dimension"/>. In case of the text renderer being hooked up to a <see cref="TextHandler"/>, 
/// it renders the cursor automatically.
/// </summary>
public class TextRenderer : UIRenderer {

    /// <summary>
    /// Vertices: The vertices for the box. It is filled with objects of <see cref="UIVertex"/>. 
    /// Take a look at it to see the options you have for the layout
    /// of boxes. Since each box can be separated into different textures, this buffer can hold 4 * num_of_cuts.
    /// 
    /// The indices connect those. The vao holds all information to render the text.
    /// 
    /// Although this item is initialized in the constructor, this is done asynchronously on the gl thread.
    /// </summary>
    private VertexArray<UIVertex>? VAO { get; set; }

    /// <summary>
    /// A potentially connected input renderer.
    /// </summary>
    private TextHandler? Connected { get; set; }

    /// <summary>
    /// The text currently displayed
    /// </summary>
    public string Text { get; private set; } = "";

    /// <summary>
    /// The position of the rendered cursor.
    /// </summary>
    public int Cursor { get; private set; } = -1;


    public TextRenderer() {
        GlThread.Invoke(() => VAO = new(
            new(BufferTarget.ArrayBuffer, hint: BufferUsageHint.DynamicDraw),
            new(BufferTarget.ElementArrayBuffer, hint: BufferUsageHint.DynamicDraw)
        ));
    }

    /// <summary>
    /// Renders a given text into the box renderer. The x component of <see cref="BoxRenderer.Dimension"/> is discarded to 
    /// fit the text. Each letter is given a box around it and a layer int the stack. For each letter, we therefore achieve 4 vertices.
    /// Plus 4 if a cursor is supplied.
    /// </summary>
    /// <param name="text">The text to be rendered</param>
    /// <param name="cursor">The position of the cursor to be rendered. If less than zero, then ignored</param>
    public void SetText(string text, int cursor = -1) {
        if (Text == text && Cursor == cursor)
            return;
            
        Text = text;
        Cursor = cursor;

        if (Cursor >= 0)
            text = text.Insert(Cursor, "|");

        float widthOfOne = Dimension.Y * 0.7f;
        float entireWidth = widthOfOne * text.Length;
        Dimension = new(entireWidth, Dimension.Y);


        Vector2 bottomLeftOffset = -RelativeAnchor * Dimension + AbsoluteAnchorOffset;

        UIVertex[] vertices = new UIVertex[text.Length * 4];
        uint[] indices = new uint[text.Length * 6];

        int vertexIndex = 0;
        int indexIndex = 0;

        for (int i = 0; i < text.Length; i++) {
            int layer = GlObjects.RenderableChars.IndexOf(text[i]);

            float coveredSpace = i * widthOfOne;
            if (i == Cursor)
                coveredSpace -= 0.5f * widthOfOne;
            if (i > Cursor && Cursor >= 0)
                coveredSpace -= 1f * widthOfOne;

            indices[indexIndex++] = (uint)vertexIndex + 0;
            indices[indexIndex++] = (uint)vertexIndex + 1;
            indices[indexIndex++] = (uint)vertexIndex + 3;
            indices[indexIndex++] = (uint)vertexIndex + 0;
            indices[indexIndex++] = (uint)vertexIndex + 2;
            indices[indexIndex++] = (uint)vertexIndex + 3;

            vertices[vertexIndex++] = new() {
                Absolute = bottomLeftOffset + new Vector2(coveredSpace, 0),
                Percent = PercentageAnchorOffset,
                TexCoord = new(0, 0, layer)
            };
            vertices[vertexIndex++] = new() {
                Absolute = bottomLeftOffset + new Vector2(widthOfOne + coveredSpace, 0),
                Percent = PercentageAnchorOffset,
                TexCoord = new(1, 0, layer)
            };
            vertices[vertexIndex++] = new() {
                Absolute = bottomLeftOffset + new Vector2(coveredSpace, Dimension.Y),
                Percent = PercentageAnchorOffset,
                TexCoord = new(0, 1, layer)
            };
            vertices[vertexIndex++] = new() {
                Absolute = bottomLeftOffset + new Vector2(widthOfOne + coveredSpace, Dimension.Y),
                Percent = PercentageAnchorOffset,
                TexCoord = new(1, 1, layer)
            };
        }

        GlThread.Invoke(() => {
            VAO?.VertexBuffer.Fill(vertices, true);
            VAO?.IndexBuffer.Fill(indices, true);
        });
    }

    /// <summary>
    /// Hooks the text field to this text renderer. No further action on the text renderer is then required.
    /// </summary>
    /// <param name="tf">The text field whose values are checked on every update.</param>
    public void ConnectToTextInput(TextHandler tf) {
        Connected = tf;
        Connected.OnUpdate += SetTextFromConnected;
        SetTextFromConnected();
    }

    private void SetTextFromConnected()
        => SetText(Connected!.Text, Connected!.Cursor);

    /// <summary>
    /// Renders the underlying box renderer with the specfic text on it. Binds the letter stack to the texture stack slot.
    /// </summary>
    public override void Render() {
        GlObjects.LetterTextures.Bind();
        Program.Active!.SetUniform("useStack", true);

        VAO?.Draw();
    }


    public override void Dispose() {
        if (Connected is not null)
            Connected.OnUpdate -= SetTextFromConnected;

        VAO?.Close();
    }
}
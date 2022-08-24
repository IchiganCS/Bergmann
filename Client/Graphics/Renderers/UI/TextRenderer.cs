using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.InputHandlers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers.UI;

/// <summary>
/// Renders a text on top of a box. It makes use of <see cref="BoxRenderer.BuildVAO"/> method, so there's no need to call it.
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
        if (cursor >= 0) {
            text = text.Insert(cursor, "|");
        }

        float widthOfOne = Dimension.Y * 0.7f;
        float entireWidth = widthOfOne * text.Length;
        Dimension = new(entireWidth, Dimension.Y);


        Vector2 anchorOffset = -RelativeAnchor * Dimension;

        List<UIVertex> vertices = new();
        List<uint> indices = new();

        //the index to use for the next box
        uint indexToUse = 0;

        for (int i = 0; i < text.Length; i++) {
            char ch = text[i];
            int layer = GlObjects.RenderableChars.IndexOf(ch);
            float coveredSpace = i * widthOfOne;
            float spaceThisPass = widthOfOne;
            float cursorOffset = 0f;

            if (i == cursor)
                cursorOffset = -0.5f * widthOfOne;
            if (i > cursor && cursor > 0)
                cursorOffset = -1f * widthOfOne;

            vertices.AddRange(new UIVertex[4] {
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(coveredSpace + cursorOffset, 0),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(0, 0, layer)},
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass + coveredSpace + cursorOffset, 0),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(1, 0, layer)},
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(coveredSpace + cursorOffset, Dimension.Y),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(0, 1, layer)},
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass + coveredSpace + cursorOffset, Dimension.Y),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(1, 1, layer)}});

            indices.AddRange(new uint[6] {
                indexToUse + 0, indexToUse + 1, indexToUse + 3,
                indexToUse + 0, indexToUse + 2, indexToUse + 3
            });
            indexToUse += 4;
        }

        UIVertex[] verticesArray = vertices.ToArray();
        uint[] indicesArray = indices.ToArray();

        GlThread.Invoke(() => {
            VAO?.VertexBuffer.Fill(verticesArray, true);
            VAO?.IndexBuffer.Fill(indicesArray, true);
        });
    }

    /// <summary>
    /// Hooks the text field to this text renderer. No further action on the text renderer is then required.
    /// </summary>
    /// <param name="tf">The text field whose values are checked on every update</param>
    public void ConnectToTextInput(TextHandler tf) {
        Connected = tf;
        Connected.OnUpdate += SetTextFromConnected;
        SetTextFromConnected();
    }

    private void SetTextFromConnected() {
        SetText(Connected!.Text, Connected!.Cursor);
    }

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

        GlThread.Invoke(() => VAO?.Dispose());
    }
}
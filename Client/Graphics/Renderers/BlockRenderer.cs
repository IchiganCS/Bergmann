using System.Text.Json;
using System.Text.Json.Serialization;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// This class does not render a block. Instead, it only holds necessary information, such as the textures.
/// It could also be added to Block.cs but let's keep shared clean of any graphical issues.
/// </summary>
public static class BlockRenderer {
    #pragma warning disable CS8618
    /// <summary>
    /// The texture stack is a 2d array of textures, compiled out of a supplied json file in <see cref="MakeTextureStack"/>.
    /// It can be bound and every block can find its appropriate textures if the layer specified in <see cref="BlockInfo"/> is used as the texture coordiante's
    /// z component.
    /// </summary>
    public static TextureStack TextureStack { get; set; }
    #pragma warning restore CS8618

    public static void MakeTextureStack(string filename) {
        TextureStack = new();


        using JsonDocument doc = JsonDocument.Parse(
            ResourceManager.ReadFile(ResourceManager.Type.Jsons, filename));

        JsonSerializerOptions options = new() {
            Converters = {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
        };

        JsonElement arrayNode = doc.RootElement.GetProperty("Textures");



        int size = doc.RootElement.GetProperty("Size").GetInt32();
        TextureStack.Reserve(size, size, arrayNode.GetArrayLength());

        foreach (JsonElement x in arrayNode.EnumerateArray()) {
            try {
                using Image texture = Image.Load(ResourceManager.FullPath(ResourceManager.Type.Textures, x.GetProperty("Texture").GetString()!));
                using Image<Rgba32> tex = texture.CloneAs<Rgba32>();
                TextureStack.Write(tex, x.GetProperty("Layer").GetInt32());
                GlLogger.WriteGLError();
            } catch (Exception) {
                Logger.Warn($"Couldn't load texture from json file {filename} with {x.ToString()}");
            }
        }
    }


    public static void Dispose() {
        TextureStack.Dispose();
    }
}
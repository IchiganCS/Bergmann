using System.Text.Json;
using System.Text.Json.Serialization;
using Shared;

namespace Bergmann.Shared.World;

public class BlockInfo {
    private static List<BlockInfo> AllInfos { get; set; } = new();

    public static BlockInfo GetFromID(int id) {
        return AllInfos[id];
    }
    public static void ReadFromJson(string filename) {
        AllInfos.Clear();
        AllInfos.Add(new() { Name = "invalid_air" });

        using JsonDocument doc = JsonDocument.Parse(
            ResourceManager.ReadFile(ResourceManager.Type.Jsons, filename));

        JsonSerializerOptions options = new() {
            Converters = {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
        };

        foreach (JsonElement x in doc.RootElement.GetProperty("Blocks").EnumerateArray()) {
            BlockInfo? deserialized = x.Deserialize<BlockInfo>(options);


            if (deserialized is not null)
                AllInfos.Add(deserialized);
            else
                Logger.Warn($"Couldn't deserialize value from json file {filename}");
        }
    }

    public BlockType Type { get; set; }

    public string Name { get; set; } = default!;
    public int ID { get; set; }

    public int TopTexture { get; set; }
    public int BottomTexture { get; set; }
    public int LeftTexture { get; set; }
    public int RightTexture { get; set; }
    public int FrontTexture { get; set; }
    public int BackTexture { get; set; }

    public int GetLayerFromFace(Block.Face face) {
        return face switch {
            Block.Face.Top => TopTexture,
            Block.Face.Left => LeftTexture,
            Block.Face.Right => RightTexture,
            Block.Face.Front => FrontTexture,
            Block.Face.Back => BackTexture,
            _ => BottomTexture
        };
    }


    public enum BlockType {
        Normal, Custom
    }
}
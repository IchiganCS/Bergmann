using System.Text.Json;
using System.Text.Json.Serialization;

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

    public string Name { get; set; } = default!;
    public int ID { get; set; }

    public bool IsTransparent { get; set; } = false;
    public bool IsOpaque => !IsTransparent;

    public int TopLayer { get; set; }
    public int BottomLayer { get; set; }
    public int LeftLayer { get; set; }
    public int RightLayer { get; set; }
    public int FrontLayer { get; set; }
    public int BackLayer { get; set; }

    public int GetLayerFromFace(Block.Face face) {
        return face switch {
            Block.Face.Top => TopLayer,
            Block.Face.Left => LeftLayer,
            Block.Face.Right => RightLayer,
            Block.Face.Front => FrontLayer,
            Block.Face.Back => BackLayer,
            _ => BottomLayer
        };
    }

}
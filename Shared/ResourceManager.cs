using Bergmann.Shared;

namespace Shared;

public static class ResourceManager {

    public enum Type {
        Shaders, Textures, Jsons, Fonts
    }

    private static string? _Root;
    public static string Root {
        get {
            if (_Root is not null)
                return _Root;

            //_Root = Environment.GetEnvironmentVariable("ResourceDirectory") ?? Environment.CurrentDirectory
            if (Directory.Exists(_Root = Path.Combine(Environment.CurrentDirectory, "Shared/Resources/"))) {
                return _Root;
            }
            if (Directory.Exists(_Root = Path.Combine("../../../../Shared/Resources/", _Root))) {
                return _Root;
            }
            if (Directory.Exists(_Root = Path.Combine(Environment.CurrentDirectory, "Resources/"))) {
                return _Root;
            }
            else {
                Logger.Warn("Couldn't find resources directory");
                _Root = ""; //means working directory
                return _Root;
            }
        }
    }


    public static string FullPath(Type type, string name) {
        return Path.Combine(Root, type.ToString(), name);
    }
    public static string ReadFile(Type type, string name) {
        return File.ReadAllText(FullPath(type, name));
    }
}
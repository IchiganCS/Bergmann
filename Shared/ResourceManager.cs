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

            //_Root = Environment.GetEnvironmentVariable("ResourceDirectory") ?? Environment.CurrentDirectory;
            _Root = Path.Combine(Environment.CurrentDirectory, "Resources/");
            if (!Directory.Exists(_Root)) {
                _Root = Path.Combine("../../../../", _Root);
                if (!Directory.Exists(_Root)) {
                    Logger.Warn("Couldn't find resources directory");
                    return "";
                }
            }
            return _Root;
        }
    }

    public static string FullPath(Type type, string name) {
        return Path.Combine(Root, type.ToString(), name);
    }
    public static string ReadFile(Type type, string name) {
        return File.ReadAllText(FullPath(type, name));
    }
}
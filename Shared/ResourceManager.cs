namespace Shared;

public static class ResourceManager {

    public enum Type {
        Shaders, Images, Textures
    }

    private static string? _Root;
    public static string Root {
        get {
            if (_Root is not null)
                return _Root;

            //_Root = Environment.GetEnvironmentVariable("ResourceDirectory") ?? Environment.CurrentDirectory;
            _Root = Path.Combine(Environment.CurrentDirectory, "Resources");
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
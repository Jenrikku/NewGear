using NewGear.Utils;

namespace NewGear.Configuration;

internal static class ConfigManager {
    public static Configuration Values;

    public static void Load(string? filename = null) {
#if DEBUG
        Values = new();
#else
        if(!string.IsNullOrEmpty(filename) && File.Exists(filename))
            Values = YAMLWrapper.Desearialize<Configuration>(filename);
        else
            Values = new();
#endif
    }

    public static void Save(string filename) =>
        YAMLWrapper.Serialize(filename, Values);
}

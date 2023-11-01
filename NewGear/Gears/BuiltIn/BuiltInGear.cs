using NewGear.GearSystem;

namespace NewGear.Gears.BuiltIn;

internal static class BuiltInGear {
    public static IGearManifest Generate() => new BuiltInGearManifest();

    class BuiltInGearManifest : IGearManifest {
        public string Name => "Built-In";
        public string Description => "A set of built-in gears.";
        public string Version => string.Empty;
        public string[] Authors => new string[] { "Jenrikku (JkKU)" };
        public string[]? OriginalSources => null;

        public string[] GearEntries => new string[] {
            nameof(FileTree)
        };
    }
}

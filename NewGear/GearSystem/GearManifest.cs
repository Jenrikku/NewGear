namespace NewGear.GearSystem;

public interface IGearManifest {
    public string Name { get; }
    public string Description { get; }
    public string Version { get; }
    public string[] Authors { get; }
    public string[]? OriginalSources { get; }

    public string[] GearEntries { get; }
}

using NewGear.GearSystem.Interfaces;

namespace NewGear.FileManagement;

public struct FileMetadata {
    public string Name;
    public string? Path;
    public Type AttachedGear;
    public List<(Type, ICompressionInfo)> CompressionHistory;
    public bool Saved;
}

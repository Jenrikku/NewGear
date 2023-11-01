using NewGear.GearSystem.Interfaces;

namespace NewGear.FileManagement;

public static class FileHolder {
    public static List<(IFile File, FileMetadata Metadata)> Files { get; } = new();

    public static IFile? CurrentFile { get; set; }
    public static FileMetadata? CurrentMetadata { get; set; }
    
    public static List<string> RecentFiles { get; } = new();
}

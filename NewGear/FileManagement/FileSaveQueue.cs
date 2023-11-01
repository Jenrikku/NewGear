using NewGear.GearSystem.Interfaces;

namespace NewGear.FileManagement;

public class FileSaveQueue {
    private static readonly List<(IFile, FileMetadata)> files = new();
    public static void Add(IFile file, FileMetadata metadata) =>
        files.Add((file, metadata));
    public static void Add(params (IFile file, FileMetadata metadata)[] fileArray) =>
        files.AddRange(fileArray);
    /// <summary>
    /// Checks for files inside the queue and saves them.
    /// </summary>
    public static void Check() {
        foreach((IFile file, FileMetadata metadata) entry in files) {
            // [!] Ask the user if they want to use compression.
            // Add also an option to skip this dialog depending on the file type.
        }
    }
}

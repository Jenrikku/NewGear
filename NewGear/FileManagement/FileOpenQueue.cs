using NewGear.GearSystem.Interfaces;
using NewGear.GearSystem.GearManagement;
using NewGear.GUI;

namespace NewGear.FileManagement;
public static class FileOpenQueue {
    private static readonly List<string> files = new();

    public static void Add(string filename) {
        if(Directory.Exists(filename)) {  // Only goes down one directory.
            foreach(string file in Directory.GetFiles(filename))
                if(File.Exists(file))
                    files.Add(file);
        } else if(File.Exists(filename))
            files.Add(filename);
    }

    public static void Add(IEnumerable<string> filenames) {
        if(filenames is null)
            return;

        foreach(string filename in filenames)
            Add(filename);
    }

    /// <summary>
    /// Checks for files inside the queue and opens them.
    /// </summary>
    public static void Check() {
#if DEBUG
        foreach(string filename in files) {
            FileHolder.Files.Add((new DebugFile(), new() { Path = filename, Name = Path.GetFileName(filename) }));

            if(FileHolder.CurrentFile is null) {
                FileHolder.CurrentFile = FileHolder.Files[0].File;
                FileHolder.CurrentMetadata = FileHolder.Files[0].Metadata;
            }
        }

#else

        foreach(string filename in files) {
            byte[] data = File.ReadAllBytes(filename);
            IEnumerable<Type> gears = GearDirector.GetCompatibleGears(filename, data);
            int count = gears.Count();
            IFile file;
            switch(count) {
                case 0:
                    // [!] Ask the user to use a Gear even if the file was not recognised.
                    break;
                case 1:
                    // [!] Open file directly.
                    break;
                default:
                    // [!] Ask the user to choose between the multiple compatible gears.
                    break;
            }
            //WindowCreator.CreateFrom(file);
            //FileHolder.RecentFiles.Remove(filename);
            //FileHolder.RecentFiles.Add(filename);
        }
        
#endif

        var (file, metadata) = FileHolder.Files.Last();

        FileHolder.CurrentFile = file;
        FileHolder.CurrentMetadata = metadata;

        EditorHolder.ChangeCurrentEditors(file);

        files.Clear();
    }
}

struct DebugFile : IFile {
    public NewGear.Commons.IO.ByteOrder ByteOrder { get; set; }
} 
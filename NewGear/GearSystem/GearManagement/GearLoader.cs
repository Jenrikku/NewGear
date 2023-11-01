using System.Reflection;

namespace NewGear.GearSystem.GearManagement;

public static class GearLoader {
    public static void LoadAllGears(string path = "gears") {
        if(!Directory.Exists(path))
            return;

        LoadRecursively(path);

        void LoadRecursively(string dir) {
            foreach(string file in Directory.EnumerateFiles(dir))
                LoadGearAssembly(file);

            foreach(string subdir in Directory.EnumerateDirectories(dir))
                LoadRecursively(subdir);
        }
    }

    public static void LoadGearAssembly(string path) {
        Assembly assembly;
        IGearManifest manifest;

        try {
            assembly = Assembly.LoadFrom(path);
        } catch(Exception e) {
            Console.Error.WriteLine($"ERROR: The file {path} could not be loaded as an assembly:\n{e.Message}");
            return;
        }

        Type? manifestType = assembly.GetType("Manifest");

        if(manifestType is null) {
            Console.Error.WriteLine($"ERROR: The file {path} has no proper manifest.");
            return;
        }

        object? instance = Activator.CreateInstance(manifestType);

        if(instance is not IGearManifest gearManifest) {
            Console.Error.WriteLine($"ERROR: The manifest of the file {path} does not implement the proper interface.");
            return;
        }

        manifest = gearManifest;

        GearHolder.StoreGears(Path.GetFileName(path), manifest, assembly);
    }
}

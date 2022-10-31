using System.Reflection;

namespace NewGear.GearSystem.GearLoading {
    public static class GearManager {
        public static List<GearManifest> LoadedManifests { get; } = new();
        public static List<GearEntry> LoadedGears { get; } = new();

        /// <summary>
        /// Loads all the gears inside the "Gear" directory.
        /// </summary>
        public static void LoadGears() {
            // Removes all items if this function was called before.
            LoadedManifests.Clear();
            LoadedGears.Clear();

            foreach(FileInfo file in new DirectoryInfo(AppContext.BaseDirectory + "Gears").GetFiles()) {
                if(file.Extension != ".dll")
                    continue;

                // Loads the assembly from the file, calling all static constructors.
                Assembly assembly = Assembly.LoadFrom(file.FullName);

                // Gets the manifest from the gear.
                Type? manifest = assembly.GetType("Manifest");

                if(manifest is not null) {
                    object? instance = Activator.CreateInstance(manifest);

                    if(instance is not GearManifest loadedManifest)
                        continue;

                    LoadedManifests.Add(loadedManifest);
                    LoadedGears.AddRange(loadedManifest.Entries);

                    Console.WriteLine(" - " + loadedManifest.Name);
                }
            }
        }
    }
}

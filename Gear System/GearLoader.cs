using NewGear.GearSystem.AbstractGears;
using System.Reflection;

namespace NewGear.GearSystem {
    public static class GearLoader {
        public static List<Type> LoadedCompressionGears = new();
        public static List<Type> LoadedDataGears = new();

        /// <summary>
        /// Loads all the gears inside the "Gear" directory.
        /// </summary>
        public static void LoadGears() {
            foreach(FileInfo file in new DirectoryInfo("Gears").GetFiles()) {
                if(file.Extension != ".dll")
                    continue;

                // Loads the assembly from the file, calling all static constructors.
                Assembly assembly = Assembly.LoadFrom(file.FullName);

                // Adds all types to the lists, according to their parent classes.
                foreach(Type type in assembly.ExportedTypes) {
                    if(!type.IsNested) {
                        string loadingString = $"Loading gear {type.Name}...";

                        if(type.Name != Path.GetFileNameWithoutExtension(file.Name))
                            loadingString += $" (from {file.Name})";

                        Console.WriteLine(loadingString);

                        switch(type) {
                            case Type x when x.IsAssignableTo(typeof(DataGear)):
                                LoadedDataGears.Add(type);
                                break;

                            case Type x when x.IsAssignableTo(typeof(CompressionGear)):
                                LoadedCompressionGears.Add(type);
                                break;

                            default:
                                Console.WriteLine($"Warning: {type.FullName} inside {file.Name} could not be loaded as a gear.");
                                break;
                        }
                    }
                }
            }
        }
    }
}

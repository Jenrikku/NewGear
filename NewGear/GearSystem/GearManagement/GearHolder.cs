using NewGear.GearSystem.Interfaces;
using System.Diagnostics;
using System.Reflection;

namespace NewGear.GearSystem.GearManagement {
    public static class GearHolder {
        public static List<(string, IGearManifest)> GearInformation { get; } = new();
        public static List<Type> DataGears { get; } = new();
        public static List<Type> WindowGears { get; } = new();

        internal static void StoreGears(string filename, IGearManifest manifest, Assembly assembly) {
            GearInformation.Add((filename, manifest));

            foreach(string entry in manifest.GearEntries) {
                Type? type = assembly.GetType(entry);

                if(type is null) {
                    Debug.WriteLine("Type not found: " + entry);
                    continue;
                }

                if(type.IsAssignableTo(typeof(IDataGear)))
                    DataGears.Add(type);

                if(type.IsAssignableTo(typeof(IWindowGear)))
                    WindowGears.Add(type);
            }
        }
    }
}

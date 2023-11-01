using NewGear.GearSystem.Interfaces;

namespace NewGear.GearSystem.GearManagement {
    public class GearDirector {
        /// <returns>An enumerator of gear types that can read the given file.</returns>
        public static IEnumerable<Type> GetCompatibleGears(string filename, byte[] data) {
            foreach(Type type in GearHolder.DataGears) {
                object? result = type.GetMethod("Identify")?.Invoke(null, new object[] { filename, data });

                if(result is bool isCompatible && isCompatible == true)
                    yield return type;
            }
        }

        public static bool IsCompressionGear(Type gear) => gear.IsAssignableTo(typeof(ICompressionGear));

        /// <returns>An <see cref="IFile"/> that has been read by the given gear.</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static IFile Read(Type gear, byte[] data) {
            object? result = gear.GetMethod("Read")?.Invoke(null, new object[] { data });

            if(result is IFile file)
                return file;
            else
                throw new InvalidCastException($"Failed to cast a file read by: {gear.FullName}");
        }

        /// <returns>A byte array containing the written data from the given gear.</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static byte[] Write(Type gear, IFile file) {
            object? result = gear.GetMethod("Write")?.Invoke(null, new object[] { file });

            if(result is byte[] data)
                return data;
            else
                throw new InvalidCastException($"Failed to cast data written by: {gear.FullName}");
        }

        /// <returns>A compressed byte array from the given gear.</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static byte[] Compress(Type gear, byte[] data) {
            object? result = gear.GetMethod("Compress")?.Invoke(null, new object[] { data });

            if(result is byte[] compressed)
                return compressed;
            else
                throw new InvalidCastException($"Failed to cast data compressed by: {gear.FullName}");
        }

        /// <returns>A decompressed byte array from the given gear.</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static byte[] Decompress(Type gear, byte[] data) {
            object? result = gear.GetMethod("Decompress")?.Invoke(null, new object[] { data });

            if(result is byte[] decompressed)
                return decompressed;
            else
                throw new InvalidCastException($"Failed to cast data decompressed by: {gear.FullName}");
        }
    }
}

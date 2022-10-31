using System.Text;

namespace NewGear.GearSystem.GearLoading {
    public static class DefaultIdentifyMethods {
        /// <summary>
        /// Identifies a file by its magic.
        /// </summary>
        /// <param name="magicStartIndex">The offset at which the magic is stored.</param>
        public static FileIdentification IdentifyByMagic(string magic, int magicStartIndex = 0) =>
            (string filename, byte[] contents) =>
                Encoding.ASCII.GetString(contents[magicStartIndex..magic.Length]) == magic;

        /// <summary>
        /// Identifies a file by its possible magics.
        /// </summary>
        public static FileIdentification IdentifyByMagic(string[] magics, int[]? magicStartIndeces) =>
            (string filename, byte[] contents) => {
                for(int i = 0; i < magics.Length; i++) {
                    string magic = magics[i];
                    int startIndex = magicStartIndeces is null ? 0 : magicStartIndeces[i];

                    if(Encoding.ASCII.GetString(contents[startIndex..magic.Length]) == magic)
                        return true;
                }

                return false;
            };

        /// <summary>
        /// Identifies a file by its extension.
        /// </summary>
        public static FileIdentification IdentifyByExtension(string extension) =>
            (string filename, byte[] contents) =>
                filename.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Identifies a file by its possible extensions.
        /// </summary>
        public static FileIdentification IdentifyByExtension(params string[] extensions) =>
            (string filename, byte[] contents) => {
                foreach(string extension in extensions)
                    if(filename.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                        return true;

                return false;
            };
    }
}

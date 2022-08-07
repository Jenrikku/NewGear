using System.Text;

namespace NewGear.GearSystem.InterfaceGears {
    public interface IModifiableGear : IReadableGear {
        /// <summary>
        /// Writes the contents of the file to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="leaveOpen">Whether or not to leave the stream opened, it is closed by default.</param>
        public abstract void Write(Stream stream, Encoding encoding, bool leaveOpen = false);

        /// <summary>
        /// Writes the contents of the file to a byte array. 
        /// </summary>
        public byte[] Write() => Write(Encoding.ASCII);

        /// <summary>
        /// Writes the contents of the file to a byte array with a given encoding.
        /// </summary>
        public byte[] Write(Encoding encoding) {
            using MemoryStream stream = new();
            Write(stream, encoding);

            return stream.ToArray();
        }

        /// <summary>
        /// Writes the contents of the file to the drive.
        /// </summary>
        /// <param name="filename">The output path.</param>
        public void Write(string filename) => Write(filename, Encoding.ASCII);

        /// <summary>
        /// Writes the contents of the file to the drive with a given encoding.
        /// </summary>
        /// <param name="filename">The output path.</param>
        public void Write(string filename, Encoding encoding) {
            using FileStream stream = new(filename, FileMode.Create);
            Write(stream, encoding);
        }
    }
}

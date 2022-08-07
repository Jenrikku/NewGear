using System.Text;

namespace NewGear.GearSystem.InterfaceGears {
    public interface IReadableGear : IDataGear {
        /// <summary>
        /// Reads an object from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="leaveOpen">Whether or not to leave the stream opened, it is closed by default.</param>
        public abstract void Read(Stream stream, Encoding encoding, bool leaveOpen = false);

        /// <summary>
        /// Reads an object from a byte array.
        /// </summary>
        public void Read(byte[] data) => Read(new MemoryStream(data), Encoding.ASCII);

        /// <summary>
        /// Reads an object from a byte array with a given encoding.
        /// </summary>
        public void Read(byte[] data, Encoding encoding) => Read(new MemoryStream(data), encoding);

        /// <summary>
        /// Reads an object from a file.
        /// </summary>
        public void Read(string filename) => Read(new FileStream(filename, FileMode.Open), Encoding.ASCII);

        /// <summary>
        /// Reads an object from a file with a given encoding.
        /// </summary>
        public void Read(string filename, Encoding encoding) => Read(new FileStream(filename, FileMode.Open), encoding);
    }
}

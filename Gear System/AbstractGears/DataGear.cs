using Syroot.BinaryData;
using System.Text;

namespace NewGear.GearSystem.AbstractGears {
    public abstract class DataGear : Gear {
        /// <summary>
        /// The compression algorithm used for reading and saving this file.
        /// </summary>
        public CompressionGear? CompressionAlgorithm { get; set; }
        /// <summary>
        /// The ByteOrder or endianess of the file, it depends on the platform it is for.
        /// </summary>
        public ByteOrder ByteOrder { get; set; } = ByteOrder.LittleEndian;

        #region Reading

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

        #endregion

        #region Writing

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

        #endregion
    }
}

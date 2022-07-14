namespace NewGear.GearSystem.AbstractGears {
    public abstract class CompressionGear : Gear {
        /// <summary>
        /// The level of compression used.
        /// </summary>
        public byte? Level { get; set; }
        public byte[]? Reserved;

        #region Compression

        /// <summary>
        /// Compresses the content of a <see cref="Stream"/> and returns it as byte array.
        /// </summary>
        /// <param name="leaveOpen">Whether or not to leave the stream opened, it is closed by default.</param>
        public abstract byte[] Compress(Stream stream, bool leaveOpen = false);

        /// <summary>
        /// Compresses a byte array.
        /// </summary>
        public byte[] Compress(byte[] data) => Compress(new MemoryStream(data));

        /// <summary>
        /// Compresses the contents of the file and returns it as a byte array. The actual file is not modified.
        /// </summary>
        public byte[] Compress(string filename) => Compress(new MemoryStream(File.ReadAllBytes(filename)));

        #endregion

        #region Decompression

        /// <summary>
        /// Decompresses the content of a <see cref="Stream"/> and returns it as byte array.
        /// </summary>
        /// <param name="leaveOpen">Whether or not to leave the stream opened, it is closed by default.</param>
        public abstract byte[] Decompress(Stream stream, bool leaveOpen = false);

        /// <summary>
        /// Decompresses a byte array.
        /// </summary>
        public byte[] Decompress(byte[] data) => Decompress(new MemoryStream(data));

        /// <summary>
        /// Decompresses the contents of the file and returns it as a byte array. The actual file is not modified.
        /// </summary>
        public byte[] Decompress(string filename) => Decompress(new MemoryStream(File.ReadAllBytes(filename)));

        #endregion
    }
}

using Syroot.BinaryData;

namespace NewGear.GearSystem.InterfaceGears {
    public interface IDataGear : IGear {
        /// <summary>
        /// The compression algorithm used for reading and saving this file.
        /// </summary>
        public ICompressionGear? CompressionAlgorithm { get; set; }
        /// <summary>
        /// The ByteOrder or endianess of the file, it depends on the platform it is for.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }
    }
}

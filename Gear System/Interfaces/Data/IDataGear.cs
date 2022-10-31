using NewGear.IO;
using System.Text;

namespace NewGear.GearSystem.Interfaces {
    public interface IDataGear : IGear {
        /// <summary>
        /// The compression algorithm used for reading and saving this file.
        /// </summary>
        public ICompressionGear? CompressionAlgorithm { get; set; }
        /// <summary>
        /// The <see cref="IO.ByteOrder"/> or endianess of the file, it depends on the platform it is for.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }
        /// <summary>
        /// The <see cref="System.Text.Encoding"/> used for reading and writing strings.
        /// </summary>
        public Encoding Encoding { get; set; }
    }
}

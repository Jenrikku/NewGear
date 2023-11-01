namespace NewGear.GearSystem.Interfaces {
    public interface ICompressionGear : IDataGear {
        public static abstract (byte[] Data, ICompressionInfo Info) Decompress(byte[] data);
        public static abstract byte[] Compress(byte[] data, ICompressionInfo info);
    }
}

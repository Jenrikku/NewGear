namespace NewGear.GearSystem.Interfaces {
    public interface IReadableGear : IDataGear {
        public static abstract IFile Read(byte[] data);
    }
}

namespace NewGear.GearSystem.Interfaces {
    public interface IWritableGear : IDataGear {
        public static abstract byte[] Write(IFile file);
    }
}

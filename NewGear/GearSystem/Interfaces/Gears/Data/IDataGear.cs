namespace NewGear.GearSystem.Interfaces;

public interface IDataGear : IGear {
    public static abstract string? DefaultEditor { get; }
    
    public static abstract bool Identify(byte[] data, string filename);
}

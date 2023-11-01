namespace NewGear.GearSystem.Interfaces;

public interface ICustomMenuGear : IDataGear {
    public static abstract ContextMenuItem[]? ContextMenu { get; }
}
namespace NewGear.GearSystem.Interfaces {
    /// <summary>
    /// Special containers are not treated alike: the files within it are abstract and handled by the gear itself.
    /// </summary>
    public interface ISpecialContainerGear : IContainerGear { }
}

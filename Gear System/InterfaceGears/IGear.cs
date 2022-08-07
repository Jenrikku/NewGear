namespace NewGear.GearSystem.InterfaceGears {
    public interface IGear {
        /// <returns>Whether or not the given data matches this gear's specifications.</returns>
        public static abstract bool Identify(byte[] data);
    }
}

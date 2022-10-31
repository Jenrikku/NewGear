using System.Text;

namespace NewGear.GearSystem.Interfaces {
    public interface IModifiableGear : IReadableGear {
        /// <summary>
        /// Writes the contents of the file to a byte array with a given encoding.
        /// </summary>
        public abstract byte[] Write();
    }
}

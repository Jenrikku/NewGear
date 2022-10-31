using System.Text;

namespace NewGear.GearSystem.Interfaces {
    public interface IReadableGear : IDataGear {
        /// <summary>
        /// Reads an object from a byte array.
        /// </summary>
        public abstract void Read(byte[] data);
    }
}

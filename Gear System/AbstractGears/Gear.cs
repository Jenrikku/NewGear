using Syroot.BinaryData;
using System.Text;

namespace NewGear.GearSystem.AbstractGears {
    public abstract class Gear {
        /// <returns>Whether or not the given data matches this gear's specifications.</returns>
        public static bool Identify(byte[] data) { return false; }
    }
}

using NewGear.TrueTree;

namespace NewGear.GearSystem.AbstractGears {
    public abstract class ContainerGear : DataGear {
        /// <summary>
        /// Contains all the files inside the archive. It can be iterated.
        /// </summary>
        public Node RootNode = new("root");
    }
}

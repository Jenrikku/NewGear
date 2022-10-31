using NewGear.Trees.TrueTree;

namespace NewGear.GearSystem.Interfaces {
    public interface IContainerGear : IDataGear {
        /// <summary>
        /// Contains all the files inside the archive. It can be iterated.
        /// </summary>
        public BranchNode RootNode { get; set; }
    }
}

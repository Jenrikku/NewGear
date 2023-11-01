using NewGear.Commons.Trees.TrueTree;

namespace NewGear.GearSystem.Interfaces {
    public interface IArchiveFile : IFile {
        public BranchNode<byte[]> RootNode { get; }
    }
}

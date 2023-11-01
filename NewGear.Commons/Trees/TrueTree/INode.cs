namespace NewGear.Commons.Trees.TrueTree
{
    public interface INode<T>
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns the parent node.
        /// </summary>
        public BranchNode<T>? Parent { get; protected set; }
    }
}

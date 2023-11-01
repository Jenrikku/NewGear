namespace NewGear.Commons.Trees.TrueTree
{
    public class LeafNode<T> : INode<T>
    {
        public LeafNode(string name = "", T? contents = default)
        {
            Name = name;
            Contents = contents;
        }

        public T? Contents { get; set; }

        // Interface implementation:

        public string Name { get; set; }
        public BranchNode<T>? Parent { get; set; }
    }
}

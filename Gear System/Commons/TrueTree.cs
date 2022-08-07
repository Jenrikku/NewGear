using System.Collections;

namespace NewGear.TrueTree {
    public interface INode {
        /// <summary>
        /// The ID of the node.
        /// </summary>
        public dynamic ID { get; set; }
        /// <summary>
        /// Used to store data that does not belong to the node's contents.
        /// </summary>
        public dynamic? Metadata { get; set; }
        /// <summary>
        /// The contents of this node, used to store various data.
        /// </summary>
        public dynamic? Contents { get; set; }
        /// <summary>
        /// Represents a link between the data on two nodes, normally from different trees.
        /// </summary>
        public INode? LinkedNode { get; set; }
        /// <summary>
        /// Returns the parent node.
        /// </summary>
        public BranchNode? Parent { get; internal set; }
    }

    public class BranchNode : INode, IEnumerable<INode> {
        public BranchNode(dynamic id) {
            ID = id;
        }

        /// <summary>
        /// Returns the first occurrence of a child with the same ID.
        /// </summary>
        /// <param name="id">The child's ID.</param>
        /// <returns></returns>
        public INode? this[dynamic id] {
            get {
                foreach(INode node in Children)
                    if(node.ID == id)
                        return node;
                return null;
            }
        }

        /// <summary>
        /// Returns a node based on what its index is.
        /// </summary>
        /// <param name="index">The index of the node.</param>
        /// <returns></returns>
        public INode this[int index] {
            get => Children[index];
        }

        /// <summary>
        /// Checks whether or not this node has children.
        /// </summary>
        public bool HasChildren { get => Children.Count > 0; }

        internal List<INode> Children = new();

        /// <summary>
        /// Adds a node as a child to another one and returns this child node.
        /// </summary>
        public INode AddChild(INode child) {
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        // Interface implementation.
        public IEnumerator<INode> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Children.GetEnumerator();

        public dynamic ID { get; set; }
        public dynamic? Contents { get; set; }
        public dynamic? Metadata { get; set; }
        public INode? LinkedNode { get; set; }
        public BranchNode? Parent { get; set; }
    }

    public class LeafNode : INode {
        public LeafNode(dynamic id) {
            ID = id;
        }

        // Interface implementation.
        public dynamic ID { get; set; }
        public dynamic? Contents { get; set; }
        public dynamic? Metadata { get; set; }
        public INode? LinkedNode { get; set; }
        public BranchNode? Parent { get; set; }
    }
}

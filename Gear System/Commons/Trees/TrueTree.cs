using System.Collections;

namespace NewGear.Trees.TrueTree {
    public interface INode {
        /// <summary>
        /// The ID of the node. (It can be the same as another node)
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
        public BranchNode(dynamic id) => ID = id;

        /// <summary>
        /// Returns the first occurrence of a child with the same ID.
        /// </summary>
        /// <param name="id">The child's ID.</param>
        /// <returns></returns>
        public INode? this[dynamic id] {
            get {
                foreach(INode node in ChildLeaves)
                    if(node.ID == id)
                        return node;

                foreach(INode node in ChildBranches)
                    if(node.ID == id)
                        return node;

                return null;
            }
        }

        /// <summary>
        /// Returns a node based on what its index is.
        /// </summary>
        /// <param name="index">The index of the node.</param>
        public INode this[int index] {
            get {
                int brachCount = ChildBranches.Count;

                if(brachCount <= index)
                    return ChildLeaves[index - brachCount];
                else
                    return ChildBranches[index];
            }
        }

        /// <summary>
        /// Checks whether or not this node has children.
        /// </summary>
        public bool HasChildren { get => ChildLeaves.Count > 0 || ChildBranches.Count > 0; }

        /// <summary>
        /// A list containg all child branches.
        /// </summary>
        public readonly List<BranchNode> ChildBranches = new();

        /// <summary>
        /// A list containg all child leaves.
        /// </summary>
        public readonly List<LeafNode> ChildLeaves = new();

        /// <summary>
        /// Adds a node as a child to another one.
        /// </summary>
        public void AddChild(INode child) {
            if(child is BranchNode branch)
                AddChild(branch);
            else if(child is LeafNode leaf)
                AddChild(leaf);
        }

        /// <summary>
        /// Adds a leaf node as a child to another one.
        /// </summary>
        public void AddChild(LeafNode child) {
            child.Parent = this;
            ChildLeaves.Add(child);
        }

        /// <summary>
        /// Adds a branch node as a child to another one.
        /// </summary>
        public void AddChild(BranchNode child) {
            child.Parent = this;
            ChildBranches.Add(child);
        }

        /// <summary>
        /// Removes a child leaf node from the current branch.
        /// </summary>
        /// <returns>Whether the node was removed successfully.</returns>
        public bool RemoveChild(LeafNode child) => ChildLeaves.Remove(child);

        /// <summary>
        /// Removes a child leaf node from the current branch.
        /// </summary>
        /// <returns>Whether the node was removed successfully.</returns>
        public bool RemoveChild(BranchNode child) => ChildBranches.Remove(child);

        /// <summary>
        /// Removes a child node at an specific index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveChild(int index) {
            int brachCount = ChildBranches.Count;

            if(brachCount <= index)
                ChildLeaves.RemoveAt(index - brachCount);
            else
                ChildBranches.RemoveAt(index);
        }

        /// <summary>
        /// Finds a child by it's relative path.
        /// Example: "firstChild/secondChild"
        /// </summary>
        public T? FindChildByPath<T>(string relativePath) where T : INode {
            string[] entries = relativePath.Split('/');

            BranchNode current = this;
            for(int i = 0; i < entries.Length; i++) {
                string entry = entries[i];

                if(i != entries.Length - 1) {
                    BranchNode? child = current.ChildBranches.Find((BranchNode node) => { return node.ID = entry; });

                    if(child == null)
                        return default;

                    current = child;

                    continue;
                }

                // Last part of the path:
                if(typeof(T) == typeof(BranchNode))
                    return (T?) (INode?) current.ChildBranches.Find((BranchNode node) => { return node.ID = entry; });
                else
                    return (T?) (INode?) current.ChildLeaves.Find((LeafNode node) => { return node.ID = entry; });
            }

            return default;
        }

        /// <summary>
        /// Converts a <see cref="LeafNode"/> into a <see cref="BranchNode"/>.
        /// </summary>
        public static explicit operator BranchNode(LeafNode leaf) {
            return new(leaf.ID) {
                Contents = leaf.Contents,
                Metadata = leaf.Metadata,
                LinkedNode = leaf.LinkedNode,
                Parent = leaf.Parent
            };
        }

        // Interface implementation:

        public IEnumerator<INode> GetEnumerator() {
            foreach(BranchNode node in ChildBranches)
                yield return node;

            foreach(LeafNode node in ChildLeaves)
                yield return node;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public dynamic ID { get; set; }
        public dynamic? Contents { get; set; }
        public dynamic? Metadata { get; set; }
        public INode? LinkedNode { get; set; }
        public BranchNode? Parent { get; set; }
    }

    public class LeafNode : INode {
        public LeafNode(dynamic id) => ID = id;

        // Interface implementation.
        public dynamic ID { get; set; }
        public dynamic? Contents { get; set; }
        public dynamic? Metadata { get; set; }
        public INode? LinkedNode { get; set; }
        public BranchNode? Parent { get; set; }
    }
}

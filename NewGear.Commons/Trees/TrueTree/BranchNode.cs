using System.Collections;

namespace NewGear.Commons.Trees.TrueTree
{
    public class BranchNode<T> : INode<T>, IEnumerable<INode<T>>
    {
        public BranchNode(string name) => Name = name;

        /// <summary>
        /// Returns the first occurrence of a child with the same ID.
        /// </summary>
        /// <param name="name">The child's ID.</param>
        /// <returns></returns>
        public INode<T>? this[string name]
        {
            get
            {
                foreach (LeafNode<T> node in ChildLeaves)
                    if (node.Name == name)
                        return node;

                foreach (BranchNode<T> node in ChildBranches)
                    if (node.Name == name)
                        return node;

                return null;
            }
        }

        /// <summary>
        /// Returns a node based on what its index is.
        /// </summary>
        /// <param name="index">The index of the node.</param>
        public INode<T> this[int index]
        {
            get
            {
                int brachCount = ChildBranches.Count;

                if (brachCount <= index)
                    return ChildLeaves[index - brachCount];
                else
                    return ChildBranches[index];
            }
        }

        /// <summary>
        /// Checks whether or not this node has children.
        /// </summary>
        public bool HasChildren
        {
            get => ChildLeaves.Count > 0 || ChildBranches.Count > 0;
        }

        /// <summary>
        /// A list containg all child branches.
        /// </summary>
        public readonly List<BranchNode<T>> ChildBranches = new();

        /// <summary>
        /// A list containg all child leaves.
        /// </summary>
        public readonly List<LeafNode<T>> ChildLeaves = new();

        /// <summary>
        /// Adds a node as a child to another one.
        /// </summary>
        public void AddChild(INode<T> child)
        {
            if (child is BranchNode<T> branch)
                AddChild(branch);
            else if (child is LeafNode<T> leaf)
                AddChild(leaf);
        }

        /// <summary>
        /// Adds a leaf node as a child to another one.
        /// </summary>
        public void AddChild(LeafNode<T> child)
        {
            child.Parent = this;
            ChildLeaves.Add(child);
        }

        /// <summary>
        /// Adds a branch node as a child to another one.
        /// </summary>
        public void AddChild(BranchNode<T> child)
        {
            child.Parent = this;
            ChildBranches.Add(child);
        }

        /// <summary>
        /// Removes a child leaf node from the current branch.
        /// </summary>
        /// <returns>Whether the node was removed successfully.</returns>
        public bool RemoveChild(LeafNode<T> child) => ChildLeaves.Remove(child);

        /// <summary>
        /// Removes a child leaf node from the current branch.
        /// </summary>
        /// <returns>Whether the node was removed successfully.</returns>
        public bool RemoveChild(BranchNode<T> child) => ChildBranches.Remove(child);

        /// <summary>
        /// Removes a child node at an specific index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveChild(int index)
        {
            int brachCount = ChildBranches.Count;

            if (brachCount <= index)
                ChildLeaves.RemoveAt(index - brachCount);
            else
                ChildBranches.RemoveAt(index);
        }

        /// <summary>
        /// Finds a child by it's relative path.
        /// Example: "firstChild/secondChild"
        /// </summary>
        public NodeType? FindChildByPath<NodeType>(string relativePath)
            where NodeType : INode<T>
        {
            string[] entries = relativePath.Split('/');

            BranchNode<T> current = this;
            for (int i = 0; i < entries.Length; i++)
            {
                string entry = entries[i];

                if (i != entries.Length - 1)
                {
                    BranchNode<T>? child = current.ChildBranches.Find(
                        (BranchNode<T> node) =>
                        {
                            return node.Name == entry;
                        }
                    );

                    if (child == null)
                        return default;

                    current = child;

                    continue;
                }

                // Last part of the path:
                if (typeof(NodeType) == typeof(BranchNode<T>))
                    return (NodeType?)
                        (INode<T>?)
                            current.ChildBranches.Find(
                                (BranchNode<T> node) =>
                                {
                                    return node.Name == entry;
                                }
                            );
                else
                    return (NodeType?)
                        (INode<T>?)
                            current.ChildLeaves.Find(
                                (LeafNode<T> node) =>
                                {
                                    return node.Name == entry;
                                }
                            );
            }

            return default;
        }

        // Interface implementation:

        public IEnumerator<INode<T>> GetEnumerator()
        {
            foreach (BranchNode<T> node in ChildBranches)
                yield return node;

            foreach (LeafNode<T> node in ChildLeaves)
                yield return node;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public string Name { get; set; }
        public BranchNode<T>? Parent { get; set; }
    }
}

using System.Collections;

namespace NewGear.TrueTree {
    public class Node : IEnumerable<Node> {
        /// <summary>
        /// Returns the first occurrence of a child with the same name.
        /// </summary>
        /// <param name="name">The child's name.</param>
        /// <returns></returns>
        public Node? this[string name] {
            get {
                foreach(Node node in Children)
                    if(node.Name == name)
                        return node;
                return null;
            }
        }

        /// <summary>
        /// Returns a node based on what its index is.
        /// </summary>
        /// <param name="index">The index of the node.</param>
        /// <returns></returns>
        public Node this[int index] {
            get => Children[index];
        }

        public Node(string name) {
            Name = name;
        }

        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// This list is intended to be used to store the node's data.
        /// </summary>
        public List<dynamic> Contents { get; set; } = new();
        /// <summary>
        /// Returns the parent node.
        /// </summary>
        public Node? Parent { get; internal set; }
        /// <summary>
        /// Checks whether or not this node has children.
        /// </summary>
        public bool HasChildren { get => Children.Count > 0; }

        internal List<Node> Children = new();

        /// <summary>
        /// Adds a node as a child to another one and returns the child node.
        /// </summary>
        public Node AddChild(Node child) {
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        public IEnumerator<Node> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Children.GetEnumerator();
    }
}

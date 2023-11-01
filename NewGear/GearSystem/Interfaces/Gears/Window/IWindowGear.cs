namespace NewGear.GearSystem.Interfaces {
    public interface IWindowGear {
        public string Name { get; }
        /// <summary>
        /// The path that will define where this window will be shown under the menu bar.
        /// Example: Editors/Example Editor
        /// </summary>
        public string? Path { get; }

        public IFile? AttachedFile { get; set; }

        public void RenderContents();
    }
}

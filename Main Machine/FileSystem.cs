using NewGear.GearSystem.GearLoading;
using NewGear.GearSystem.Interfaces;
using NewGear.MainMachine.GUI;
using NewGear.Trees.TrueTree;
using System.Collections.ObjectModel;

namespace NewGear.MainMachine.FileSystem {
    internal static class FileManager {
        /// <summary>
        /// Used to switch between currently loaded files.
        /// </summary>
        public static ObservableCollection<FileInstance> LoadedFiles { get; } = new();

        private static FileInstance? _currentFile;
        /// <summary>
        /// Specifies the file that it is being worked on.
        /// </summary>
        public static FileInstance? CurrentFile {            
            get { return _currentFile; }
            set { 
                _currentFile = value;
                CurrentFileChanged?.Invoke();
            }
        }

        /// <summary>
        /// An event that is triggered when the current file is changed.
        /// </summary>
        public static event Action? CurrentFileChanged;

        /// <summary>
        /// Used to load new files. It is called from multiple methods.
        /// </summary>
        public static void OpenFiles(string[] filenames) {
            foreach(string filename in filenames) {
                byte[] buffer = Array.Empty<byte>();

                try {
                    buffer = File.ReadAllBytes(filename);
                } catch {
                    DialogSystem.OpenMessageDialog(
                        "Error!",
                        $"The file \"{filename}\" could not be opened.");
                    continue;
                }

                IDataGear? gear = GetGear(filename, buffer);

                if(gear is null) { // File not recognized.
                    DialogSystem.OpenMessageDialog(
                        "File not compatible.",
                        $"The file \"{filename}\" cannot be opened by any of the available libraries.");
                    continue;
                }

                if(gear is IContainerGear container) {
                    BranchNode root = ReadFilesRecursively(container.RootNode, Path.GetFileName(filename), container);

                    root.Contents = gear;

                    LoadedFiles.Add(new(root, filename));
                } else
                    LoadedFiles.Add(new(
                        new LeafNode(Path.GetFileName(filename)) {
                            Contents = gear
                        },
                        filename));
            }

            // Switch the current file to the newest:
            if(LoadedFiles.Count > 0)
                CurrentFile = LoadedFiles.Last();

            BranchNode ReadFilesRecursively(BranchNode node, dynamic id, IGear containerGear) {
                BranchNode root = new(id);

                foreach(INode child in node) {
                    if(child is BranchNode branchNode) {
                        BranchNode nextRoot = ReadFilesRecursively(branchNode, branchNode.ID, containerGear);

                        nextRoot.LinkedNode = branchNode;
                        root.AddChild(nextRoot);
                    }

                    if(child is LeafNode leafNode) {
                        if(containerGear is not ISpecialContainerGear) {
                            IDataGear gear = GetGear(child.ID is string ? child.ID : string.Empty, leafNode.Contents);

                            if(gear is IContainerGear container)
                                root.AddChild(ReadFilesRecursively(container.RootNode, child.ID, gear));
                            else
                                root.AddChild(new LeafNode(leafNode.ID) {
                                    LinkedNode = leafNode,
                                    Contents = gear
                                });
                        } else
                            // If the gear is an special container, the contents inside it will not be identified using GetGear():
                            root.AddChild(leafNode);
                    }
                }

                return root;
            }
        }

        /// <summary>
        /// Saves a file to a specified path.
        /// </summary>
        /// <return>Whether the file was saved succesfully.</return>
        public static bool SaveFile(FileInstance file, string? destination = null) {
            if(file.Node.Contents is not IModifiableGear)
                return false;

            destination ??= file.SavePath;

            if(destination is null) // If it is still null: (SavePath is not set)
                return false; // Change to open save dialog.

            File.WriteAllBytes(destination, file.Node.Contents.Write());

            return File.Exists(destination);
        }

        public static IDataGear? GetGear(string filename, byte[] contents) {
            if(contents is null)
                return null;

            foreach(GearEntry entry in GearManager.LoadedGears) {
                if(entry.Identify is not null && !entry.Identify(filename, contents))
                    continue; // Go to the next gear.

                switch(entry.Type) {
                    case Type x when x.IsAssignableTo(typeof(ICompressionGear)):
                        ICompressionGear compression = (ICompressionGear) x.GetConstructors()[0].Invoke(null);
                        IDataGear? gear = GetGear(filename.Remove(filename.LastIndexOf('.')), compression.Decompress(contents));

                        if(gear is null)
                            return null;

                        gear.CompressionAlgorithm = compression;

                        return gear;
                    case Type x when x.IsAssignableTo(typeof(IDataGear)):
                        IReadableGear readableGear = (IReadableGear) x.GetConstructors()[0].Invoke(null);
                        readableGear.Read(contents);

                        return readableGear;
                }
            }

            return null; // If no gear can read this file.
        }

        /// <summary>
        /// Closes a file from a given index from <see cref="LoadedFiles"/>
        /// </summary>
        /// <returns>Whether the file was closed successfully.</returns>
        public static bool CloseFile(FileInstance? file) {
            if(file is null) return false;

            // Asks the user if they want to discard the changes if applicable.
            if(!file.Saved) {
                DialogSystem.OpenMessageDialog(
                    "Unsaved changes:",
                    $"\"{file.Name}\"\n\nYou have unsaved changes in this file.\nDo you want to save them before closing it?",
                    DialogSystem.DialogOptions.YesNoCancel,
                    new Action[] {
                        () => {},
                        () => {},
                        () => {}
                    });

                return false;
            }

            LoadedFiles.Remove(file);

            // Changes the current file if it was closed.
            if(file == CurrentFile)
                if(LoadedFiles.Count > 0)
                    CurrentFile = LoadedFiles.Last();
                else CurrentFile = null;

            return true;
        }
    }

    internal class FileInstance {
        public INode Node;

        private INode? _activeFile;
        /// <summary>
        /// The file that was last selected.
        /// </summary>
        public INode? ActiveFile {
            get { return _activeFile; }
            set {
                _activeFile = value;
                ActiveFileChanged?.Invoke(value);
            }
        }

        public event Action<INode?>? ActiveFileChanged;

        /// <summary>
        /// A list containing selected files (multiselect)
        /// </summary>
        public List<INode> SelectedFiles = new();

        /// <summary>
        /// Used to keep track of whether the file has been saved.
        /// </summary>
        public bool Saved = true;

        private string? _savePath;
        /// <summary>
        /// The full path of the file. Used for saving.
        /// </summary>
        public string? SavePath {
            get { return _savePath; }
            set {
                _savePath = value;
                Name = Path.GetFileName(value);
            }
        }

        public string? Name { get; private set; }

        public FileInstance(INode node, string savePath) {
            Node = node;
            SavePath = savePath;
        }
    }
}

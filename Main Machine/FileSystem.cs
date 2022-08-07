using NewGear.GearSystem;
using NewGear.GearSystem.InterfaceGears;
using NewGear.TrueTree;
using System.Reflection;
using TinyDialogsNet;

namespace NewGear.MainMachine.FileSystem {
    internal delegate void FileSystemEvent();

    internal static class FileManager {
        /// <summary>
        /// An event called each time <see cref="CurrentFile"/> is changed.
        /// </summary>
        public static event FileSystemEvent? FileChanged;

        /// <summary>
        /// Used to switch between the currently loaded files.
        /// </summary>
        public static List<INode> LoadedFiles { get; set; } = new();

        private static INode? _currentFile;
        /// <summary>
        /// Specifies the file that is being shown in the editor.
        /// </summary>
        public static INode? CurrentFile {
            get { return _currentFile; }
            set {
                _currentFile = value;
                FileChanged?.Invoke();
            }
        }

        /// <summary>
        /// Refers to the file that has been selected within a container.
        /// </summary>
        public static INode? SelectedFile { get; set; }

        /// <summary>
        /// Used to load new files. It is called from multiple methods.
        /// </summary>
        public static void OpenFiles(string[] filenames) {
            foreach(string filename in filenames) {
                byte[] buffer = Array.Empty<byte>();

                try {
                    buffer = File.ReadAllBytes(filename);
                } catch {
                    Dialogs.MessageBox(
                        buttons: Dialogs.MessageBoxButtons.Ok,
                        iconType: Dialogs.MessageBoxIconType.Error,
                        defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
                        message: $"The file \"{filename}\" cannot be opened."
                        );
                }

                IDataGear? gear = GetGear(buffer);

                if(gear is null) { // File not recognized.
                    Dialogs.MessageBox(
                        Dialogs.MessageBoxButtons.Ok,
                        Dialogs.MessageBoxIconType.Error,
                        Dialogs.MessageBoxDefaultButton.OkYes,
                        filename,
                        "The file cannot be opened by any of the available libraries.");
                    continue;
                }

                if(gear is IContainerGear container) {
                    BranchNode root = ReadFilesRecursively(container.RootNode, Path.GetFileName(filename));

                    root.Metadata = new LoadedFileProperties(filename);
                    root.Contents = gear;

                    LoadedFiles.Add(root);
                } 
                else
                    LoadedFiles.Add(new LeafNode(Path.GetFileName(filename)) {
                        Metadata = new LoadedFileProperties(filename),
                        Contents = gear
                    });
            }

            // Switch the current file to the newest:
            if(LoadedFiles.Count > 0)
                CurrentFile = LoadedFiles.Last();

            BranchNode ReadFilesRecursively(BranchNode node, dynamic id) {
                BranchNode root = new(id);

                foreach(INode child in node) {
                    if(child is BranchNode branchNode) {
                        BranchNode nextRoot = ReadFilesRecursively(branchNode, branchNode.ID);

                        nextRoot.LinkedNode = branchNode;
                        root.AddChild(nextRoot);
                    }
                    
                    if(child is LeafNode leafNode) {
                        IDataGear gear = GetGear(leafNode.Contents);
                        
                        if(gear is IContainerGear container)
                            root.AddChild(ReadFilesRecursively(container.RootNode, child.ID));
                        else
                            root.AddChild(new LeafNode(leafNode.ID) {
                                LinkedNode = leafNode,
                                Contents = gear
                            });
                    }
                }

                return root;
            }
        }

        public static IDataGear? GetGear(byte[] buffer) {
            ICompressionGear? compressionAlgorithm = null;

            if(buffer is null)
                return null;

            #region Compression

            foreach(Type type in GearLoader.LoadedCompressionGears) {
                MethodInfo? identiyMethod = type.GetMethod("Identify");

                if(identiyMethod is null)
                    continue; // If it cannot be identified, it is skipped.

                bool? success = (bool?) identiyMethod.Invoke(null, new object[] { buffer });

                if(success.HasValue && success.Value) {
                    compressionAlgorithm = (ICompressionGear) type.GetConstructors()[0].Invoke(null);
                    buffer = compressionAlgorithm.Decompress(buffer);
                }
            }

            #endregion

            #region Data

            foreach(Type type in GearLoader.LoadedDataGears) {
                MethodInfo? identiyMethod = type.GetMethod("Identify");

                if(identiyMethod is null)
                    continue; // If it cannot be identified, it is skipped.

                bool? success = (bool?) identiyMethod.Invoke(null, new object[] { buffer });

                if(success.HasValue && success.Value) {
                    IReadableGear gear = (IReadableGear) type.GetConstructors()[0].Invoke(null);
                    gear.CompressionAlgorithm = compressionAlgorithm;

                    gear.Read(buffer);

                    return gear;
                }
            }

            #endregion

            return null; // If no gear can read this file.
        }

        public static bool CloseCurrentFile() {
            if(CurrentFile is null)
                return false;

            return CloseFile(CurrentFile);
        }

        /// <summary>
        /// Closes a file from a given index from <see cref="LoadedFiles"/>
        /// </summary>
        /// <returns>Whether the file was closed successfully.</returns>
        public static bool CloseFile(INode file) {
            // Asks the user if they want to discard the changes if applicable.
            if(!file.Metadata?.Saved) {
                if(Dialogs.MessageBox(
                    Dialogs.MessageBoxButtons.YesNo,
                    Dialogs.MessageBoxIconType.Warning,
                    Dialogs.MessageBoxDefaultButton.CancelNo,
                    "Unsaved changes: " + file.ID,
                    "You have unsaved changes in this file. Do you want to discard them?")
                    != 1) return false;
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

    internal struct LoadedFileProperties {
        /// <summary>
        /// Used to keep track of whether the file has been saved.
        /// </summary>
        public bool Saved { get; set; } = true;
        /// <summary>
        /// The full path of the file. Used for saving.
        /// </summary>
        public string? SavePath { get; set; }

        public LoadedFileProperties(string savePath) =>
            SavePath = savePath;
    }

    //internal class LoadedFile {
    //    /// <summary>
    //    /// Used to keep track of whether the file has been saved.
    //    /// </summary>
    //    public bool Saved { get; set; } = true;
    //    /// <summary>
    //    /// The full path of the file. Used for saving.
    //    /// </summary>
    //    public string FullName { get; set; }
    //    /// <summary>
    //    /// The name of the file.
    //    /// </summary>
    //    public string Name { get; set; }
    //    /// <summary>
    //    /// The gear containing the contents of the file.
    //    /// </summary>
    //    public IDataGear Gear { get; set; }

    //    public LoadedFile(string filename, IDataGear gear) {
    //        FullName = filename;
    //        Name = Path.GetFileName(filename);
    //        Gear = gear;
    //    }
    //}

    //internal class LoadedContainer : LoadedFile {
    //    /// <summary>
    //    /// The root node of the container.
    //    /// </summary>
    //    public BranchNode? RootNode { get; set; }
    //    /// <summary>
    //    /// The list containing all children.
    //    /// </summary>
    //    public List<LoadedChildFile> Children { get; set; } = new();
    //    /// <summary>
    //    /// Represents the selected file within the children list.
    //    /// </summary>
    //    public LoadedChildFile? SelectedFile { get; set; }

    //    public LoadedContainer(string filename, IContainerGear gear) : base(filename, gear) {
    //        RootNode = gear.RootNode;
            
    //        foreach(INode child in RootNode)
    //            Children.Add(new(child.ID, child.Contents, FileManager.GetGear(child.Contents)));
    //    }
    //}

    //internal class LoadedChildFile {
    //    /// <summary>
    //    /// The name of the child.
    //    /// </summary>
    //    public string Name { get; set; }
    //    /// <summary>
    //    /// The bytes of the file.
    //    /// </summary>
    //    public byte[] RawContents { get; set; }
    //    /// <summary>
    //    /// The gear that is generated from <see cref="RawContents"/>.
    //    /// </summary>
    //    public IDataGear? Gear { get; set; }

    //    public LoadedChildFile(string name, byte[] rawContents, IDataGear? gear) {
    //        Name = name;
    //        RawContents = rawContents;
    //        Gear = gear;
    //    }
    //}
}

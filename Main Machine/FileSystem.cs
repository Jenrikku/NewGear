using NewGear.GearSystem;
using NewGear.GearSystem.AbstractGears;
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
        public static List<LoadedFile> LoadedFiles = new();

        private static LoadedFile? _currentFile;
        /// <summary>
        /// Specifies the file that is being shown in the editor.
        /// </summary>
        public static LoadedFile? CurrentFile {
            get { return _currentFile; }
            set {
                _currentFile = value;
                FileChanged?.Invoke();
            }
        }

        /// <summary>
        /// Used to load new files. It is called from multiple methods.
        /// </summary>
        public static void OpenFiles(string[] filenames) {
            foreach(string filename in filenames) {
                byte[] buffer = File.ReadAllBytes(filename);
                CompressionGear? compressionAlgorithm = null;

                #region Compression

                foreach(Type type in GearLoader.LoadedCompressionGears) {
                    MethodInfo? identiyMethod = type.GetMethod("Identify");

                    if(identiyMethod is null)
                        continue; // If it cannot be identified, it is skipped.

                    bool? success = (bool?) identiyMethod.Invoke(null, new object[] { buffer });

                    if(success.HasValue && success.Value) {
                        compressionAlgorithm = (CompressionGear) type.GetConstructors()[0].Invoke(null);
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
                        DataGear gear = (DataGear) type.GetConstructors()[0].Invoke(null);
                        gear.CompressionAlgorithm = compressionAlgorithm;

                        gear.Read(buffer);
                        LoadedFiles.Add(new(filename, gear));
                        goto nextFile;
                    }
                }

                #endregion

                // File not recognized.
                Dialogs.MessageBox(
                    Dialogs.MessageBoxButtons.Ok,
                    Dialogs.MessageBoxIconType.Error,
                    Dialogs.MessageBoxDefaultButton.OkYes,
                    filename,
                    "The file cannot be opened by any of the available libraries.");

                nextFile:
                    continue;
            }

            // Switch the current file to the newest:
            if(LoadedFiles.Count > 0)
                CurrentFile = LoadedFiles.Last();
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
        public static bool CloseFile(LoadedFile file) {
            // Asks the user if they want to discard the changes if applicable.
            if(!file.Saved) {
                if(Dialogs.MessageBox(
                    Dialogs.MessageBoxButtons.YesNo,
                    Dialogs.MessageBoxIconType.Warning,
                    Dialogs.MessageBoxDefaultButton.CancelNo,
                    "Unsaved changes: " + file.Name,
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

    internal class LoadedFile {
        /// <summary>
        /// Used to keep track of whether the file has been saved.
        /// </summary>
        public bool Saved { get; set; } = true;
        /// <summary>
        /// The full path of the file. Used for saving.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The gear containing the contents of the file.
        /// </summary>
        public DataGear Gear { get; set; }
        /// <summary>
        /// Used to specify the file selected inside a container archive if applicable.
        /// </summary>
        public byte[]? CurrentSubFile { get; set; }

        public LoadedFile(string filepath, DataGear gear) {
            FullName = filepath;
            Name = Path.GetFileName(filepath);
            Gear = gear;
        }
    }
}

using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using TinyDialogsNet;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        /// <returns>True if the "Exit" button has been pressed.</returns>
        public static bool RenderMainMenuBar() {
            ImGui.BeginMainMenuBar();

            #region File menu

            if(ImGui.BeginMenu("File")) {
                if(ImGui.MenuItem("Open", "Ctrl+O")) {
                    IEnumerable<string> files = Dialogs.OpenFileDialog(allowMultipleSelects: true);

                    if(!(files is null))
                        FileManager.OpenFiles(files.ToArray());
                }

                if(ImGui.MenuItem("Save", "Ctrl+S")) {
                    if(FileManager.CurrentFile is null)
                        return false;

                    File.WriteAllBytes(FileManager.CurrentFile.FullName, FileManager.CurrentFile.Gear.Write());
                }

                if(ImGui.MenuItem("Save As...", "Ctrl+Shift+S")) {
                    if(FileManager.CurrentFile is null)
                        return false;

                    string result = Dialogs.SaveFileDialog(filter: "*" + Path.GetExtension(FileManager.CurrentFile.FullName));

                    if(!(result is null)) {
                        byte[] buffer = FileManager.CurrentFile.Gear.Write();

                        if(!(FileManager.CurrentFile.Gear.CompressionAlgorithm is null))
                            buffer = FileManager.CurrentFile.Gear.CompressionAlgorithm.Compress(buffer);

                        File.WriteAllBytes(result, buffer);
                        FileManager.CurrentFile.FullName = result;
                        FileManager.CurrentFile.Name = Path.GetFileName(result);
                    }
                }

                if(ImGui.MenuItem("Close File")) {
                    FileManager.CloseCurrentFile();
                }

                ImGui.Separator();

                if(ImGui.MenuItem("Exit")) {
                    return true; // Go to dispose.
                };

                ImGui.EndMenu();
            }

            #endregion

            #region Windows menu

            if(ImGui.BeginMenu("Windows")) {
                ImGui.Checkbox("File Tree", ref OpenedWindows[0]);
                ImGui.Checkbox("Viewport", ref OpenedWindows[1]);
                ImGui.Checkbox("Hex Editor", ref OpenedWindows[2]);
                ImGui.EndMenu();
            }

            #endregion

            #region File selector

            if(!(FileManager.CurrentFile is null)) {
                // Used for keeping track of duplicated names.
                List<string> currentNames = new();

                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 500);
                if(ImGui.BeginCombo("Current file", FileManager.CurrentFile.Name))
                    foreach(LoadedFile file in FileManager.LoadedFiles) {
                        if(currentNames.Contains(file.Name) && !file.Name.EndsWith(')')) { // Prevent duplicates.
                            LoadedFile previousFile = FileManager.LoadedFiles[currentNames.IndexOf(file.Name)];
                            // Set name of previous file with the same name.
                            previousFile.Name += $" ({previousFile.FullName})";

                            // Set name of this file.
                            file.Name += $" ({file.FullName})";
                        }

                        currentNames.Add(file.Name);

                        if(ImGui.Selectable(file.Name)) // If the file has been selected
                            FileManager.CurrentFile = file;
                    }

                ImGui.EndCombo();
            }

            #endregion

            ImGui.EndMainMenuBar();
            return false;
        }
    }
}

using ImGuiNET;
using NewGear.GearSystem.InterfaceGears;
using NewGear.MainMachine.FileSystem;
using NewGear.TrueTree;
using TinyDialogsNet;

using static NewGear.MainMachine.FileSystem.FileManager;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        /// <returns>True if the "Exit" button has been pressed.</returns>
        public static bool RenderMainMenuBar() {
            ImGui.BeginMainMenuBar();

            #region File menu

            if(ImGui.BeginMenu("File")) {
                if(ImGui.MenuItem("Open", "Ctrl+O")) {
                    IEnumerable<string> files = Dialogs.OpenFileDialog(allowMultipleSelects: true);

                    if(files is not null)
                        OpenFiles(files.ToArray());
                }

                if(CurrentFile is null || CurrentFile?.Contents is not IModifiableGear)
                    ImGui.BeginDisabled();

                if(ImGui.MenuItem("Save", "Ctrl+S")) {
                    if(CurrentFile is null)
                        return false;

                    if(CurrentFile.Contents is IModifiableGear gear)
                        try {
                            File.WriteAllBytes(CurrentFile.Metadata?.SavePath, gear.Write());
                        } catch {
                            Dialogs.MessageBox(
                                buttons: Dialogs.MessageBoxButtons.Ok,
                                iconType: Dialogs.MessageBoxIconType.Error,
                                defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
                                message: "The file could not be saved."
                                );
                        }
                }

                if(ImGui.MenuItem("Save As...", "Ctrl+Shift+S")) {
                    if(CurrentFile is null)
                        return false;

                    string result = Dialogs.SaveFileDialog(
                        defaultPath: CurrentFile.Metadata?.SavePath,
                        filter: "*" + Path.GetExtension(CurrentFile.Metadata?.SavePath),
                        filterName: new string(Path.GetExtension(CurrentFile.Metadata?.SavePath))[1..].ToUpperInvariant() + " file");

                    if(result is not null && CurrentFile.Contents is IModifiableGear gear) {
                        byte[] buffer = gear.Write();

                        if(CurrentFile.Contents.CompressionAlgorithm is not null)
                            buffer = CurrentFile.Contents.CompressionAlgorithm.Compress(buffer);
                        
                        try {
                            File.WriteAllBytes(result, buffer);
                        } catch {
                            Dialogs.MessageBox(
                                buttons: Dialogs.MessageBoxButtons.Ok,
                                iconType: Dialogs.MessageBoxIconType.Error,
                                defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
                                message: "The file could not be saved."
                                );
                        }

                        if(CurrentFile.Metadata is not null)
                            CurrentFile.Metadata.SavePath = result;

                        CurrentFile.ID = Path.GetFileName(result);
                    }
                }

                if(ImGui.MenuItem("Close File"))
                    CloseCurrentFile();

                ImGui.EndDisabled(); // Only executed if no file is loaded.

                ImGui.Separator();

                if(ImGui.MenuItem("Exit"))
                    return true; // Go to dispose.

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

            if(CurrentFile is not null) {
                // Used for keeping track of duplicated names.
                List<string> currentNames = new();

                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 500);
                if(ImGui.BeginCombo("Current file", CurrentFile.ID))
                    foreach(INode file in LoadedFiles) {
                        if(currentNames.Contains(file.ID) && !file.ID.EndsWith(')')) { // Prevent duplicates.
                            INode previousFile = LoadedFiles[currentNames.IndexOf(file.ID)];
                            // Set name of previous file with the same name.
                            previousFile.ID += $" ({previousFile.Metadata?.SavePath})";

                            // Set name of this file.
                            file.ID += $" ({file.Metadata?.SavePath})";
                        }

                        currentNames.Add(file.ID);

                        if(ImGui.Selectable(file.ID)) // If the file has been selected
                            CurrentFile = file;
                    }

                ImGui.EndCombo();
            }

            #endregion

            ImGui.EndMainMenuBar();
            return false;
        }
    }
}

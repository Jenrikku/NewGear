using ImGuiNET;
using NewGear.GearSystem.Interfaces;
using NewGear.MainMachine.FileSystem;
using NewGear.MainMachine.GUI.WindowSystem;
using NewGear.Trees.TrueTree;
using System.Reflection;
using TinyDialogsNet;

using static NewGear.MainMachine.FileSystem.FileManager;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        private static bool isTabBarFull = false;

        // debug
        private static bool styleEditorOpened;
        private static bool demoWindowOpened;

        /// <returns>True if the "Exit" button has been pressed.</returns>
        internal static bool RenderMainMenuBar() {
            if(ImGui.BeginMainMenuBar()) {

                #region File menu

                if(ImGui.BeginMenu("File")) {
                    if(ImGui.MenuItem("Open", "Ctrl+O")) {
                        DialogSystem.OpenFileSelectDialog(
                            OpenFiles,
                            DialogSystem.FileSelectMode.OpenMultiple);
                    }

                    if(ImGui.BeginMenu("Open recent")) {
                        ImGui.TextDisabled("WIP");
                        ImGui.EndMenu();
                    }

                    if(CurrentFile is null || CurrentFile?.Node.Contents is not IModifiableGear)
                        ImGui.BeginDisabled();

                    if(ImGui.MenuItem("Save", "Ctrl+S"))
                        if(CurrentFile?.SavePath is null)
                            SaveAs();
                        else
                            SaveFile(CurrentFile);

                        //if(CurrentFile.Node.Contents is IModifiableGear gear)
                        //    try {
                        //        File.WriteAllBytes(CurrentFile.SavePath, gear.Write());
                        //    } catch {
                        //        Dialogs.MessageBox(
                        //            buttons: Dialogs.MessageBoxButtons.Ok,
                        //            iconType: Dialogs.MessageBoxIconType.Error,
                        //            defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
                        //            message: "The file could not be saved."
                        //            );
                        //    }

                    if(ImGui.MenuItem("Save As...", "Ctrl+Shift+S"))
                        SaveAs();

                    void SaveAs() {
                        if(CurrentFile is null)
                            return;

                        DialogSystem.OpenFileSelectDialog(
                            (string[] paths) =>
                                SaveFile(CurrentFile, paths[0]),
                            DialogSystem.FileSelectMode.SaveFile);
                    }

                    //if(CurrentFile is null)
                    //    return false;

                    //string result = Dialogs.SaveFileDialog(
                    //    defaultPath: CurrentFile.Metadata?.SavePath,
                    //    filter: "*" + Path.GetExtension(CurrentFile.Metadata?.SavePath),
                    //    filterName: new string(Path.GetExtension(CurrentFile.Metadata?.SavePath))[1..].ToUpperInvariant() + " file");

                    //if(result is not null && CurrentFile.Contents is IModifiableGear gear) {
                    //    byte[] buffer = gear.Write();

                    //    if(CurrentFile.Contents.CompressionAlgorithm is not null)
                    //        buffer = CurrentFile.Contents.CompressionAlgorithm.Compress(buffer);

                    //    try {
                    //        File.WriteAllBytes(result, buffer);
                    //    } catch {
                    //        Dialogs.MessageBox(
                    //            buttons: Dialogs.MessageBoxButtons.Ok,
                    //            iconType: Dialogs.MessageBoxIconType.Error,
                    //            defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
                    //            message: "The file could not be saved."
                    //            );
                    //    }

                    //    if(CurrentFile.Metadata is not null)
                    //        CurrentFile.Metadata.SavePath = result;

                    //    CurrentFile.ID = Path.GetFileName(result);
                    //}

                    if(ImGui.MenuItem("Close File"))
                        CloseFile(CurrentFile);

                    ImGui.EndDisabled(); // Only used when no file is loaded / it cannot be saved.

                    ImGui.Separator();

                    if(ImGui.MenuItem("Exit"))
                        return true; // Go to dispose.

                    ImGui.EndMenu();
                }

                #endregion

                #region Windows menu

                if(ImGui.BeginMenu("Windows")) {
                    if(ImGui.BeginMenu("Editors", CurrentFile is not null)) {
                        OpenOrSetFocusItem("File Tree", ref WindowManager.WindowOpenedStates[(int) WindowManager.WindowType.FileTree]);
                        OpenOrSetFocusItem("Viewport", ref WindowManager.WindowOpenedStates[(int) WindowManager.WindowType.Viewport]);
                        OpenOrSetFocusItem("Hex View", ref WindowManager.WindowOpenedStates[(int) WindowManager.WindowType.HexView]);
                        ImGui.EndMenu();
                    }

                    if(ImGui.BeginMenu("Others")) {
                        ImGui.EndMenu();
                    }

                    void OpenOrSetFocusItem(string windowTitle, ref bool enabled) {
                        if(ImGui.MenuItem(windowTitle)) {
                            if(enabled)
                                ImGui.SetWindowFocus(windowTitle);
                            else
                                enabled = true;
                        }
                    }

                    ImGui.Separator();

                    if(ImGui.BeginMenu("Saved Layouts", CurrentFile is not null)) {
                        ImGui.TextDisabled("Nothing here");

                        ImGui.Separator();

                        ImGui.MenuItem("New...");

                        ImGui.EndMenu();
                    }

                    ImGui.MenuItem("Restore Layout", CurrentFile is not null);

                    ImGui.EndMenu();
                }

                #endregion

                #region About

                if(ImGui.MenuItem("About"))
                    DialogSystem.OpenMessageDialog(
                        "About NewGear",
                        $"NewGear v{Assembly.GetExecutingAssembly().GetName().Version}",
                        DialogSystem.DialogOptions.None);

                #endregion

                #region Debug
                #if DEBUG

                if(ImGui.BeginMenu("Debug")) {
                    if(ImGui.MenuItem("File selector test"))
                        DialogSystem.OpenFileSelectDialog(null, DialogSystem.FileSelectMode.OpenMultiple, filters: new (string, string)[] { ("Yaz0 compressed", ".szs") });

                    if(ImGui.MenuItem("Show style editor"))
                        styleEditorOpened = !styleEditorOpened;

                    if(ImGui.MenuItem("Show demo window"))
                        demoWindowOpened = !demoWindowOpened;

                    if(ImGui.MenuItem("Show example basic dialog"))
                        DialogSystem.OpenBasicComboBoxDialog("Compress?", "Select a compression method:", new string[] { "None", "Yaz0" }, Console.WriteLine);

                    ImGui.EndMenu();
                }
                
                if(styleEditorOpened) {
                    if(ImGui.Begin("Style", ref styleEditorOpened, ImGuiWindowFlags.NoCollapse)) {
                        ImGui.ShowStyleEditor();
                        ImGui.End();
                    }
                }

                if(demoWindowOpened)
                    ImGui.ShowDemoWindow(ref demoWindowOpened);

                #endif
                #endregion

                #region File bar

                if(CurrentFile is not null) {
                    ImGuiTabBarFlags flags = ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.NoTabListScrollingButtons;

                    if(isTabBarFull)
                        flags ^= ImGuiTabBarFlags.NoTabListScrollingButtons | ImGuiTabBarFlags.TabListPopupButton;

                    if(ImGui.BeginTabBar("FileBar", flags)) {
                        float tabBarWidth = ImGui.GetContentRegionAvail().X - 10;
                        float tabsTotalWidth = 0;

                        // Used to take track of the amount of times a name is repeated.
                        Dictionary<string, int> names = new();

                        for(int i = 0; i < LoadedFiles.Count; i++) {
                            bool opened = true;
                            FileInstance file = LoadedFiles[i];
                            ImGuiTabItemFlags itemFlags = ImGuiTabItemFlags.None;

                            if(!CurrentFile.Saved)
                                itemFlags |= ImGuiTabItemFlags.UnsavedDocument;

                            string name = file.Name ?? "?";
                            int times = 0;

                            if(!names.ContainsKey(name)) {
                                names.Add(name, times);
                            }

                            names.TryGetValue(name, out times);
                            names[name]++;

                            if(times > 0)
                                name += $" ({times})";

                            ImGui.PushID(i);

                            if(ImGui.BeginTabItem(name, ref opened, itemFlags)) {
                                CurrentFile = file;
                                ImGui.EndTabItem();
                            }

                            ImGui.PopID();

                            tabsTotalWidth += ImGui.GetItemRectSize().X;

                            if(!opened) {
                                CloseFile(file);
                                i--;
                            }
                        }

                        isTabBarFull = tabBarWidth < tabsTotalWidth;
                    }

                    //ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 500);
                    //if(ImGui.BeginCombo("Current file", CurrentFile.Name))
                    //    foreach(INode file in LoadedFiles) {
                    //        if(currentNames.Contains(file.ID) && !file.ID.EndsWith(')')) { // Prevent duplicates.
                    //            INode previousFile = LoadedFiles[currentNames.IndexOf(file.ID)];
                    //            // Set name of previous file with the same name.
                    //            previousFile.ID += $" ({previousFile.Metadata?.SavePath})";

                    //            // Set name of this file.
                    //            file.ID += $" ({file.Metadata?.SavePath})";
                    //        }

                    //        currentNames.Add(file.ID);

                    //        if(ImGui.Selectable(file.ID)) // If the file has been selected
                    //            CurrentFile = file;
                    //    }

                    //ImGui.EndCombo();
                }

                #endregion

                ImGui.EndMainMenuBar();
            }

            return false;
        }
    }
}

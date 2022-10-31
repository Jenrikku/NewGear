using ImGuiNET;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;

namespace NewGear.MainMachine.GUI {
    public static partial class DialogSystem {
        private static bool fileSelectOpened = false;
        private static FileSelectDialogData? currentDialogData;

        internal static List<string> RecentFolders = new();

        /// <summary>
        /// Opens a file dialog.
        /// </summary>
        /// <param name="successfulAction">Whathever will happen when the "Open/Save" button is pressed. The argument is the paths.</param>
        public static void OpenFileSelectDialog(
            Action<string[]>? successfulAction,
            FileSelectMode mode,
            string? defaultPath = null,
            (string displayName, string extension)[]? filters = null) {

            currentDialogData = new();

            switch(mode) {
                case FileSelectMode.OpenFile:
                    currentDialogData.Title = "Select a file to open:";
                    break;
                case FileSelectMode.OpenMultiple:
                    currentDialogData.Title = "Select a file or more to open:";
                    break;
                case FileSelectMode.SaveFile:
                    currentDialogData.Title = "Choose where to save the file:";
                    break;
                case FileSelectMode.FolderSelect:
                    currentDialogData.Title = "Select a folder:";
                    break;
            }

            currentDialogData.SuccessfulAction = successfulAction;
            currentDialogData.Mode = mode;
            currentDialogData.DisplayPath = defaultPath ?? (Path.Exists("C:\\") ? "C:\\" : "/");

            if(filters != null)
                foreach((string, string) filter in filters) {
                    currentDialogData.FileFiltersNames.Add(filter.Item1);
                    currentDialogData.FileFiltersExtensions.Add(filter.Item2);
                }

            fileSelectOpened = true;
        }

        public static void RenderFileSelectDialog() {
            if(currentDialogData is null)
                return;

            ImGui.OpenPopup(currentDialogData.Title);

            ImGui.SetNextWindowSize(new(720, 480), ImGuiCond.Appearing);
            ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new(0.5f, 0.5f));
            ImGui.SetNextWindowSizeConstraints(new(300, 300), ImGui.GetMainViewport().Size);

            if(ImGui.BeginPopupModal(currentDialogData.Title, ref fileSelectOpened,
                ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)) {

                // Overwrite drop event:
                MainWindow.FileDroppedAction = (string[] paths) => {
                    string path = paths.Last();

                    if(Path.Exists(path))
                        if((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                            currentDialogData.DisplayPath = path;
                        else {
                            currentDialogData.DisplayPath = Directory.GetParent(path)?.FullName ?? Directory.GetDirectoryRoot(path);

                            currentDialogData.SelectedFiles.Clear();

                            if(currentDialogData.Mode == FileSelectMode.OpenMultiple)
                                currentDialogData.SelectedFiles.AddRange(Raylib.GetDroppedFiles());
                            else
                                currentDialogData.SelectedFiles.Add(path);
                        }
                };

                // Show tooltip if requested:
                if(currentDialogData.TooltipRegion.HasValue) {
                    ImGui.SetTooltip(currentDialogData.Tooltip);

                    // Remove it if the mouse is out of the region.
                    if(!ImGui.IsMouseHoveringRect(
                            currentDialogData.TooltipRegion.Value.Min,
                            currentDialogData.TooltipRegion.Value.Max)) {

                        currentDialogData.TooltipRegion = null;
                    }
                }

                #region TopBar

                ImGui.BeginChild(
                    "##TopBar",
                    new(ImGui.GetContentRegionAvail().X, ImGui.GetFontSize() + ImGui.GetStyle().ItemInnerSpacing.Y * 2),
                    false,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                if(Directory.GetParent(currentDialogData.DisplayPath) is null)
                    ImGui.BeginDisabled();

                if(ImGui.ArrowButton("##Up", ImGuiDir.Up))
                    currentDialogData.DisplayPath = Directory.GetParent(currentDialogData.DisplayPath)?.FullName ?? currentDialogData.DisplayPath;

                ImGui.EndDisabled();

                ImGui.SameLine();

                if(ImGui.Button("Refresh"))
                    currentDialogData.DisplayPath = currentDialogData.DisplayPath;

                ImGui.SameLine();

                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);

                if(ImGui.InputTextWithHint(string.Empty, "Search within the folder.", ref currentDialogData.SearchFilter, byte.MaxValue)) {
                    try {
                        currentDialogData.Regex = new(currentDialogData.SearchFilter, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    } catch {
                        currentDialogData.Regex = null;
                    }
                }

                ImGui.EndChild();

                #endregion

                #region SidePanel

                ImGui.BeginChild(
                    "##SidePanel",
                    new(ImGui.GetContentRegionAvail().X / 3.5f, ImGui.GetContentRegionAvail().Y - ImGui.GetFontSize() * 4),
                    false,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                #region SidePanelRecents

                bool multipleDrives = DriveInfo.GetDrives().Length > 0;

                ImGui.BeginChild(
                    "##SidePanelRecents",
                    new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y / (multipleDrives ? 1.5f : 1)),
                    true);

                ImGui.Text("Recent Folders");
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.BeginChild("##SidePanelRecentsContents");

                if(RecentFolders.Count <= 0) {
                    ImGui.BeginDisabled();
                    ImGui.TextWrapped("There are no recent directories.");
                    ImGui.EndDisabled();
                } else {
                    foreach(string folder in RecentFolders) {
                        if(ImGui.Selectable(Path.GetFileName(folder)))
                            currentDialogData.DisplayPath = folder;
                    }
                }

                ImGui.EndChild();

                ImGui.EndChild();

                #endregion

                #region SidePanelDrives

                ImGui.BeginChild(
                    "##SidePanelDrives",
                    new(0, 0),
                    true);

                ImGui.Text("Drives");
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.BeginChild("##SidePanelDrivesContents");

                foreach(DriveInfo drive in DriveInfo.GetDrives()) {
                    string volumeLabel = drive.VolumeLabel.Length > 0 ? drive.VolumeLabel : "Local Disk";

                    if(ImGui.Selectable($"{volumeLabel} ({drive.Name})")) {
                        currentDialogData.DisplayPath = drive.Name;
                    }
                }

                ImGui.EndChild();

                ImGui.EndChild();

                #endregion

                ImGui.EndChild();

                #endregion

                ImGui.SameLine();

                #region MainView

                ImGui.BeginChild(
                    "##MainView",
                    new(0, ImGui.GetContentRegionAvail().Y - ImGui.GetFontSize() * 4),
                    true);

                if(currentDialogData.ContentsCache.Length <= 0)
                    ImGui.TextDisabled("There is nothing inside this directory.");

                foreach(string entry in currentDialogData.ContentsCache) {
                    string name = Path.GetFileName(entry);
                    FileAttributes attributes = File.GetAttributes(entry);
                    bool matches = false;

                    if(currentDialogData.Regex is null)
                        matches = name.Contains(currentDialogData.SearchFilter, StringComparison.InvariantCultureIgnoreCase);
                    else
                        matches = currentDialogData.Regex.IsMatch(name);

                    if((attributes & FileAttributes.Directory) != FileAttributes.Directory)
                        matches = matches && name.EndsWith(currentDialogData.FileFiltersExtensions[currentDialogData.CurrentFilter]);

                    if(matches)
                        if(ImGui.Selectable(name, currentDialogData.SelectedFiles.Contains(entry), ImGuiSelectableFlags.AllowDoubleClick)) {
                            if((attributes & FileAttributes.Directory) == FileAttributes.Directory) {
                                if(ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                                    currentDialogData.DisplayPath = entry;
                            } else {
                                if(!ImGui.GetIO().KeyCtrl || currentDialogData.Mode != FileSelectMode.OpenMultiple)
                                    currentDialogData.SelectedFiles.Clear();

                                if(!currentDialogData.SelectedFiles.Contains(entry))
                                    currentDialogData.SelectedFiles.Add(entry);
                            }
                        }
                }

                ImGui.EndChild();

                #endregion

                #region BottomBar

                ImGui.BeginChild(
                    "##BottomBar",
                    new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y),
                    false,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                if(ImGui.InputTextWithHint(string.Empty, $"Input the path here.", ref currentDialogData.Path, ushort.MaxValue)) {
                    currentDialogData.Path = currentDialogData.Path.Replace('\"', ' ').Trim();

                    if(Path.Exists(currentDialogData.Path))
                        if((File.GetAttributes(currentDialogData.Path) & FileAttributes.Directory) == FileAttributes.Directory)
                            currentDialogData.DisplayPath = currentDialogData.Path;
                        else {
                            currentDialogData.DisplayPath = Directory.GetParent(currentDialogData.Path)?.FullName ?? Directory.GetDirectoryRoot(currentDialogData.Path);

                            currentDialogData.SelectedFiles.Clear();
                            currentDialogData.SelectedFiles.Add(currentDialogData.Path);
                        }
                }

                ImGui.SameLine();

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                
                ImGui.Combo("##", ref currentDialogData.CurrentFilter, currentDialogData.FileFiltersNames.ToArray(), currentDialogData.FileFiltersNames.Count);

                ImGui.SetCursorPosX(ImGui.GetWindowSize().X - ImGui.GetStyle().WindowPadding.X - 100);

                if(currentDialogData.SelectedFiles.Count <= 0)
                    ImGui.BeginDisabled();

                if(ImGui.Button(currentDialogData.Mode == FileSelectMode.SaveFile ? "Save" : "Open", new(50, 0))
                    || (ImGui.IsKeyPressed(ImGuiKey.Enter) && currentDialogData.SelectedFiles.Count > 0)) {

                    ImGui.CloseCurrentPopup();
                    fileSelectOpened = false;

                    RecentFolders.Add(currentDialogData.DisplayPath);

                    currentDialogData.SuccessfulAction?.Invoke(currentDialogData.SelectedFiles.ToArray());
                }

                ImGui.EndDisabled();

                ImGui.SameLine();

                if(ImGui.Button("Cancel", new(50, 0))) {
                    ImGui.CloseCurrentPopup();
                    fileSelectOpened = false;
                }

                ImGui.EndChild();

                #endregion

                ImGui.EndPopup();
            }

            if(!fileSelectOpened) { // After the dialog was closed.
                MainWindow.RestoreDragAndDrop();
                currentDialogData = null;
            }
        }

        private class FileSelectDialogData {
            public string Title = "##";
            public FileSelectMode Mode = default;

            public string SearchFilter = string.Empty;
            public Regex? Regex;

            public List<string> SelectedFiles = new();
            public string Path = string.Empty;

            public Action<string[]>? SuccessfulAction;

            public int CurrentFilter = 0;
            public List<string> FileFiltersNames = new() { "All files" };
            public List<string> FileFiltersExtensions = new() { string.Empty };

            public string Tooltip = string.Empty;
            public (Vector2 Min, Vector2 Max)? TooltipRegion;

            private string _displayPath = string.Empty;
            public string DisplayPath {
                get { return _displayPath; }
                set {
                    string oldValue = _displayPath;
                    _displayPath = value;

                    try {
                        ContentsCache = Directory.GetFileSystemEntries(DisplayPath);
                    } catch {
                        Tooltip = "You can't access this directory.";
                        TooltipRegion = (ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                        _displayPath = oldValue;
                        return;
                    }

                    SelectedFiles.Clear();

                    if(Directory.GetDirectoryRoot(DisplayPath) == DisplayPath)
                        Path = string.Empty;
                    else Path = DisplayPath;
                }
            }

            public string[] ContentsCache { get; private set; } = Array.Empty<string>();
        }

        public enum FileSelectMode {
            OpenFile,
            OpenMultiple,
            SaveFile,
            FolderSelect
        }
    }
}

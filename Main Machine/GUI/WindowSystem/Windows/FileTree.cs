using ImGuiNET;
using NewGear.GearSystem.AbstractGears;
using NewGear.MainMachine.FileSystem;
using NewGear.TrueTree;
using System.Numerics;
using TinyDialogsNet;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class FileTree : ImGUIWindow {
        private static BranchNode? RootNode;

        public Vector2 WindowPositionMin { get; set; }
        public Vector2 WindowPositionMax { get; set; }

        public FileTree() {
            FileManager.FileChanged += () => {
                // If the file can be displayed as a file tree:
                if(FileManager.CurrentFile?.Gear is ContainerGear)
                    RootNode = ((ContainerGear) FileManager.CurrentFile.Gear).RootNode;
                // If all files are closed:
                if(FileManager.CurrentFile?.Gear is null)
                    RootNode = null;
            };
        }

        public void Render() {
            ImGui.SetNextWindowSize(new(500, 350), ImGuiCond.FirstUseEver);
            ImGui.Begin("File Tree", ref MainWindow.OpenedWindows[0]);
            WindowPositionMin = ImGui.GetWindowPos();
            WindowPositionMax = WindowPositionMin + ImGui.GetWindowSize();

            if(RootNode is null) {
                ImGui.TextDisabled("There is nothing to show.");
                return;
            }

            RenderChildren(RootNode);

            void RenderChildren(BranchNode parent) {
                foreach(INode child in parent) {
                    if(child is BranchNode) {
                        if(ImGui.TreeNodeEx(child.ID)) {
                            RenderChildren((BranchNode) child);
                            ImGui.TreePop();
                        }
                    } else {
                        // When file is selected
                        if(ImGui.Selectable(child.ID))
                            if(!(FileManager.CurrentFile is null))
                                FileManager.CurrentFile.CurrentSubFile = child.Contents;

                        // Context Menu
                        if(ImGui.BeginPopupContextItem()) {
                            if(ImGui.Selectable("Export")) {
                                string? result = Dialogs.SaveFileDialog(
                                    defaultPath: child.ID,
                                    filter: "*" + Path.GetExtension(child.ID),
                                    filterName: new string(Path.GetExtension(child.ID)).Substring(1).ToUpperInvariant() + " file");

                                if(result != null)
                                    File.WriteAllBytes(result, (byte[]) (child.Contents ?? new byte[0]));
                            }

                            if(ImGui.Selectable("Replace")) {
                                string? result = Dialogs.OpenFileDialog()?.First();

                                if(result != null) {
                                    child.Contents = File.ReadAllBytes(result);

                                    if(!(FileManager.CurrentFile is null))
                                        FileManager.CurrentFile.Saved = false;
                                }
                            }

                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }

        public void ContextMenu(Vector2 mousePosition) {
            return;
        }
    }
}

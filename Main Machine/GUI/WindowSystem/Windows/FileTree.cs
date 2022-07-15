using ImGuiNET;
using NewGear.GearSystem.AbstractGears;
using NewGear.MainMachine.FileSystem;
using NewGear.TrueTree;
using System.Numerics;
using TinyDialogsNet;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class FileTree : ImGUIWindow {
        private static Node? RootNode;

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

            void RenderChildren(Node parent) {
                foreach(Node child in parent) {
                    if(child.HasChildren) {
                        if(ImGui.TreeNodeEx(child.Name)) {
                            RenderChildren(child);
                            ImGui.TreePop();
                        }
                    } else {
                        if(ImGui.Selectable(child.Name) && !(FileManager.CurrentFile is null)) {
                            // [!] This is hard-coded and should not be left like this.
                            FileManager.CurrentFile.CurrentSubFile = child.Contents[1];
                        }

                        // Context Menu
                        if(ImGui.BeginPopupContextItem()) {
                            if(ImGui.Selectable("Export")) {
                                string? result = Dialogs.SaveFileDialog(filter: "*" + Path.GetExtension(child.Name));

                                // [!] This is hard-coded and should not be left like this.
                                if(result != null)
                                    File.WriteAllBytes(result, (byte[]) child.Contents[1]);
                            }

                            if(ImGui.Selectable("Replace")) {
                                string? result = Dialogs.OpenFileDialog()?.First();

                                // [!] This is hard-coded and should not be left like this.
                                if(result != null) {
                                    child.Contents[1] = File.ReadAllBytes(result);

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

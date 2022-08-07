using ImGuiNET;
using NewGear.GearSystem.InterfaceGears;
using NewGear.MainMachine.FileSystem;
using NewGear.TrueTree;
using System.Numerics;
using TinyDialogsNet;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class FileTree : ImGUIWindow {
        private static BranchNode? RootNode;

        public FileTree() {
            FileManager.FileChanged += () => {
                // If the file can be displayed as a file tree:
                if(FileManager.CurrentFile is BranchNode branch)
                    RootNode = branch;
                else
                    RootNode = null;
            };
        }

        public void Render() {
            ImGui.SetNextWindowSize(new(500, 350), ImGuiCond.FirstUseEver);
            ImGui.Begin("File Tree", ref MainWindow.OpenedWindows[0]);

            if(RootNode is null) {
                ImGui.TextDisabled("There is nothing to show.");
                return;
            }

            ImGui.Spacing();

            RenderChildren(RootNode);

            static void RenderChildren(BranchNode parent) {
                foreach(INode child in parent) {
                    if(child is BranchNode branchNode) {
                        if(ImGui.TreeNodeEx(child.ID)) { // Branch node.
                            RenderChildren(branchNode);
                            ImGui.TreePop();
                        }
                    } else if(child is LeafNode leafNode) { // Leaf node.
                        ImGui.TreePush();

                        // When file is selected:
                        if(ImGui.Selectable(leafNode.ID))
                            FileManager.SelectedFile = leafNode;

                        // Context Menu
                        if(ImGui.BeginPopupContextItem()) {
                            if(ImGui.Selectable("Export")) {
                                string? result = Dialogs.SaveFileDialog(
                                    title: "Export at...",
                                    defaultPath: leafNode.ID,
                                    filter: "*" + Path.GetExtension(leafNode.ID),
                                    filterName: new string(Path.GetExtension(leafNode.ID))[1..].ToUpperInvariant() + " file");

                                if(result != null)
                                    try {
                                        File.WriteAllBytes(result, (byte[]) (leafNode.Contents ?? Array.Empty<byte>()));
                                    } catch {
                                        Dialogs.MessageBox(
                                            buttons: Dialogs.MessageBoxButtons.Ok,
                                            iconType: Dialogs.MessageBoxIconType.Error,
                                            defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
                                            message: "The file could not be exported."
                                            );
                                    }
                            }

                            if(ImGui.Selectable("Replace")) {
                                string? result = Dialogs.OpenFileDialog("Replace with...")?.First();

                                if(result != null) {
                                    try {
                                        leafNode.Contents = File.ReadAllBytes(result);
                                    } catch {
                                        Dialogs.MessageBox(
                                            buttons: Dialogs.MessageBoxButtons.Ok,
                                            iconType: Dialogs.MessageBoxIconType.Error,
                                            defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
                                            message: "The file could not be replaced."
                                            );
                                    }

                                    if(FileManager.CurrentFile is not null && FileManager.CurrentFile.Metadata is not null)
                                        FileManager.CurrentFile.Metadata.Saved = false;
                                }
                            }

                            ImGui.EndPopup();
                        }

                        ImGui.TreePop();
                    }
                }
            }
        }
    }
}

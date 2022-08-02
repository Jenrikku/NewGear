﻿using ImGuiNET;
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

            static void RenderChildren(BranchNode parent) {
                foreach(INode child in parent) {
                    if(child is BranchNode branchNode) {
                        if(ImGui.TreeNodeEx(child.ID)) {    // Branch node.
                            RenderChildren(branchNode);
                            ImGui.TreePop();
                        }
                    } else if(child is LeafNode leafNode) { // Leaf node.
                        // When file is selected:
                        if(ImGui.Selectable(leafNode.ID))
                            if(FileManager.CurrentFile is not null)
                                FileManager.CurrentFile.CurrentSubFile = leafNode.Contents;

                        // Context Menu
                        if(ImGui.BeginPopupContextItem()) {
                            if(ImGui.Selectable("Export")) {
                                string? result = Dialogs.SaveFileDialog(
                                    defaultPath: leafNode.ID,
                                    filter: "*" + Path.GetExtension(leafNode.ID),
                                    filterName: new string(Path.GetExtension(leafNode.ID))[1..].ToUpperInvariant() + " file");

                                if(result != null)
                                    File.WriteAllBytes(result, (byte[]) (leafNode.Contents ?? Array.Empty<byte>()));
                            }

                            if(ImGui.Selectable("Replace")) {
                                string? result = Dialogs.OpenFileDialog()?.First();

                                if(result != null) {
                                    leafNode.Contents = File.ReadAllBytes(result);

                                    if(FileManager.CurrentFile is not null)
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

using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using NewGear.Trees.TrueTree;
using System.Numerics;
using TinyDialogsNet;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class FileTree : ImGUIWindow {
        public FileInstance LinkedFile { get; set; }
        private readonly BranchNode? root;

        public FileTree(FileInstance file) {
            LinkedFile = file;

            if(file.Node is BranchNode branch)
                root = branch;
            else
                root = null;
        }

        public void Render() {
            ImGui.Begin("File Tree", ref WindowManager.WindowOpenedStates[(int) WindowManager.WindowType.FileTree]);

            if(root is null) {
                ImGui.BeginDisabled();
                ImGui.TextWrapped("There is nothing to show.");
                ImGui.EndDisabled();
                return;
            }

            // Main context menu -----------------

            //if(ImGui.BeginPopupContextItem()) {
            //    if(ImGui.Selectable("Export All")) {
            //        if(FileManager.CurrentFile?.Node.LinkedNode is not BranchNode branch)
            //            return;

            //        string? result = Dialogs.SelectFolderDialog(
            //            title: "Export all at...",
            //            defaultPath: Directory.GetParent(FileManager.CurrentFile?.SavePath));

            //        if(result is not null) {
            //            try {
            //                RecursiveExport(branch, result);
            //            } catch {
            //                Dialogs.MessageBox(
            //                    buttons: Dialogs.MessageBoxButtons.Ok,
            //                    iconType: Dialogs.MessageBoxIconType.Error,
            //                    defaultButton: Dialogs.MessageBoxDefaultButton.OkYes,
            //                    message: "The files were not exported successfully."
            //                    );
            //            }
            //        }

            //        void RecursiveExport(BranchNode branch, string dir) {
            //            Directory.CreateDirectory(dir);

            //            foreach(INode node in branch)
            //                if(node is BranchNode childBranch)
            //                    RecursiveExport(childBranch, Path.Combine(dir, node.ID));
            //                else if(node is LeafNode leaf)
            //                    File.WriteAllBytes(Path.Combine(dir, node.ID), leaf.Contents);
            //        }
            //    }
            //}

            // Render contents -------------------

            //ImGui.InputText("##", )

            RenderChildren(root);

            void RenderChildren(BranchNode parent) {
                foreach(INode child in parent) {
                    if(child is BranchNode branchNode) {
                        if(ImGui.TreeNodeEx(child.ID)) { // Branch node.
                            RenderChildren(branchNode);
                            ImGui.TreePop();
                        }
                    } else if(child is LeafNode leafNode) { // Leaf node.
                        ImGui.TreePush();

                        bool popVar = false;

                        unsafe {
                            if(LinkedFile.ActiveFile == leafNode && LinkedFile.SelectedFiles.Count > 1) {
                                Vector4 currentColor = *ImGui.GetStyleColorVec4(ImGuiCol.Header);

                                currentColor.W += 0.3f;

                                ImGui.PushStyleColor(ImGuiCol.Header, currentColor);

                                popVar = true;
                            }
                        };

                        // When file is selected:
                        if(ImGui.Selectable(leafNode.ID, LinkedFile.SelectedFiles.Contains(leafNode))) {
                            // Multiselect
                            if(!ImGui.GetIO().KeyCtrl)
                                LinkedFile.SelectedFiles.Clear();

                            if(!LinkedFile.SelectedFiles.Contains(leafNode))
                                LinkedFile.SelectedFiles.Add(leafNode);

                            LinkedFile.ActiveFile = leafNode;
                        }

                        if(popVar)
                            ImGui.PopStyleColor();

                        // Context Menu (for files)
                        if(ImGui.BeginPopupContextItem()) {
                            if(ImGui.Selectable("Export")) {
                                string? filterName = Path.GetExtension(leafNode.ID);
                                if(filterName != string.Empty) // If the file has an extension.
                                    filterName = new string(Path.GetExtension(leafNode.ID))[1..].ToUpperInvariant() + " file";

                                string? result = Dialogs.SaveFileDialog(
                                    title: "Export at...",
                                    defaultPath: leafNode.ID,
                                    filter: Path.GetExtension(leafNode.ID),
                                    filterName: filterName);

                                if(result is not null)
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

                                if(result is not null) {
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

                                    if(FileManager.CurrentFile is not null)
                                        FileManager.CurrentFile.Saved = false;
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

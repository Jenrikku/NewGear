using ImGuiNET;
using NewGear.Commons.Trees.TrueTree;
using NewGear.GearSystem.Interfaces;

namespace NewGear.Gears.BuiltIn;

public class FileTree : IWindowGear {
    public string Name { get; } = "File Tree";
    public string Path { get; } = "Basic Editors/File Tree";

    public IFile? AttachedFile { get; set; }

    private float _windowContentWidth = 0;
    private string _searchQuery = string.Empty;

    public void RenderContents() {

        if(AttachedFile is not IArchiveFile archive) {
            ImGui.TextDisabled("Open an archive to show its files here.");
            return;
        }

        _windowContentWidth = ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X;

        #region Search Box

        ImGui.SetNextItemWidth(_windowContentWidth);

        ImGui.InputTextWithHint("Search",
                                "Search for files here...",
                                ref _searchQuery,
                                256);

        #endregion

        RenderNode(archive.RootNode);

    }

    private static void RenderNode(BranchNode<byte[]> node) {
        if(!ImGui.TreeNode(node.Name))
            return;

        foreach(INode<byte[]> child in node) {

            if(child is BranchNode<byte[]> branch)
                RenderNode(branch);

            ImGui.Selectable(child.Name);

        }

        ImGui.TreePop();
    }
} 
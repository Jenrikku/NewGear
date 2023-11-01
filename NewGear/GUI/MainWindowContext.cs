using ImGuiNET;
using Silk.NET.OpenGL;
using NewGear.FileManagement;
using NewGear.GearSystem.GearManagement;
using NewGear.GearSystem.Interfaces;
using TinyFileDialogsSharp;

namespace NewGear.GUI;

internal class MainWindowContext : WindowContext
{
    byte[] data = new byte[1024];

    public MainWindowContext()
        : base()
    {
        Window.Load += () => Window.Title = "NewGear";

        Window.Render += (deltaSeconds) =>
        {
            if (ImGuiController is null)
                return;

            ImGuiController.MakeCurrent();

            RenderMainMenuBar();

            RenderEditors();

            ShortcutHandler.ExecuteShortcuts(Keyboard!);
            FileOpenQueue.Check();

            GL!.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL!.Clear(ClearBufferMask.ColorBufferBit);
            GL!.Viewport(Window.FramebufferSize);
            ImGuiController.Render();
        };

        SetShortcuts();
    }

    private void RenderMainMenuBar()
    {
        if (!ImGui.BeginMainMenuBar())
            return;

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Open"))
                OpenFile();

            if (FileHolder.CurrentFile is null)
                ImGui.BeginDisabled();

            if (ImGui.MenuItem("Save")) { }

            if (ImGui.MenuItem("Save as...")) { }

            if (ImGui.MenuItem("Close")) { }

            ImGui.EndDisabled();

            ImGui.Separator();

            if (ImGui.MenuItem("Exit"))
                WindowManager.Stop();

            ImGui.EndMenu();
        }

        #region FileTabs

        ImGuiTabBarFlags barFlags = ImGuiTabBarFlags.None;
        int fileCount = FileHolder.Files.Count;

        if (fileCount > 0 && ImGui.BeginTabBar("fileTabs", barFlags))
        {
            for (int i = 0; i < fileCount; i++)
            {
                ImGuiTabItemFlags flags = ImGuiTabItemFlags.NoPushId;

                var (file, metadata) = FileHolder.Files[i];

                if (!metadata.Saved)
                    flags |= ImGuiTabItemFlags.UnsavedDocument;

                bool opened = true;

                ImGui.PushID(metadata.Name);

                if (
                    ImGui.BeginTabItem(metadata.Name, ref opened, flags)
                    && FileHolder.CurrentFile != file
                )
                {
                    FileHolder.CurrentFile = file;
                    FileHolder.CurrentMetadata = metadata;

                    EditorHolder.ChangeCurrentEditors(file);
                }

                ImGui.EndTabItem();

                ImGui.PopID();

                if (!opened && CloseFileAt(i))
                    i--;
            }

            ImGui.EndTabBar();
        }
        else
            EditorHolder.ClearCurrentEditors();

        #endregion

        ImGui.EndMainMenuBar();
    }

    private void RenderEditors()
    {
        if (EditorHolder.Editors is null)
        {
            RenderNoFileScreen();
            return;
        }

        foreach (IWindowGear editor in EditorHolder.Editors)
        {
            if (!ImGui.Begin(editor.Name))
                continue;

            editor.RenderContents();

            ImGui.End();
        }
    }

    private void RenderNoFileScreen()
    {
        ImGuiWindowFlags flags =
            ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.NoInputs
            | ImGuiWindowFlags.NoSavedSettings;

        ImGui.SetNextWindowPos(
            ImGui.GetWindowViewport().GetCenter(),
            ImGuiCond.Always,
            new(0.5f, 0.5f)
        );

        if (!ImGui.Begin("##", flags))
            return;

        ImGui.TextDisabled("Please, open a file from the menu or drop it here.");

        ImGui.End();
    }

    private void SetShortcuts()
    {
        ShortcutHandler.SetShorcutAction(Shortcuts.OpenFile, OpenFile);
    }

    private void OpenFile()
    {
        if (TinyFileDialogs.OpenFileDialog(out string[]? paths, allowMultipleSelects: true))
            FileOpenQueue.Add(paths);
    }

    private bool SaveFile()
    {
        if (string.IsNullOrEmpty(FileHolder.CurrentMetadata!.Value.Path))
            return SaveFileAs();

        return false;
    }

    private bool SaveFileAs()
    {
        return false;
    }

    private bool CloseFileAt(int index)
    {
        return false;
    }
}

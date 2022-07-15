using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using NewGear.MainMachine.GUI.WindowSystem;
using Raylib_cs;
using System.Numerics;

using static NewGear.MainMachine.GUI.Constants;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        internal static ImguiController Controller = new();
        internal static RenderTexture2D ViewportTexture = Raylib.LoadRenderTexture(1, 1);
        internal static bool[] OpenedWindows = new bool[3];

        internal static unsafe void Start() {
            Raylib.SetTraceLogCallback(&Logging.LogConsole);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(MAIN_WINDOW_WIDTH, MAIN_WINDOW_HEIGHT, "New Gear | Alpha");
            Raylib.SetTargetFPS(60);

            Controller.Load(MAIN_WINDOW_WIDTH, MAIN_WINDOW_HEIGHT);

            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            ImGui.GetStyle().Colors[(int) ImGuiCol.WindowBg] = IMGUI_WINDOW_BACKGROUND;
            ImGui.GetStyle().Colors[(int) ImGuiCol.DockingEmptyBg] = IMGUI_DOCKSPACE_BACKGROUND;

            // Default windows (will be replaced with configuration file later)
            OpenedWindows[0] = true;  // File Tree
            OpenedWindows[1] = false; // Viewport
            OpenedWindows[2] = false; // Hex Editor

            abortExit: // Label used for when the files have not been closed properly. (Not saved)
            while(!Raylib.WindowShouldClose()) {
                // Feed the input events to our ImGui Controller, which passes them through to ImGui.
                Controller.Update(Raylib.GetFrameTime());

                // File drag and drop
                if(Raylib.IsFileDropped()) {
                    FileManager.OpenFiles(Raylib.GetDroppedFiles());
                    Raylib.ClearDroppedFiles();
                }

                // Placeholder window (to prevent "Debug")
                ImGui.Begin("null", 
                    ImGuiWindowFlags.NoInputs |
                    ImGuiWindowFlags.NoDecoration |
                    ImGuiWindowFlags.NoBackground |
                    ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoBringToFrontOnFocus);

                if(RenderMainMenuBar())
                    break; // Go to dispose.

                if(ImGui.GetIO().MouseClicked[1])
                    WindowManager.RenderContextMenu(ImGui.GetIO().MousePos);

                ImGui.End();

                #region Windows

                ImGui.DockSpaceOverViewport();
                WindowManager.RenderActiveWindows();

                #endregion

                // Draw:

                Raylib.BeginDrawing();

                Raylib.ClearBackground(MAIN_WINDOW_BACKGROUND);
                Controller.Draw();

                Raylib.EndDrawing();
            }

            // Disposing:

            int fileCount = FileManager.LoadedFiles.Count;

            for(int i = fileCount - 1; i >= 0; i--)
                if(!FileManager.CloseFile(FileManager.LoadedFiles.ElementAt(i)))
                    goto abortExit; // If not all files were closed, abort exit.

            Raylib.UnloadRenderTexture(ViewportTexture);

            Controller.Dispose();
            Raylib.CloseWindow();
        }
    }
}

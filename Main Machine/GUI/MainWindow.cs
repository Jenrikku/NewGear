using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using NewGear.MainMachine.GUI.WindowSystem;
using Raylib_cs;

using static NewGear.MainMachine.GUI.Constants;
using static Raylib_cs.Raylib;
using static ImGuiNET.ImGui;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        internal static ImguiController Controller = new();
        internal static RenderTexture2D ViewportTexture = LoadRenderTexture(1, 1);
        internal static bool[] OpenedWindows = new bool[3];

        internal static unsafe void Start() {
            SetTraceLogCallback(&Logging.LogConsole);
            SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
            InitWindow(MAIN_WINDOW_WIDTH, MAIN_WINDOW_HEIGHT, "New Gear | Alpha");
            SetWindowIcon(LoadImage("icon.png"));
            SetExitKey(KeyboardKey.KEY_NULL);
            SetTargetFPS(60);

            Controller.Load(MAIN_WINDOW_WIDTH, MAIN_WINDOW_HEIGHT);

            GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            // To be replaced in the future with theming:
            GetStyle().Colors[(int) ImGuiCol.WindowBg] = IMGUI_WINDOW_BACKGROUND;
            GetStyle().Colors[(int) ImGuiCol.DockingEmptyBg] = IMGUI_DOCKSPACE_BACKGROUND;

            // Default windows (will be replaced with configuration file later)
            OpenedWindows[0] = true;  // File Tree
            OpenedWindows[1] = false; // Viewport
            OpenedWindows[2] = false; // Hex Editor

            abortExit: // Label used for when the files have not been closed properly. (Not saved)
            while(!WindowShouldClose()) {
                // Feed the input events to our ImGui Controller, which passes them through to ImGui.
                Controller.Update(GetFrameTime());

                // File drag and drop
                if(IsFileDropped()) {
                    FileManager.OpenFiles(GetDroppedFiles());
                    ClearDroppedFiles();
                }

                // Placeholder window (to prevent "Debug")
                Begin(string.Empty, 
                    ImGuiWindowFlags.NoInputs |
                    ImGuiWindowFlags.NoDecoration |
                    ImGuiWindowFlags.NoBackground |
                    ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoBringToFrontOnFocus |
                    ImGuiWindowFlags.NoSavedSettings);

                if(RenderMainMenuBar())
                    break; // Go to dispose.

                End();

                #region Windows

                DockSpaceOverViewport();
                WindowManager.RenderActiveWindows();

                #endregion

                // Draw:

                BeginDrawing();

                ClearBackground(MAIN_WINDOW_BACKGROUND);
                Controller.Draw();

                EndDrawing();
            }

            // Disposing:

            int fileCount = FileManager.LoadedFiles.Count;

            for(int i = fileCount - 1; i >= 0; i--)
                if(!FileManager.CloseFile(FileManager.LoadedFiles.ElementAt(i)))
                    goto abortExit; // If not all files were closed, abort exit.

            UnloadRenderTexture(ViewportTexture);

            Controller.Dispose();
            CloseWindow();
        }
    }
}

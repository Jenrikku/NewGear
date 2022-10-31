using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using NewGear.MainMachine.GUI.WindowSystem;
using Raylib_cs;
using rlImGui_cs;
using System.Text;

using static NewGear.MainMachine.GUI.Constants;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        internal static RenderTexture2D ViewportTexture = Raylib.LoadRenderTexture(1, 1);
        
        internal static unsafe void Start() {
            Raylib.SetTraceLogCallback(&Logging.LogConsole);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(MAIN_WINDOW_WIDTH, MAIN_WINDOW_HEIGHT, "New Gear | Alpha");
            Raylib.SetExitKey(KeyboardKey.KEY_NULL);
            Raylib.SetTargetFPS(60);

            // Dark mode support (Windows 10+):
            InitColorMode((nint) Raylib.GetWindowHandle());

            rlImGui.Setup();

            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            // To be replaced in the future with theming:
            ImGui.GetStyle().Colors[(int) ImGuiCol.WindowBg] = IMGUI_WINDOW_BACKGROUND;
            ImGui.GetStyle().Colors[(int) ImGuiCol.DockingEmptyBg] = IMGUI_DOCKSPACE_BACKGROUND;

            WindowManager.Initialize();

            abortExit: // Label used for when the files have not been closed properly. (Not saved)
            while(!Raylib.WindowShouldClose()) {
                Raylib.BeginDrawing();

                Raylib.ClearBackground(MAIN_WINDOW_BACKGROUND);
                rlImGui.Begin();

                DialogSystem.RenderMessages();
                DialogSystem.RenderFileSelectDialog();
                DialogSystem.RenderBasicInputDialogs();

                // File drag and drop
                if(Raylib.IsFileDropped()) {
                    FilePathList droppedFiles = Raylib.LoadDroppedFiles();

                    string[] paths = new string[droppedFiles.count];

                    for(uint i = 0; i < droppedFiles.count; i++) {
                        byte* startpos = droppedFiles.paths[i];
                        byte* pos = startpos;
                        int length = 0;

                        while(*pos++ != 0)
                            length++;

                        paths[i] = Encoding.Default.GetString(startpos, length);
                    }

                    Raylib.UnloadDroppedFiles(droppedFiles);

                    FileDroppedAction?.Invoke(paths);
                }

                ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new(0.5f, 0.5f));

                // Placeholder window (to prevent "Debug")
                ImGui.Begin(string.Empty, 
                    ImGuiWindowFlags.NoInputs |
                    ImGuiWindowFlags.NoDecoration |
                    ImGuiWindowFlags.NoBackground |
                    ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoBringToFrontOnFocus |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.AlwaysAutoResize);

                if(RenderMainMenuBar())
                    break; // Go to dispose.

                if(FileManager.LoadedFiles.Count <= 0)
                    ImGui.TextDisabled("Drop a file here or open it.");

                ImGui.End();

                if(FileManager.LoadedFiles.Count > 0) {
                    ImGui.DockSpaceOverViewport();
                    WindowManager.RenderActiveWindows();
                }

                rlImGui.End();
                Raylib.EndDrawing();
            }

            // Disposing:

            int fileCount = FileManager.LoadedFiles.Count;

            for(int i = fileCount - 1; i >= 0; i--)
                if(!FileManager.CloseFile(FileManager.LoadedFiles.ElementAt(i)))
                    goto abortExit; // If not all files were closed, abort exit.

            Raylib.UnloadRenderTexture(ViewportTexture);

            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}

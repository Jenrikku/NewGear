using ImGuiNET;
using Raylib_cs;
using System.Numerics;
using static NewGear.MainMachine.GUI.Constants;
using static NewGear.MainMachine.GUI.MainWindow;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class Viewport : ImGUIWindow {
        public Vector2 WindowPositionMin { get; set; }
        public Vector2 WindowPositionMax { get; set; }

        public void Render() {
            // Draw ViewportTexture:
            Raylib.BeginTextureMode(ViewportTexture);
            Raylib.ClearBackground(VIEWPORT_BACKGROUND);

            Raylib.DrawText("WIP", 0, 0, 20, Color.GRAY);

            //Raylib.DrawGrid(10, 1);

            Raylib.EndTextureMode();

            // Update:

            ImGui.SetNextWindowSize(new(500, 350), ImGuiCond.FirstUseEver);
            ImGui.Begin("Viewport", ref OpenedWindows[1]);
            WindowPositionMin = ImGui.GetWindowPos();
            WindowPositionMax = WindowPositionMin + ImGui.GetWindowSize();

            ImGui.TextDisabled("There is no model data to show.");

            //if(...) {
            //    // DrawViewport
            //    var size = ImGui.GetContentRegionAvail();

            //    if(MainWindow.ViewportTexture.texture.width != size.X || MainWindow.ViewportTexture.texture.height != size.Y) {
            //        Raylib.UnloadRenderTexture(MainWindow.ViewportTexture);
            //        MainWindow.ViewportTexture = Raylib.LoadRenderTexture((int) size.X, (int) size.Y);
            //    }

            //    ImGui.Image((IntPtr) MainWindow.ViewportTexture.texture.id, size, new(0, 1), new(1, 0));
            //}
        }

        public void ContextMenu(Vector2 mousePosition) {
            return;
        }
    }
}

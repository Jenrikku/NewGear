using ImGuiNET;
using NewGear.MainMachine.GUI.WindowSystem.Windows;
using System.Numerics;

namespace NewGear.MainMachine.GUI.WindowSystem {
    internal static class WindowManager {
        public static ImGUIWindow[] WindowList = new ImGUIWindow[] {
            new FileTree(),
            new Viewport(),
            new HexEditor()
        };

        public static void RenderActiveWindows() {
            for(int i = 0; i < WindowList.Length; i++) {
                if(MainWindow.OpenedWindows[i]) {
                    WindowList[i].Render();
                    ImGui.End();
                }
            }
        }

        public static void RenderContextMenu(Vector2 mousePosition) {
            foreach(ImGUIWindow window in WindowList)
                if(ImGui.IsMouseHoveringRect(window.WindowPositionMin, window.WindowPositionMax, false))
                    window.ContextMenu(mousePosition);
        }
    }
}

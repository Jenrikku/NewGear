using Raylib_cs;
using System.Numerics;

namespace NewGear.MainMachine.GUI {
    internal static class Constants {
        public const ushort MAIN_WINDOW_WIDTH = 1280;
        public const ushort MAIN_WINDOW_HEIGHT = 720;

        public static readonly Color MAIN_WINDOW_BACKGROUND = new(0x1A, 0x1A, 0x1A, 0xFF);
        public static readonly Color VIEWPORT_BACKGROUND = new(0x33, 0x33, 0x33, 0xFF);

        public static readonly Vector4 IMGUI_DOCKSPACE_BACKGROUND = new(0.1F, 0.1F, 0.1F, 1);
        public static readonly Vector4 IMGUI_WINDOW_BACKGROUND = new(0.2F, 0.2F, 0.2F, 1);
    }
}

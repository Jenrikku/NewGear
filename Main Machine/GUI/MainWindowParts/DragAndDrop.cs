using NewGear.MainMachine.FileSystem;
using Raylib_cs;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        internal static Action<string[]> FileDroppedAction =
            FileManager.OpenFiles;

        internal static void RestoreDragAndDrop() {
            FileDroppedAction = FileManager.OpenFiles;
        }
    }
}

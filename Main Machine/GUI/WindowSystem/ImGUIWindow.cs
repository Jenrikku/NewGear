using NewGear.MainMachine.FileSystem;

namespace NewGear.MainMachine.GUI.WindowSystem {
    internal interface ImGUIWindow {
        public abstract FileInstance LinkedFile { get; set; }

        public abstract void Render();
    }
}

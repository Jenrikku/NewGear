using System.Numerics;

namespace NewGear.MainMachine.GUI.WindowSystem {
    internal interface ImGUIWindow {
        public Vector2 WindowPositionMin { get; set; }
        public Vector2 WindowPositionMax { get; set; }

        public void Render();
        public void ContextMenu(Vector2 mousePosition);
    }
}

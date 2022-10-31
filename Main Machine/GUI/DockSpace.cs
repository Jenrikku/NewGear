using ImGuiNET;
using System.Runtime.InteropServices;

// Thanks to JuPaHe64! (https://github.com/jupahe64)

namespace NewGear.MainMachine.GUI {
    internal class DockLayout {
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr igDockBuilderGetNode(uint node_id);


        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint igDockBuilderSplitNode(uint node_id, ImGuiDir split_dir,
            float size_ratio_for_node_at_dir, out uint out_id_at_dir, out uint out_id_at_opposite_dir);


        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igDockBuilderDockWindow(string window_name, uint node_id);



        private (ImGuiDir splitDir, float splitPos, DockLayout childA, DockLayout childB)? _splitInfo;
        private readonly string[]? _windows;

        public DockLayout(ImGuiDir splitDir, float splitPos, DockLayout childA, DockLayout childB) {
            _splitInfo = (splitDir, splitPos, childA, childB);
            _windows = null;
        }

        public DockLayout(params string[] windows) {
            _splitInfo = null;
            _windows = windows;
        }

        public void ApplyTo(uint dockNode) {
            if(_splitInfo == null) {
                for(int i = 0; i < _windows!.Length; i++) {
                    igDockBuilderDockWindow(_windows![i], dockNode);
                }
            } else {
                var (splitDr, splitPos, childA, childB) = _splitInfo.Value;

                igDockBuilderSplitNode(dockNode, splitDr, splitPos, out uint childNodeA, out uint childNodeB);

                childA.ApplyTo(childNodeA);
                childB.ApplyTo(childNodeB);
            }
        }
    }

    internal class DockSpace {
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint igDockBuilderAddNode(uint node_id, ImGuiDockNodeFlags flags);


        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint igDockBuilderDeleteNode(uint node_id);


        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint igDockBuilderFinish(uint node_id);



        private uint _id;

        public DockSpace(string label) {
            _id = ImGui.GetID(label);
        }

        public uint DockId => _id;

        public void Setup(DockLayout layout) {
            igDockBuilderAddNode(_id, ImGuiDockNodeFlags.None);
            layout.ApplyTo(_id);
            igDockBuilderFinish(_id);
        }
    }
}

using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using System.Reflection;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class Properties : ImGUIWindow {
        public FileInstance LinkedFile { get; set; }

        public Properties(FileInstance linkedFile) {
            LinkedFile = linkedFile;
        }

        public void Render() {
            ImGui.Begin("Properties", ref WindowManager.WindowOpenedStates[(int) WindowManager.WindowType.Properties]);

            if(LinkedFile.ActiveFile is null) {
                ImGui.BeginDisabled();
                ImGui.TextWrapped("Select a file to see its properties.");
                ImGui.EndDisabled();
                return;
            }

            if(LinkedFile.ActiveFile.Metadata is not null) {
                object metadate = LinkedFile.ActiveFile.Metadata;

                if(ImGui.TreeNodeEx(metadate.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen)) {
                    foreach(FieldInfo field in metadate.GetType().GetFields()) {
                        if(ImGui.Selectable(field.Name)) {

                        }

                        object? value = field.GetValue(metadate);

                        if(value is not null && value.GetType().IsPrimitive) {
                            ImGui.SameLine();
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize(value.ToString()).X - ImGui.GetStyle().WindowPadding.X);

                            ImGui.Text(value.ToString());
                        }
                    }

                    ImGui.TreePop();

                    ImGui.Separator();
                }

                if(LinkedFile.ActiveFile.Contents is not null) {
                    foreach(FieldInfo field in LinkedFile.ActiveFile.Contents.GetType().GetFields()) {
                        ImGui.Text(field.Name);
                    }
                }
            }
        }
    }
}

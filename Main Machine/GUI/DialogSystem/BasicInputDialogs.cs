using ImGuiNET;

namespace NewGear.MainMachine.GUI {
    public static partial class DialogSystem {
        private record struct BasicInputDialogContents(string Title, Action Rendering, Action OkButtonPressed);

        private static bool basicInputDialogOpened;
        private static BasicInputDialogContents currentBasicInputContents;

        private static int auxiliarIndex;

        public static void OpenBasicComboBoxDialog(string title, string message, string[] entries, Action<string> action) {
            currentBasicInputContents = new() {
                Title = title,
                Rendering = () => {
                    ImGui.TextWrapped(message);

                    ImGui.Combo("##", ref auxiliarIndex, entries, entries.Length);
                },
                OkButtonPressed = () => action.Invoke(entries[auxiliarIndex])
            };

            basicInputDialogOpened = true;
        }

        public static void RenderBasicInputDialogs() {
            if(!basicInputDialogOpened) {
                auxiliarIndex = 0;
                return;
            }

            ImGui.OpenPopup(currentBasicInputContents.Title);

            ImGui.SetNextWindowSize(new(3000, 3000), ImGuiCond.Appearing); // Fixes text wrapping incorrectly.
            ImGui.SetNextWindowSizeConstraints(new(10, 10), ImGui.GetMainViewport().Size / 2);
            ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new(0.5f, 0.5f));

            if(ImGui.BeginPopupModal(currentBasicInputContents.Title, ref basicInputDialogOpened, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
                currentBasicInputContents.Rendering.Invoke();

                ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 100 - ImGui.GetStyle().WindowPadding.X);
                ImGui.SetNextItemWidth(50);

                if(ImGui.Button("Ok"))
                    currentBasicInputContents.OkButtonPressed.Invoke();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(50);

                if(ImGui.Button("Cancel")) {
                    ImGui.CloseCurrentPopup();
                    basicInputDialogOpened = false;
                }

                ImGui.EndPopup();
            }
        }
    }
}

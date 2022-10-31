using ImGuiNET;

namespace NewGear.MainMachine.GUI {
    public static partial class DialogSystem {
        private record struct MessageDialog(string title, string message, DialogOptions options, Action[]? actions);

        private static List<MessageDialog> messageQueue = new();
        private static bool currentMessageOpened;

        public static void OpenMessageDialog(string title, string message, DialogOptions options = DialogOptions.Ok, Action[]? actions = null) =>
            messageQueue.Add(new(title, message, options, actions));

        public static void RenderMessages() {
            if(messageQueue.Count > 0) {
                currentMessageOpened = true;

                MessageDialog dialog = messageQueue[0];

                ImGui.OpenPopup(dialog.title);

                ImGui.SetNextWindowSize(new(3000, 3000), ImGuiCond.Appearing); // Fixes text wrapping incorrectly.
                ImGui.SetNextWindowSizeConstraints(new(10, 10), ImGui.GetMainViewport().Size / 2);
                ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new(0.5f, 0.5f));

                if(ImGui.BeginPopupModal(dialog.title, ref currentMessageOpened, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
                    ImGui.TextWrapped(dialog.message);

                    byte actionIndex = 0;

                    byte count = 0;
                    byte num = (byte) dialog.options;
                    while(num != 0) {
                        if((num & 1) == 1)
                            count++;
                        num /= 2;
                    }

                    if(count != 0)
                        ImGui.SetCursorPosX(ImGui.GetItemRectSize().X - 50 * count - ImGui.GetStyle().WindowPadding.X);

                    if(((byte) dialog.options & 0b0001) == 0b0001) {
                        if(ImGui.Button("Ok", new(50, 19))) {
                            dialog.actions?[actionIndex].Invoke();
                            ImGui.CloseCurrentPopup();
                            currentMessageOpened = false;
                        }

                        ImGui.SameLine();
                        actionIndex++;
                    }

                    if(((byte) dialog.options & 0b0010) == 0b0010) {
                        if(ImGui.Button("Yes", new(50, 19))) {
                            dialog.actions?[actionIndex].Invoke();
                            ImGui.CloseCurrentPopup();
                            currentMessageOpened = false;
                        }

                        ImGui.SameLine();
                        actionIndex++;
                    }

                    if(((byte) dialog.options & 0b0100) == 0b0100) {
                        if(ImGui.Button("No", new(50, 19))) {
                            dialog.actions?[actionIndex].Invoke();
                            ImGui.CloseCurrentPopup();
                            currentMessageOpened = false;
                        }

                        ImGui.SameLine();
                        actionIndex++;
                    }

                    if(((byte) dialog.options & 0b1000) == 0b1000) {
                        if(ImGui.Button("Cancel", new(50, 19))) {
                            dialog.actions?[actionIndex].Invoke();
                            ImGui.CloseCurrentPopup();
                            currentMessageOpened = false;
                        }
                    }

                    ImGui.EndPopup();
                }

                if(!currentMessageOpened)
                    messageQueue.Remove(dialog);
            }
        }

        public enum DialogOptions : byte {

            // Ok     = 0001
            // Yes    = 0010
            // No     = 0100
            // Cancel = 1000

            None = 0b0000,
            Ok = 0b0001,
            OkCancel = 0b1001,
            YesCancel = 0b1010,
            YesNo = 0b0110,
            YesNoCancel = 0b1110
        }
    }
}

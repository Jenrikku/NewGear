using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using NewGear.Trees.TrueTree;
using System.Diagnostics;
using System.Numerics;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    internal class HexView : ImGUIWindow {
        public FileInstance LinkedFile { get; set; }
        public byte? ColumnAmount { get; set; } = 16;

        private byte[]? buffer;
        private int cursorIndex;
        private int selectionEndIndex;

        public HexView(FileInstance file) {
            LinkedFile = file;

            LinkedFile.ActiveFileChanged += (INode? file) => {
                buffer = (file?.LinkedNode?.Contents is byte[]) ? file.LinkedNode.Contents : null;
                cursorIndex = -1;
                selectionEndIndex = -1;
            };
        }

        public void Render() {
            ImGui.Begin(
                "Hex View",
                ref WindowManager.WindowOpenedStates[(int) WindowManager.WindowType.HexView],
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
            
            if(buffer is null) {
                ImGui.BeginDisabled();
                ImGui.TextWrapped("Select an item to display its contents.");
                ImGui.EndDisabled();
                return;
            }

            byte finalColumnAmount = 0;

            #region Header

            ImGui.BeginChild(
                "##Header",
                new(ImGui.GetContentRegionAvail().X, ImGui.GetFontSize() + ImGui.GetStyle().ItemInnerSpacing.Y * 2),
                false,
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

            ImGui.Text("Address");

            ImGui.SameLine();
            ImGui.SetCursorPosX(100);

            if(ColumnAmount.HasValue) {
                finalColumnAmount = ColumnAmount.Value;

                for(byte i = 0; i < ColumnAmount; i++) {
                    ImGui.Text(i.ToString("X2"));
                    ImGui.SameLine();
                }
            } else { // Automatic column amount.
                while(ImGui.GetContentRegionAvail().X >= (finalColumnAmount * 10 + 20) || finalColumnAmount < 4) {
                    ImGui.Text(finalColumnAmount++.ToString("X2"));
                    ImGui.SameLine();
                }
            }

            ImGui.NewLine();
            ImGui.Separator();

            ImGui.EndChild();

            #endregion

            #region MainView

            ImGui.BeginChild(
                "##MainView",
                ImGui.GetContentRegionAvail() - new Vector2(0, ImGui.GetFontSize() + ImGui.GetStyle().ItemInnerSpacing.Y * 3.5f));

            int rowCount = 0;

            if(buffer.Length > 0) {
                rowCount = buffer.Length / finalColumnAmount;

                if(buffer.Length % finalColumnAmount != 0)
                    rowCount++;
            }

            for(int i = 0; i < rowCount; i++) {
                ImGui.Text((i * finalColumnAmount).ToString("X10") + " |");

                ImGui.SameLine();
                ImGui.SetCursorPosX(100);

                string text = string.Empty;
                byte[] row = buffer[
                                (i * finalColumnAmount)..
                                (buffer.Length - 1 >= (i * finalColumnAmount + (finalColumnAmount - 1)) ?
                                    (i * finalColumnAmount + finalColumnAmount) :
                                    buffer.Length)];

                for(int j = 0; j < row.Length; j++) {
                    byte b = row[j];
                    int byteIndex = i * finalColumnAmount + j;

                    unsafe {
                        if(b == 0)
                            ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled));

                        Vector2 previousCursorPos = ImGui.GetCursorPos();

                        if(cursorIndex < byteIndex && byteIndex <= selectionEndIndex)
                            ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.Header));

                        if(cursorIndex == byteIndex)
                            ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.HeaderHovered));

                        ImGui.Text(b.ToString("X2"));

                        if(cursorIndex <= byteIndex && byteIndex <= selectionEndIndex)
                            ImGui.PopStyleColor();

                        Vector2 itemMin = ImGui.GetItemRectMin();
                        Vector2 itemMax = ImGui.GetItemRectMax();

                        Vector2 currentCursorPos = ImGui.GetCursorPos();

                        if(ImGui.IsMouseHoveringRect(itemMin, itemMax)) {
                            if(ImGui.IsMouseClicked(ImGuiMouseButton.Left)) {
                                cursorIndex = byteIndex;
                            }

                            if(ImGui.IsMouseDown(ImGuiMouseButton.Left)) {
                                if(cursorIndex <= byteIndex)
                                    selectionEndIndex = byteIndex;
                            }
                        }

                        if(b == 0)
                            ImGui.PopStyleColor();
                    }

                    if(b < 0x20 || (b < 0xA0 && b > 0x7E))
                        text += ".";
                    else if(b == 0x25)
                        text += "%%";
                    else
                        text += (char) b;

                    ImGui.SameLine();
                }

                ImGui.SetCursorPosX(110 + finalColumnAmount * 22);

                ImGui.Text(text);
            }

            if(buffer.Length <= 0) {
                ImGui.TextDisabled("This file is empty.");
            }

            ImGui.EndChild();

            #endregion

            #region Footer

            ImGui.BeginChild(
                "##Footer",
                ImGui.GetContentRegionAvail(),
                false,
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

            ImGui.Separator();

            Vector2 oldPos = ImGui.GetCursorPos();

            ImGui.SetCursorPosY(7);

            ImGui.Text($"File length: {buffer.Length.ToString("X")}");

            ImGui.SetCursorPos(oldPos + ImGui.GetContentRegionAvail() - new Vector2(ImGui.CalcTextSize("Column amount:").X + 110, 0));
            ImGui.SetCursorPosY(7);

            ImGui.Text("Column amount:");
            ImGui.SameLine();

            ImGui.SetCursorPosY(5);
            ImGui.SetNextItemWidth(100);

            int columnAmount = ColumnAmount ?? 0;
            if(ImGui.InputInt(string.Empty, ref columnAmount)) {
                if(columnAmount > 0xFF)
                    columnAmount = 0xFF;

                if(columnAmount < 0)
                    columnAmount = 0;

                if(columnAmount == 0)
                    ColumnAmount = null;
                else
                    ColumnAmount = (byte) columnAmount;
            }

            ImGui.EndChild();

            #endregion

            #region HotKeys

            // Ctrl + C
            if(ImGui.GetIO().KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.C)
                && cursorIndex > -1) {
                string output = string.Empty;

                foreach(byte b in buffer[cursorIndex..(selectionEndIndex + 1)])
                    output += b.ToString("X2") + " ";

                output = output.TrimEnd();

                ImGui.SetClipboardText(output);
            }

            #endregion
        }
    }
}

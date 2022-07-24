using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using System.Globalization;
using System.Numerics;

namespace NewGear.MainMachine.GUI.WindowSystem.Windows {
    // C# port of ocornut's imgui_memory_editor.h - https://gist.github.com/ocornut/0673e37e54aff644298b

    // Mini memory editor for ImGui (to embed in your game/tools)
    // v0.10
    // Animated gif: https://cloud.githubusercontent.com/assets/8225057/9028162/3047ef88-392c-11e5-8270-a54f8354b208.gif
    //
    // You can adjust the keyboard repeat delay/rate in ImGuiIO.
    // The code assume a mono-space font for simplicity! If you don't use the default font, use ImGui::PushFont()/PopFont() to switch to a mono-space font before caling this.
    //
    // Usage:
    //   static MemoryEditor memory_editor;                                                     // save your state somewhere
    //   memory_editor.Draw("Memory Editor", mem_block, mem_block_size, (size_t)mem_block);     // run

    internal class HexEditor : ImGUIWindow {
        public Vector2 WindowPositionMin { get; set; }
        public Vector2 WindowPositionMax { get; set; }

        public bool AllowEdits = true;
        public int Column = 16;
        public int DataEditingAddr = -1;
        public bool DataEditingTakeFocus;
        public int BaseDisplayAddress = 0;
        public byte[] DataInput = new byte[32];
        public byte[] AddrInput = new byte[32];

        public unsafe void Render() {
            ImGui.SetNextWindowSize(new(500, 350), ImGuiCond.FirstUseEver);
            if(!ImGui.Begin("Hex Editor", ImGuiWindowFlags.NoScrollbar)) {
                ImGui.End();
                return;
            }

            if(FileManager.CurrentFile?.CurrentSubFile is null) {
                ImGui.TextDisabled("Select an item to display its contents.");
                return;
            }

            int fileLength = FileManager.CurrentFile.CurrentSubFile.Length;

            float line_height = ImGuiNative.igGetTextLineHeight();
            int line_total_count = (fileLength + Column - 1) / Column;

            ImGuiNative.igSetNextWindowContentSize(new Vector2(0.0f, line_total_count * line_height));
            ImGui.BeginChild("##scrolling", new Vector2(0, -ImGuiNative.igGetFrameHeightWithSpacing()), false, 0);

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            int addr_digits_count = 0;
            for(int n = BaseDisplayAddress + fileLength - 1; n > 0; n >>= 4)
                addr_digits_count++;

            float glyph_width = ImGui.CalcTextSize("F").X;
            float cell_width = glyph_width * 3; // "FF " we include trailing space in the width to easily catch clicks everywhere

            var clipper = new ImGuiListClipper2(line_total_count, line_height);
            int visible_start_addr = clipper.DisplayStart * Column;
            int visible_end_addr = clipper.DisplayEnd * Column;

            bool data_next = false;

            if(!AllowEdits || DataEditingAddr >= fileLength)
                DataEditingAddr = -1;

            int data_editing_addr_backup = DataEditingAddr;

            if(DataEditingAddr != -1) {
                if(ImGui.IsKeyPressed(ImGuiKey.UpArrow) && DataEditingAddr >= Column) { DataEditingAddr -= Column; DataEditingTakeFocus = true; } else if(ImGui.IsKeyPressed(ImGuiKey.DownArrow) && DataEditingAddr < fileLength - Column) { DataEditingAddr += Column; DataEditingTakeFocus = true; } else if(ImGui.IsKeyPressed(ImGuiKey.LeftArrow) && DataEditingAddr > 0) { DataEditingAddr -= 1; DataEditingTakeFocus = true; } else if(ImGui.IsKeyPressed(ImGuiKey.RightArrow) && DataEditingAddr < fileLength - 1) { DataEditingAddr += 1; DataEditingTakeFocus = true; }
            }
            if((DataEditingAddr / Column) != (data_editing_addr_backup / Column)) {
                // Track cursor movements
                float scroll_offset = ((DataEditingAddr / Column) - (data_editing_addr_backup / Column)) * line_height;
                bool scroll_desired = (scroll_offset < 0.0f && DataEditingAddr < visible_start_addr + Column * 2) || (scroll_offset > 0.0f && DataEditingAddr > visible_end_addr - Column * 2);
                if(scroll_desired)
                    ImGuiNative.igSetScrollY_Float(ImGuiNative.igGetScrollY() + scroll_offset);
            }

            for(int line_i = clipper.DisplayStart; line_i < clipper.DisplayEnd; line_i++) // display only visible items
            {
                int addr = line_i * Column;
                ImGui.Text(FixedHex(BaseDisplayAddress + addr, addr_digits_count) + ": ");
                ImGui.SameLine();

                // Draw Hexadecimal
                float line_start_x = ImGuiNative.igGetCursorPosX();
                for(int n = 0; n < Column && addr < fileLength; n++, addr++) {
                    ImGui.SameLine(line_start_x + cell_width * n);

                    if(DataEditingAddr == addr) {
                        // Display text input on current byte
                        ImGui.PushID(addr);

                        // FIXME: We should have a way to retrieve the text edit cursor position more easily in the API, this is rather tedious.
                        ImGuiInputTextCallback callback = (data) => {
                            int* p_cursor_pos = (int*) data->UserData;

                            if(ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                                *p_cursor_pos = data->CursorPos;
                            return 0;
                        };
                        int cursor_pos = -1;
                        bool data_write = false;
                        if(DataEditingTakeFocus) {
                            ImGui.SetKeyboardFocusHere();
                            ReplaceChars(DataInput, FixedHex(FileManager.CurrentFile.CurrentSubFile[addr], 2));
                            ReplaceChars(AddrInput, FixedHex(BaseDisplayAddress + addr, addr_digits_count));
                        }
                        ImGui.PushItemWidth(ImGui.CalcTextSize("FF").X);

                        var flags = ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoHorizontalScroll | ImGuiInputTextFlags.CallbackAlways;

                        if(ImGui.InputText("##data", DataInput, 32, flags, callback, (IntPtr) (&cursor_pos)))
                            data_write = data_next = true;
                        else if(!DataEditingTakeFocus && !ImGui.IsItemActive())
                            DataEditingAddr = -1;

                        DataEditingTakeFocus = false;
                        ImGui.PopItemWidth();
                        if(cursor_pos >= 2)
                            data_write = data_next = true;
                        if(data_write) {
                            int data;
                            if(TryHexParse(DataInput, out data))
                                FileManager.CurrentFile.CurrentSubFile[addr] = (byte) data;
                        }
                        ImGui.PopID();
                    } else {
                        ImGui.Text(FixedHex(FileManager.CurrentFile.CurrentSubFile[addr], 2));
                        if(AllowEdits && ImGui.IsItemHovered() && ImGui.IsMouseClicked(0)) {
                            DataEditingTakeFocus = true;
                            DataEditingAddr = addr;
                        }
                    }
                }

                ImGui.SameLine(line_start_x + cell_width * Column + glyph_width * 2);
                //separator line drawing replaced by printing a pipe char

                // Draw ASCII values
                addr = line_i * Column;
                var asciiVal = new System.Text.StringBuilder(2 + Column);
                asciiVal.Append("| ");
                for(int n = 0; n < Column && addr < fileLength; n++, addr++) {
                    int c = FileManager.CurrentFile.CurrentSubFile[addr];
                    asciiVal.Append((c >= 32 && c < 128) ? Convert.ToChar(c) : '.');
                }
                ImGui.TextUnformatted(asciiVal.ToString());  //use unformatted, so string can contain the '%' character
            }
            //clipper.End();  //not implemented
            ImGui.PopStyleVar(2);

            ImGui.EndChild();

            if(data_next && DataEditingAddr < fileLength) {
                DataEditingAddr = DataEditingAddr + 1;
                DataEditingTakeFocus = true;
            }

            ImGui.Separator();

            ImGuiNative.igAlignTextToFramePadding();
            ImGui.PushItemWidth(50);
            ImGui.PushAllowKeyboardFocus(true);
            int rows_backup = Column;
            if(ImGui.DragInt("##rows", ref Column, 0.2f, 4, 32, "%.0f rows")) {
                if(Column <= 0) Column = 4;
                Vector2 new_window_size = ImGui.GetWindowSize();
                new_window_size.X += (Column - rows_backup) * (cell_width + glyph_width);
                ImGui.SetWindowSize(new_window_size);
            }
            ImGui.PopAllowKeyboardFocus();
            ImGui.PopItemWidth();
            ImGui.SameLine();
            ImGui.Text(string.Format(" Range {0}..{1} ", FixedHex(BaseDisplayAddress, addr_digits_count),
                FixedHex(BaseDisplayAddress + fileLength - 1, addr_digits_count)));
            ImGui.SameLine();
            ImGui.PushItemWidth(70);
            if(ImGui.InputText("##addr", AddrInput, 32, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue, null)) {
                int goto_addr;
                if(TryHexParse(AddrInput, out goto_addr)) {
                    goto_addr -= BaseDisplayAddress;
                    if(goto_addr >= 0 && goto_addr < fileLength) {
                        ImGui.BeginChild("##scrolling");
                        ImGui.SetScrollFromPosY(ImGui.GetCursorStartPos().Y + (goto_addr / Column) * ImGuiNative.igGetTextLineHeight());
                        ImGui.EndChild();
                        DataEditingAddr = goto_addr;
                        DataEditingTakeFocus = true;
                    }
                }
            }
            ImGui.PopItemWidth();

            ImGui.End();
        }

        public void ContextMenu(Vector2 mousePosition) {
            return;
        }

        private static string FixedHex(int v, int count) {
            return v.ToString("X").PadLeft(count, '0');
        }

        private static bool TryHexParse(byte[] bytes, out int result) {
            string input = System.Text.Encoding.UTF8.GetString(bytes).ToString();

            if(!(FileManager.CurrentFile is null))
                FileManager.CurrentFile.Saved = false;

            return int.TryParse(input, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out result);
        }

        private static void ReplaceChars(byte[] bytes, string input) {
            var address = System.Text.Encoding.ASCII.GetBytes(input);
            for(int i = 0; i < bytes.Length; i++) {
                bytes[i] = (i < address.Length) ? address[i] : (byte) 0;
            }

            if(!(FileManager.CurrentFile is null))
                FileManager.CurrentFile.Saved = false;
        }


        class ImGuiListClipper2 {
            public float StartPosY;
            public float ItemsHeight;
            public int ItemsCount, StepNo, DisplayStart, DisplayEnd;

            public ImGuiListClipper2(int items_count = -1, float items_height = -1.0f) {
                Begin(items_count, items_height);
            }

            public unsafe void Begin(int count, float items_height = -1.0f) {
                StartPosY = ImGuiNative.igGetCursorPosY();
                ItemsHeight = items_height;
                ItemsCount = count;
                StepNo = 0;
                DisplayEnd = DisplayStart = -1;
                if(ItemsHeight > 0.0f) {
                    //int dispStart, dispEnd;

                    ImGuiListClipper* clipper = ImGuiNative.ImGuiListClipper_ImGuiListClipper();

                    ImGuiNative.ImGuiListClipper_Begin(clipper, ItemsCount, ItemsHeight);

                    while(ImGuiNative.ImGuiListClipper_Step(clipper) != 0) ;

                    ImGuiNative.ImGuiListClipper_End(clipper);

                    ImGuiNative.ImGuiListClipper_destroy(clipper);

                    ////ImGuiNative.ImGuiLis

                    //ImGuiNative.igCalcListClipping(ItemsCount, ItemsHeight, &dispStart, &dispEnd);

                    DisplayStart = clipper->DisplayStart;
                    DisplayEnd = clipper->DisplayEnd;
                    if(DisplayStart > 0)
                        //SetCursorPosYAndSetupDummyPrevLine(StartPosY + DisplayStart * ItemsHeight, ItemsHeight); // advance cursor
                        ImGuiNative.igSetCursorPosY(StartPosY + DisplayStart * ItemsHeight);
                    StepNo = 2;
                }
            }
        }
    }
}

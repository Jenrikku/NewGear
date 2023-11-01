using ImGuiNET;
using NewGear.Configuration;
using Silk.NET.Input;
using System.Reflection;

using Timer = System.Timers.Timer;

namespace NewGear.GUI;

internal class ShortcutHandler
{
    private static readonly FieldInfo[] _availableShortcuts =
        typeof(ShortcutsConfiguration).GetFields();

    private static readonly Dictionary<(ImGuiModFlags Mods, Key Key), Action> _shortcuts = new();

    private static readonly Timer _timer = new() { AutoReset = true };
    private static bool _isTimerElapsed = true;

    static ShortcutHandler()
    {
        _timer.Interval = ConfigManager.Values.ShortcutCooldown;

        _timer.Elapsed += (o, e) =>
        {
            _isTimerElapsed = true;
        };
    }

    public static void SetShorcutAction(Shortcuts shortcut, Action action)
    {
        string keyString =
            ((string?)_availableShortcuts[(int)shortcut].GetValue(ConfigManager.Values.Shortcuts))
            ?? string.Empty;

        ImGuiModFlags mods = ImGuiModFlags.None;
        Key key = Key.Unknown;

        foreach (string entry in keyString.Split('+'))
            if (Enum.TryParse(entry, true, out ImGuiModFlags mod))
                mods |= mod;
            else if (Enum.TryParse(entry, true, out Key result))
                key = result;
            else
                return;

        if (mods == ImGuiModFlags.None || key == Key.Unknown)
            return;

        _shortcuts.Add((mods, key), action);
    }

    public static void ExecuteShortcuts(IKeyboard keyboard)
    {
        if (!_isTimerElapsed || ImGui.GetIO().WantCaptureKeyboard)
            return;

        ImGuiModFlags mods = (ImGuiModFlags)(
            Convert.ToInt16(ImGui.IsKeyDown(ImGuiKey.ModCtrl))
            + Convert.ToInt16(ImGui.IsKeyDown(ImGuiKey.ModShift)) * 0b10
            + Convert.ToInt16(ImGui.IsKeyDown(ImGuiKey.ModAlt)) * 0b100
        );

        if (mods == ImGuiModFlags.None)
            return;

        foreach (var entry in _shortcuts)
        {
            if (entry.Key.Mods != mods || !keyboard.IsKeyPressed(entry.Key.Key))
                continue;

            // Execute shortcut:
            entry.Value.Invoke();

            // Run timer (cooldown):
            _isTimerElapsed = false;
            _timer.Start();
        }
    }
}

internal enum Shortcuts : int
{
    OpenFile,
    SaveFile,
    SaveFileAs,
    CloseFile,
    ExitProgram
}

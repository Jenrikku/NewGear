namespace NewGear.Configuration;

internal struct Configuration {

    public Configuration() { }

    public ShortcutsConfiguration Shortcuts = new();

    public double ShortcutCooldown = 500;
}

public struct ShortcutsConfiguration {

    public ShortcutsConfiguration() { }

    public string FileOpen = "Ctrl+O";
    public string FileSave = "Ctrl+S";
    public string FileSaveAs = "Ctrl+Shift+S";
    public string FileClose = string.Empty;
    public string ExitProgram = "Ctrl+Q";
}
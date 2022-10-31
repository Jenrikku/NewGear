using NewGear.GearSystem.GearLoading;
using NewGear.Gears.Containers;
using NewGear.GearSystem.Enums;

public class Manifest : GearManifest {
    public override string Name => "NARC";
    public override string Description => "A simple gear for editing NARC files.";
    public override string[] Authors => new string[] { "Jenrikku (JkKU)", "Trippixyz" };
    public new string[] OriginalSources => new string[] { "https://github.com/Jenrikku/NARCSharp" };

    public override GearEntry[] Entries => new GearEntry[] {
        new(typeof(NARC)) {
            ContextMenu = DefaultContextMenus.ContainerMenu,
            DefaultEditor = EditorEntries.FileTree,
            Identify = DefaultIdentifyMethods.IdentifyByMagic("NARC")
        }
    };
}

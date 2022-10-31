using NewGear.GearSystem.GearLoading;
using NewGear.Gears.Compression;
using NewGear.GearSystem.Enums;

public class Manifest : GearManifest {
    public override string Name => "Yaz0";
    public override string Description => "A simple gear for handling Yaz0-compressed files.";
    public override string[] Authors => new string[] { "Jenrikku (JkKU)", "Gericom (EveryFileExplorer)", "Hackio" };
    public new string[] OriginalSources => new string[] { "https://github.com/SuperHackio/Hack.io/blob/master/Hack.io.YAZ0/YAZ0.cs",
                                                          "https://github.com/Gericom/EveryFileExplorer/blob/master/CommonCompressors/YAZ0.cs" };

    public override GearEntry[] Entries => new GearEntry[] {
        new(typeof(Yaz0)) {
            Identify = DefaultIdentifyMethods.IdentifyByMagic("Yaz0")
        }
    };
}

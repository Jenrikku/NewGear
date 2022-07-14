using NewGear.Gears.Compression;
using NewGear.Gears.Containers;

NARC narc = new();
Yaz0 yaz0 = new();

narc.Read(yaz0.Decompress(@"M:\Modding\3DL\resources\romfs\ObjectData\AssistItem.szs"));

yaz0.Compress(narc.Write());

Console.WriteLine();

//byte[] filebuffer = File.ReadAllBytes(@"M:\Modding\3DL\temp\AquariumSwimStageDesign1.narc");
//Console.WriteLine(narc.Identify(filebuffer));

//narc.Write(@"M:\Modding\3DL\temp\Signora.narc");

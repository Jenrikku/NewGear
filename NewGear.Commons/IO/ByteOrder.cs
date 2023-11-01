namespace NewGear.Commons.IO
{
    public enum ByteOrder : ushort
    {
        Unspecified,
        LittleEndian = 0xFEFF,
        BigEndian = 0xFFFE
    }
}

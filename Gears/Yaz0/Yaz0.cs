using NewGear.GearSystem.AbstractGears;
using Syroot.BinaryData;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

// Code based of Hack.IO (https://github.com/SuperHackio/Hack.io/blob/master/Hack.io.Yaz0/Yaz0.cs)
// and EveryFileExplorer (https://github.com/Gericom/EveryFileExplorer/blob/master/CommonCompressors/Yaz0.cs)
namespace NewGear.Gears.Compression {
    public class Yaz0 : CompressionGear {
        public Yaz0() {
            Level = 5;
        }

        /// <returns>Whether or not the given data matches this compression method's specifications.</returns>
        public static new bool Identify(byte[] data) {
            return Encoding.ASCII.GetString(data[0..4]) == "Yaz0";
        }

        /// <summary>
        /// Compresses a byte array.
        /// </summary>
        public unsafe new byte[] Compress(byte[] data) {
            int maxBackLevel = Level is null ? 256 : (int) (0x10e0 * (Level / 9.0) - 0x0e0);

            byte* dataptr = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);

            byte[] result = new byte[data.Length + data.Length / 8 + 0x10];
            byte* resultptr = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(result, 0);
            *resultptr++ = (byte) 'Y';
            *resultptr++ = (byte) 'a';
            *resultptr++ = (byte) 'z';
            *resultptr++ = (byte) '0';
            *resultptr++ = (byte) ((data.Length >> 24) & 0xFF);
            *resultptr++ = (byte) ((data.Length >> 16) & 0xFF);
            *resultptr++ = (byte) ((data.Length >> 8) & 0xFF);
            *resultptr++ = (byte) ((data.Length >> 0) & 0xFF);
            {
                var res1 = Reserved is null ? new byte[] { 0, 0, 0, 0 } : Reserved[0..4];
                var res2 = Reserved is null ? new byte[] { 0, 0, 0, 0 } : Reserved[4..8];
                *resultptr++ = res1[0];
                *resultptr++ = res1[1];
                *resultptr++ = res1[2];
                *resultptr++ = res1[3];
                *resultptr++ = res2[0];
                *resultptr++ = res2[1];
                *resultptr++ = res2[2];
                *resultptr++ = res2[3];
            }
            int length = data.Length;
            int dstoffs = 16;
            int Offs = 0;
            while(true) {
                int headeroffs = dstoffs++;
                resultptr++;
                byte header = 0;
                for(int i = 0; i < 8; i++) {
                    int comp = 0;
                    int back = 1;
                    int nr = 2;
                    {
                        byte* ptr = dataptr - 1;
                        int maxnum = 0x111;
                        if(length - Offs < maxnum) maxnum = length - Offs;
                        //Use a smaller amount of bytes back to decrease time
                        int maxback = maxBackLevel;//0x1000;
                        if(Offs < maxback) maxback = Offs;
                        maxback = (int) dataptr - maxback;
                        int tmpnr;
                        while(maxback <= (int) ptr) {
                            if(*(ushort*) ptr == *(ushort*) dataptr && ptr[2] == dataptr[2]) {
                                tmpnr = 3;
                                while(tmpnr < maxnum && ptr[tmpnr] == dataptr[tmpnr]) tmpnr++;
                                if(tmpnr > nr) {
                                    if(Offs + tmpnr > length) {
                                        nr = length - Offs;
                                        back = (int) (dataptr - ptr);
                                        break;
                                    }
                                    nr = tmpnr;
                                    back = (int) (dataptr - ptr);
                                    if(nr == maxnum) break;
                                }
                            }
                            --ptr;
                        }
                    }
                    if(nr > 2) {
                        Offs += nr;
                        dataptr += nr;
                        if(nr >= 0x12) {
                            *resultptr++ = (byte) (((back - 1) >> 8) & 0xF);
                            *resultptr++ = (byte) ((back - 1) & 0xFF);
                            *resultptr++ = (byte) ((nr - 0x12) & 0xFF);
                            dstoffs += 3;
                        } else {
                            *resultptr++ = (byte) ((((back - 1) >> 8) & 0xF) | (((nr - 2) & 0xF) << 4));
                            *resultptr++ = (byte) ((back - 1) & 0xFF);
                            dstoffs += 2;
                        }
                        comp = 1;
                    } else {
                        *resultptr++ = *dataptr++;
                        dstoffs++;
                        Offs++;
                    }
                    header = (byte) ((header << 1) | ((comp == 1) ? 0 : 1));
                    if(Offs >= length) {
                        header = (byte) (header << (7 - i));
                        break;
                    }
                }
                result[headeroffs] = header;
                if(Offs >= length) break;
            }
            while((dstoffs % 4) != 0) dstoffs++;
            byte[] realresult = new byte[dstoffs];
            Array.Copy(result, realresult, dstoffs);
            return realresult;
        }

        public unsafe override byte[] Compress(Stream stream, bool leaveOpen = false) {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            if(!leaveOpen)
                stream.Close();

            return Compress(buffer);
            
            // WIP -------------------------------

            //MemoryStream result = new();

            //using BinaryDataReader reader = new(stream, Encoding.Default, leaveOpen);
            //using BinaryDataWriter writer = new(result, Encoding.Default, leaveOpen);

            //int maxBackLevel = Level is null ? 256 : (int) (0x10E0 * (Level / 9.0F) - 0x00E0);

            //reader.ByteOrder = ByteOrder.BigEndian;

            //writer.Write("Yaz0", BinaryStringFormat.NoPrefixOrTermination); // Magic

            //uint length = (uint) stream.Length;
            //writer.Write(length); // Uncompressed file size.

            //writer.Write(Reserved ?? new byte[8]); // Reserved numbers.

            //int offs = 0;
            //while(true) {
            //    // int headerOffset = writer.Position - 16;
            //    byte header = 0;
            //    for(int i = 0; i < 8; i++) {
            //        int comp = 0;
            //        int back = 1;
            //        int nr = 2;

            //        int ptr = (int) reader.Position - 1;

            //        int maxnum = 0x111;
            //        if(length - offs < maxnum)
            //            maxnum = (int) length - offs;

            //        int maxback = maxBackLevel;
            //        if(offs < maxback)
            //            maxback = offs;
            //        maxback = (int) reader.Position - maxback;

            //        int tmpnr;
            //        while(maxback <= ptr) {

            //        }
            //    }
            //}

            //return null;
        }

        public override byte[] Decompress(Stream stream, bool leaveOpen = false) {
            MemoryStream result = new();

            using BinaryDataReader reader = new(stream, Encoding.Default, leaveOpen);
            using BinaryDataWriter writer = new(result, Encoding.Default, leaveOpen);

            if(reader.ReadString(4) != "Yaz0")
                throw new InvalidDataException("The given file has not been compressed using Yaz0.");

            reader.ByteOrder = ByteOrder.BigEndian;

            uint requiredSize = reader.ReadUInt32(); // Uncompressed file size.
            Reserved = reader.ReadBytes(8);

            while(result.Position < requiredSize) {
                BitArray flags = new(reader.ReadBytes(1));

                for(int i = 7; i > -1 && (result.Position < requiredSize); i--) {
                    if(flags[i]) // The byte is stored directly.
                        writer.Write(reader.ReadByte());
                    else {       // A pointer to the repeated chain.
                        byte current = reader.ReadByte();
                        int offset = (((byte) (current & 0x0F) << 8) | reader.ReadByte()) + 1,
                            length = (current & 0xF0) == 0 ? reader.ReadByte() + 0x12 : (byte) ((current & 0xF0) >> 4) + 2;

                        for(int j = 0; j < length; j++) {
                            byte repeated;
                            using(writer.TemporarySeek()) {
                                writer.BaseStream.Position -= offset;
                                repeated = (byte) writer.BaseStream.ReadByte();
                            }
                            writer.Write(repeated);
                        }
                    }
                }
            }

            return result.ToArray();
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;

namespace NewGear.IO {
    public unsafe class BinaryStream : IDisposable {
        private readonly List<GCHandle> handleList = new();

        private byte* baseAddress;
        private byte* position;
        private byte* endAddress;

        /// <summary>
        /// Creates an empty stream.
        /// </summary>
        public BinaryStream() : this(Array.Empty<byte>()) { }

        /// <summary>
        /// Creates a stream from an existing array.
        /// </summary>
        public BinaryStream(byte[] buffer) {
            if(buffer.Length == 0)
                Array.Clear(buffer, 0, 1);

            handleList.Add(GCHandle.Alloc(buffer, GCHandleType.Pinned));

            baseAddress = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            endAddress = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buffer, buffer.Length - 1);
        }

        public long Position { 
            get => position - baseAddress;
            set {
                if(Length < value) { // If the region needs to be extended. (Not tested)
                    byte[] newBytes = new byte[value - Length];
                    Array.Fill(newBytes, FillingNumber);

                    Marshal.Copy(newBytes, 0, (nint) (++endAddress), newBytes.Length);
                    handleList.Add(GCHandle.FromIntPtr((nint) endAddress));

                    endAddress += newBytes.Length;
                }
                
                position = baseAddress + value;
            }
        }

        public long Length => endAddress - baseAddress;

        /// <summary>
        /// Specifies the number that will be written to all empty new spaces.
        /// </summary>
        public byte FillingNumber { get; set; } = 0x00;

        /// <summary>
        /// Specifies the order of the bytes in relation to numerical storing.
        /// </summary>
        public ByteOrder ByteOrder { get; set; } = default;

        public Encoding DefaultEncoding { get; set; } = Encoding.ASCII;


        // Reading --------------------

        public bool ReadBool() => *position++ == 1;

        public byte ReadByte() => *position++;
        public sbyte ReadSByte() => *(sbyte*) position++;

        public short ReadInt16() => (short) ReadNumericValue(2);
        public ushort ReadUInt16() => (ushort) ReadNumericValue(2);

        public float ReadFloat() => ReadNumericValue(4);
        public int ReadInt32() => (int) ReadNumericValue(4);
        public uint ReadUInt32() => (uint) ReadNumericValue(4);

        public decimal ReadDecimal() => ReadNumericValue(8);
        public long ReadInt64() => (long) ReadNumericValue(8);
        public ulong ReadUInt64() => ReadNumericValue(8);

        public char ReadChar() => ReadChar(1);
        public char ReadChar(byte byteAmount) => (char) ReadNumericValue(byteAmount);
        public char ReadChar(Encoding encoding) => (char) ReadNumericValue((byte) encoding.GetMaxByteCount(1));

        public string ReadString(int length) => ReadString(length, DefaultEncoding);
        public string ReadString(int length, Encoding encoding) {
            string result = encoding.GetString(position, length);
            position += length;

            return result;
        }

        /// <summary>
        /// Reads a string until a byte matches the given argument. 0 by default.
        /// </summary>
        /// <param name="endValue">The byte that will make the reader stop.</param>
        public string ReadStringUntil(byte endValue = 0x00) => ReadStringUntil(DefaultEncoding, endValue);
        /// <summary>
        /// Reads a string until a byte matches the given argument with a set encoding.
        /// </summary>
        /// <param name="endValue">The byte that will make the reader stop.</param>
        public string ReadStringUntil(Encoding encoding, byte endValue = 0x00) {
            byte* beginning = position;
            int length = 0;

            while(*position++ != endValue)
                length++;

            return encoding.GetString(beginning, length);
        }


        // Others ---------------------

        public SeekTask TemporarySeek() => new(this);

        public void Dispose() {
            GC.SuppressFinalize(this);
            handleList.ForEach((GCHandle handle) => handle.Free());
        }


        // Private --------------------

        private ulong ReadNumericValue(byte byteAmount) {
            ulong result = 0;

            for(byte i = 0; i < byteAmount; i++)
                if(BitConverter.IsLittleEndian == (ByteOrder == ByteOrder.LittleEndian)) // ByteOrder matches.
                    result += (ulong) *position++ << 8 * i;
                else
                    result += (ulong) *position++ >> 8 * (byteAmount - 1) - 8 * i;

            return result;
        }
    }

    public enum ByteOrder : ushort {
        LittleEndian = 0xFEFF,
        BigEndian = 0xFFFE
    }

    public unsafe struct SeekTask : IDisposable {
        private BinaryStream stream;
        private long position;

        public SeekTask(BinaryStream stream) {
            this.stream = stream;
            position = stream.Position;
        }

        /// <summary>
        /// Returns the stream to the position it was when this instance was created.
        /// </summary>
        public void Dispose() => stream.Position = position;
    }
}

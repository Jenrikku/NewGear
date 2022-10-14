using System.Runtime.InteropServices;
using System.Text;

namespace NewGear.IO {
    public unsafe class BinaryStream : IDisposable {
        private readonly List<GCHandle> handleList = new();

        private byte* baseAddress;
        private byte* endAddress;

        private byte* _position;
        private byte* position {
            get => _position;
            set {
                if(value > endAddress)
                    Position += value - baseAddress;
                else
                    _position = value;
            }
        }

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

        /// <summary>
        /// The position of the stream, next value will be read from this position.
        /// </summary>
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

        /// <summary>
        /// Calculates the length of the stream.
        /// </summary>
        public long Length => endAddress - baseAddress;

        /// <summary>
        /// Specifies the number that will be written to all empty new spaces.
        /// </summary>
        public byte FillingNumber { get; set; } = 0x00;

        /// <summary>
        /// Specifies the order of the bytes in relation to numerical storing.
        /// </summary>
        public ByteOrder ByteOrder { get; set; } = default;

        /// <summary>
        /// The default <see cref="Encoding"/> that will be used when reading strings if none is specified.
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.ASCII;


        // Reading --------------------

        // Regular values -------------

        /// <summary>
        /// Reads a <see cref="bool"/> and advances the position of the stream.
        /// </summary>
        public bool ReadBool() => *position++ == 1;

        /// <summary>
        /// Reads a <see cref="byte"/> and advances the position of the stream.
        /// </summary>
        public byte ReadByte() => *position++;
        /// <summary>
        /// Reads a <see cref="sbyte"/> and advances the position of the stream.
        /// </summary>
        public sbyte ReadSByte() => *(sbyte*) position++;

        /// <summary>
        /// Reads a <see cref="short"/> and advances the position of the stream.
        /// </summary>
        public short ReadInt16() => (short) ReadNumericValue(2);
        /// <summary>
        /// Reads an <see cref="ushort"/> and advances the position of the stream.
        /// </summary>
        public ushort ReadUInt16() => (ushort) ReadNumericValue(2);

        /// <summary>
        /// Reads a <see cref="float"/> and advances the position of the stream.
        /// </summary>
        public float ReadFloat() => ReadNumericValue(4);
        /// <summary>
        /// Reads an <see cref="int"/> and advances the position of the stream.
        /// </summary>
        public int ReadInt32() => (int) ReadNumericValue(4);
        /// <summary>
        /// Reads an <see cref="uint"/> and advances the position of the stream.
        /// </summary>
        public uint ReadUInt32() => (uint) ReadNumericValue(4);

        /// <summary>
        /// Reads a <see cref="decimal"/> and advances the position of the stream.
        /// </summary>
        public decimal ReadDecimal() => ReadNumericValue(8);
        /// <summary>
        /// Reads a <see cref="double"/> and advances the position of the stream.
        /// </summary>
        public double ReadDouble() => ReadNumericValue(8);
        /// <summary>
        /// Reads a <see cref="long"/> and advances the position of the stream.
        /// </summary>
        public long ReadInt64() => (long) ReadNumericValue(8);
        /// <summary>
        /// Reads an <see cref="ulong"/> and advances the position of the stream.
        /// </summary>
        public ulong ReadUInt64() => ReadNumericValue(8);

        /// <summary>
        /// Reads a <see cref="char"/> and advances the position of the stream.
        /// </summary>
        public char ReadChar() => ReadChar(1);
        /// <summary>
        /// Reads a <see cref="char"/> with a set length and advances the position of the stream.
        /// </summary>
        public char ReadChar(byte byteLength) => (char) ReadNumericValue(byteLength);
        /// <summary>
        /// Reads a <see cref="char"/> which has its length taken from the <see cref="Encoding"/> and advances the position of the stream.
        /// </summary>
        public char ReadChar(Encoding encoding) => (char) ReadNumericValue((byte) encoding.GetMaxByteCount(1));

        // Strings --------------------

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

        // Arrays ---------------------

        /// <summary>
        /// Reads an amount of bytes and converts them to bools.
        /// Note that this will not read bools from bits, use <see cref="ReadFlags(byte)"/> for that instead.
        /// </summary>
        public bool[] ReadBoolArray(int amount) => ReadArray(amount, ReadBool);

        /// <summary>
        /// Reads a <see cref="byte"/> array and advances the position of the stream.
        /// </summary>
        public byte[] ReadByteArray(int amount) => ReadArray(amount, ReadByte);
        /// <summary>
        /// Reads a <see cref="sbyte"/> array and advances the position of the stream.
        /// </summary>
        public sbyte[] ReadSByteArray(int amount) => ReadArray(amount, ReadSByte);

        /// <summary>
        /// Reads a <see cref="short"/> array and advances the position of the stream.
        /// </summary>
        public short[] ReadInt16Array(int amount) => ReadArray(amount, ReadInt16);
        /// <summary>
        /// Reads an <see cref="ushort"/> array and advances the position of the stream.
        /// </summary>
        public ushort[] ReadUInt16Array(int amount) => ReadArray(amount, ReadUInt16);

        /// <summary>
        /// Reads a <see cref="float"/> array and advances the position of the stream.
        /// </summary>
        public float[] ReadFloatArray(int amount) => ReadArray(amount, ReadFloat);
        /// <summary>
        /// Reads an <see cref="int"/> array and advances the position of the stream.
        /// </summary>
        public int[] ReadInt32Array(int amount) => ReadArray(amount, ReadInt32);
        /// <summary>
        /// Reads an <see cref="uint"/> array and advances the position of the stream.
        /// </summary>
        public uint[] ReadUInt32Array(int amount) => ReadArray(amount, ReadUInt32);

        /// <summary>
        /// Reads a <see cref="double"/> array and advances the position of the stream.
        /// </summary>
        public double[] ReadDoubleArray(int amount) => ReadArray(amount, ReadDouble);
        /// <summary>
        /// Reads a <see cref="decimal"/> array and advances the position of the stream.
        /// </summary>
        public decimal[] ReadDecimalArray(int amount) => ReadArray(amount, ReadDecimal);
        /// <summary>
        /// Reads a <see cref="long"/> array and advances the position of the stream.
        /// </summary>
        public long[] ReadInt64Array(int amount) => ReadArray(amount, ReadInt64);
        /// <summary>
        /// Reads an <see cref="ulong"/> array and advances the position of the stream.
        /// </summary>
        public ulong[] ReadUInt64Array(int amount) => ReadArray(amount, ReadUInt64);

        /// <summary>
        /// Reads a byte array and converts it into <see cref="Flags"/>.
        /// </summary>
        /// <param name="byteLength">The amount of bytes to read. Maximum 32.</param>
        public Flags ReadFlags(byte byteLength = 1) => new(ReadByteArray(byteLength));


        // Writing --------------------

        // [Code here]


        // Others ---------------------

        /// <summary>
        /// Creates a <see cref="SeekTask"/> that returns the stream to its past position once it is disposed.
        /// </summary>
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

        private T[] ReadArray<T>(int arrayLength, Func<T> numberReader) {
            T[] array = new T[arrayLength];

            for(int i = 0; i < arrayLength; i++)
                array[i] = numberReader.Invoke();

            return array;
        }
    }

    public enum ByteOrder : ushort {
        LittleEndian = 0xFEFF,
        BigEndian = 0xFFFE
    }

    public unsafe struct SeekTask : IDisposable {
        private BinaryStream stream;
        private long position;

        internal SeekTask(BinaryStream stream) {
            this.stream = stream;
            position = stream.Position;
        }

        /// <summary>
        /// Returns the stream to the position it was when this instance was created.
        /// </summary>
        public void Dispose() => stream.Position = position;
    }

    public struct Flags {
        private byte[] bytes;

        /// <summary>
        /// Gets a flag by its index.
        /// </summary>
        public bool this[byte index] {
            get {
                if(index > bytes.Length * 8)
                    return false;

                byte byteIndex = (byte) (index / 8);
                byte bitIndex = (byte) Math.Pow(2, index - 8 * byteIndex);

                return (bytes[byteIndex] & bitIndex) == bitIndex;
            }
        }

        public Flags(byte[] bytes) {
            if(bytes.Length < 1 && bytes.Length > 32)
                throw new ArgumentOutOfRangeException("The maximum amount of bytes is 32 and it cannot be 0.");

            this.bytes = bytes;
        }
    }
}

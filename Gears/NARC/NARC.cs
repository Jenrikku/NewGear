using NewGear.GearSystem.Interfaces;
using NewGear.Trees.TrueTree;
using NewGear.IO;
using System.Text;

// Check out this file's format here: https://redpepper.miraheze.org/wiki/NARC
namespace NewGear.Gears.Containers {
    public class NARC : IContainerGear, IModifiableGear {
        public BranchNode RootNode { get; set; } = new('*') { Metadata = new NARCHeader() };
        public ICompressionGear? CompressionAlgorithm { get; set; }
        public ByteOrder ByteOrder { get; set; }
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public void Read(byte[] data) {
            using BinaryStream stream = new(data) { 
                ByteOrder = ByteOrder.BigEndian,
                DefaultEncoding = Encoding
            };

            // Header ----------------

            if(stream.ReadString(4) != "NARC") // Magic check.
                throw new InvalidDataException("The given file is not a NARC.");

            // Reads the byte order and changes the one used by the stream if required:
            ByteOrder = stream.Read<ByteOrder>();
            stream.ByteOrder = ByteOrder;

            // Replaces the header:
            RootNode.Metadata = new NARCHeader() { 
                Version = stream.Read<ushort>()
            };

            stream.Position += 4; // Length skip (calculated when writing).

            // Reserved values (should be constant)
            RootNode.Metadata.Reserved0 = stream.Read<ushort>(); // 16 (Header length)
            RootNode.Metadata.Reserved1 = stream.Read<ushort>(); // 3  (Section / block count)

            // FATB ----------------

            if(stream.ReadString(4) != "BTAF") // Magic check.
                throw new InvalidDataException("The FATB section was not found. The file may be corrupted.");

            // Calculates the beginning of the next section:
            ulong fntbBeginning = stream.Position - 4 + stream.Read<uint>();

            uint fatbCount = stream.Read<uint>(); // Amount of hashes

            // startPos -> The starting position of the file.
            // endPos   -> The ending position of the file.
            // Both positions are relative to the FIMG section, right after the length.
            // Each entry in the array describes a file's boundary.
            (uint startPos, uint endPos)[] positions = new (uint, uint)[fatbCount];

            for(uint i = 0; i < fatbCount; i++) // Reads all hashes.
                positions[i] = (stream.Read<uint>(), stream.Read<uint>());

            // FNTB ----------------

            // Moves the stream to the beginning of this section:
            stream.Position = fntbBeginning;

            if(stream.ReadString(4) != "BTNF") // Magic check.
                throw new InvalidDataException("The FNTB section was not found. The file may be corrupted.");

            // Calculates the beginning of the next section:
            ulong fimgBeginning = stream.Position - 4 + stream.Read<uint>();

            ulong dirEntriesStart = stream.Position; // The start of the directory entries array. Constant.
            ulong dirEntriesPos = stream.Position;   // The current position within the directory entries array.

            ReadDirectory(RootNode);

            // Called each time a directory is found.
            // The stream's position is within the directory entries array.
            void ReadDirectory(BranchNode dir) {
                // The start position of the directory within the name array.
                ulong startPosition = dirEntriesStart + stream.Read<uint>();

                // Advances the position within the directory entries array.
                dirEntriesPos = stream.Position + 4;

                // Sets the stream's position to the directory's start point.
                stream.Position = startPosition;

                while(stream.Peek() != 0) { // 0 means that the end of the directory has been reached.
                    byte length = stream.Read<byte>(); // Names are length-prefixed.

                    if(length >= 0b10000000) { // Directory.
                        length = (byte) (length & 0b01111111); // Removes the first bit from the length.

                        // Creates a new branch node with the directory's name. 
                        BranchNode childDir = new(stream.ReadString(length));

                        using(stream.TemporarySeek()) {
                            stream.Position = dirEntriesPos; // Goes back to the directory entries array.
                            ReadDirectory(childDir); // Reads the child directory.
                        } // Return to the past position.

                        stream.Position += 2; // Skips directory ID and 0xF0 const.

                        dir.AddChild(childDir); // Adds the child to its parent.
                    } 
                    else // Reads a file and adds it directly to its parent directory.
                        dir.AddChild(new LeafNode(stream.ReadString(length)));
                }
            }

            // FIMG ----------------

            // Moves the stream to the beginning of this section:
            stream.Position = fimgBeginning;

            if(stream.ReadString(4) != "GMIF") // Magic check.
                throw new InvalidDataException("The FIMG section was not found. The file may be corrupted.");

            stream.Position += 4; // Length skip.

            uint index = 0; // The file index. Used to get the right hash from the FATB section.
            IterateDirectory(RootNode);

            // Reads a directory by iterating through all child nodes in it.
            void IterateDirectory(BranchNode dir) {
                // Because of how NARC files are often written, files have to be read before child directories.

                foreach(LeafNode file in dir.ChildLeaves) { // Reads files.
                    (uint startPos, uint endPos) = positions[index++]; // Gets the current hash.

                    using(stream.TemporarySeek()) {
                        stream.Position += startPos; // Sets the position to the file's beginning.

                        // Reads the file's contents.
                        // The size is calculated by subtracting the starting position to the end position.
                        file.Contents = stream.Read<byte>((int) (endPos - startPos));
                    } // Return to the past position.
                }

                foreach(BranchNode childDir in dir.ChildBranches) // Reads child directories.
                    IterateDirectory(childDir);
            }
        }

        public byte[] Write() {
            using BinaryStream stream = new(0xFF) {
                ByteOrder = ByteOrder,
                DefaultEncoding = Encoding
            };

            // Header ----------------

            stream.Write("NARC");                               // Magic.
            stream.Write<ushort>(0xFFFE);                       // Endian.
            stream.Write<ushort>(RootNode.Metadata?.Version);   // Version.
            
            stream.Position += 4;                               // Length skip.

            stream.Write<ushort>(RootNode.Metadata?.Reserved0); // Header length. (16)
            stream.Write<ushort>(RootNode.Metadata?.Reserved1); // Section count. (3)

            // FATB ----------------

            stream.Write("BTAF"); // Magic.

            uint count = 0; // Amount of files. (hash count)

            CountFiles(RootNode);

            // Goes thourgh all files inside the given directory and all its children:
            void CountFiles(BranchNode dir) {
                foreach(LeafNode file in dir.ChildLeaves)
                    count++;

                foreach(BranchNode childDir in dir.ChildBranches)
                    CountFiles(childDir);
            }

            stream.Write(count * 8 + 12); // Section length.

            stream.Write(count); // Hash count.

            // The current position inside the hash array. Used when writing file contents.
            uint fbatHashPos = (uint) stream.Position;

            // Reserve spaces to put hashes later:
            stream.Position += count * 8;

            // FNTB ----------------
            
            uint fntbStartPos = (uint) stream.Position; // FNTB start position. (before magic)

            stream.Write("BTNF"); // Magic.

            stream.Position += 4; // Length skip.

            uint dirEntriesStart = (uint) stream.Position;   // The start of the directory entries array. Constant.
            uint dirEntriesPos = (uint) stream.Position + 8; // The current position within the directory entries array.
            uint dirEntriesLength = 8;                       // The length of the directory entries array.

            FindChildDirectories(RootNode);
            
            // Finds all child directories in order to calculate the directory entries array:
            void FindChildDirectories(BranchNode dir) {
                foreach(BranchNode child in dir.ChildBranches) {
                    FindChildDirectories(child); // Searches for more directories inside the child.
                    dirEntriesLength += 8;
                }
            }

            // The length of the directory entries array is also the root's offset.
            stream.Write(dirEntriesLength);

            // No files before (always constant in the root directory):
            stream.Write<ushort>(0);

            // Amount of children directories:
            stream.Write((ushort) (RootNode.ChildBranches.Count + 1));

            // Reserve space for directory entries:
            stream.Position += dirEntriesLength - 8;

            // Write names:

            byte directoryID = 0;  // Increases by one each time a directory's name has been written.
            ushort fileAmount = 0; // Increases by one each time a file's name has been written.

            WriteNames(RootNode);

            // Writes all names and also goes back and writes directory entries when required:
            void WriteNames(BranchNode dir) {
                // Names:
                foreach(INode node in dir) {
                    if(node is BranchNode) { // Directory names.
                        stream.Write((byte) (node.ID.Length + 0b10000000)); // Name length. (8th bit set to 1)
                        stream.Write((string) node.ID);                     // Name.
                        stream.Write(++directoryID);                        // Directory ID.
                        stream.Write<byte>(0xF0);                           // Constant byte. (0xF0)
                    } else {                 // Filenames.
                        stream.Write((byte) node.ID.Length);                // Name length.
                        stream.Write((string) node.ID);                     // Name.

                        fileAmount++;
                    }
                }

                stream.Write<byte>(0); // End-of-directory byte.

                // Directory entries:
                foreach(BranchNode node in dir.ChildBranches) {
                    // The relative position of the current position to the beginning of the directory entries array.
                    uint nameSectionRelPos = (uint) stream.Position - dirEntriesStart;

                    using(stream.TemporarySeek()) {
                        // Sets the position to the beginning of the directory entries array.
                        stream.Position = dirEntriesPos;

                        stream.Write(nameSectionRelPos); // Offset to the directoriy's beginning inside the names array.
                        stream.Write(fileAmount);        // Amount of files present before this directory.

                        ushort childCount = (ushort) node.ChildBranches.Count; // Amount of child directories.

                        // Amount of children directories, counting the parent itself. (+1)
                        // 0xF000 if none.
                        stream.Write((ushort) (childCount == 0 ? 0xF000 : childCount + 1));

                        dirEntriesPos += 8; // Advances the current position within the directory entries array.
                    } // Return to the past position.

                    WriteNames(node); // Write the names of this directory's children.
                }
            }

            stream.Align(128); // Alignment to the FIMG section.

            using(stream.TemporarySeek()) {
                // Calculates the FNTB section's length by taking in mind its starting point:
                uint fntbLength = (uint) (stream.Position - fntbStartPos);

                stream.Position = fntbStartPos + 4; // Goes to the FNTB's length position.
                stream.Write(fntbLength);           // FNTB section's length.
            } // Return to the FIMG section's beginning.

            // FIMG ----------------

            uint fimgStartPos = (uint) stream.Position; // FIMG start position. (before magic)

            stream.Write("GMIF"); // Magic

            stream.Position += 4; // Length skip.

            WriteFileData(RootNode);

            // Writes file contents and also FATB's hashes:
            void WriteFileData(BranchNode dir) {
                foreach(LeafNode file in dir.ChildLeaves) { // Files within the directory.
                    // FATB hash values, both relatives to the position after the FIMG's length:
                    uint fileStart = (uint) stream.Position - (fimgStartPos + 8);
                    uint fileEnd;

                    stream.Write((byte[]) (file.Contents ?? Array.Empty<byte>())); // File contents.

                    fileEnd = (uint) stream.Position - (fimgStartPos + 8); // Calculates file's end.

                    using(stream.TemporarySeek()) {
                        stream.Position = fbatHashPos; // Goes to the current position within the hash array.

                        stream.Write(fileStart); // File's start. 
                        stream.Write(fileEnd);   // File's end.

                        fbatHashPos += 8; // Advances the position within the hash array.
                    } // Return to the past position.

                    stream.Align(128); // Align to the next file.
                }

                foreach(BranchNode childDir in dir.ChildBranches)
                    WriteFileData(childDir); // Files within child directories.
            }

            uint fimgLength = (uint) (stream.Position - fimgStartPos); // Calculate FIMG section's length.

            stream.Position = fimgStartPos + 4; // Goes to the FIMG's length position.
            stream.Write(fimgLength);           // FIMG's section length.

            // ----------------

            stream.Position = 8; // Goes back to the file's length position.

            stream.Write((uint) stream.Length); // Writes the file's length.

            return stream.ToArray();
        }

        public struct NARCHeader {
            public NARCHeader() {
                Version = 0x0100;
                Reserved0 = 16;
                Reserved1 = 3;
            }

            public ushort Version;
            public ushort Reserved0; // Header length.
            public ushort Reserved1; // Section (block) count.
        }
    }
}
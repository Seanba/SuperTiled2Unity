using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AseSliceChunk : AseChunk
    {
        public override ChunkType ChunkType => ChunkType.Slice;

        public uint NumSliceKeys { get; }
        public SliceFlags Flags { get; }
        public string Name { get; }

        public List<AseSliceEntry> Entries { get; }

        public AseSliceChunk(AseFrame frame, AseReader reader)
            : base(frame)
        {
            NumSliceKeys = reader.ReadDWORD();
            Flags = (SliceFlags)reader.ReadDWORD();

            // Ignore next dword
            reader.ReadDWORD();

            Name = reader.ReadSTRING();

            Entries = Enumerable.Repeat<AseSliceEntry>(null, (int)NumSliceKeys).ToList();
            for (int i = 0; i < (int)NumSliceKeys; i++)
            {
                Entries[i] = new AseSliceEntry(reader, Flags);
            }
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitSliceChunk(this);
        }
    }
}

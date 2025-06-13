using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AsePaletteChunk : AseChunk
    {
        public override ChunkType ChunkType => ChunkType.Palette;

        public int PaletteSize { get; }
        public int FirstIndex { get; }
        public int LastIndex { get; }

        public List<AsePaletteEntry> Entries { get; }

        public AsePaletteChunk(AseFrame frame, AseReader reader)
            : base(frame)
        {
            PaletteSize = (int)reader.ReadDWORD();
            FirstIndex = (int)reader.ReadDWORD();
            LastIndex = (int)reader.ReadDWORD();

            // Next 8 bytes are ignored
            reader.ReadBYTEs(8);

            Entries = Enumerable.Repeat<AsePaletteEntry>(null, LastIndex + 1).ToList();
            for (int i = FirstIndex; i <= LastIndex; i++)
            {
                Entries[i] = new AsePaletteEntry(reader);
            }
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitPaletteChunk(this);
        }
    }
}

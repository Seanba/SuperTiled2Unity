using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AseOldPaletteChunk : AseChunk
    {
        public override ChunkType ChunkType => ChunkType.OldPalette;

        public List<(byte red, byte green, byte blue)> Colors { get; }

        public AseOldPaletteChunk(AseFrame frame, AseReader reader)
            : base(frame)
        {
            int numPackets = reader.ReadWORD();

            (byte, byte, byte)[] colors = new (byte, byte, byte)[0];

            for (int i = 0; i < numPackets; i++)
            {
                int skip = reader.ReadBYTE();
                int numColors = reader.ReadBYTE();

                if (numColors == 0)
                {
                    numColors = 256;
                }

                // This is so weird
                int newSize = (skip + numColors);
                if (colors.Length < newSize)
                {
                    Array.Resize(ref colors, newSize);
                }

                for (int j = skip; j < numColors; j++)
                {
                    byte red = reader.ReadBYTE();
                    byte green = reader.ReadBYTE();
                    byte blue = reader.ReadBYTE();
                    colors[j] = (red, green, blue);
                }
            }

            Colors = new List<(byte red, byte green, byte blue)>(colors);
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitOldPaletteChunk(this);
        }
    }
}

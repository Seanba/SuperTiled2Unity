using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AseColorProfileChunk : AseChunk
    {
        public override ChunkType ChunkType => ChunkType.ColorProfile;

        public ColorProfileType ColorProfileType { get; }
        public ColorProfileFlags ColorProfileFlags { get; }
        public uint GammaFixed { get; }

        public AseColorProfileChunk(AseFrame frame, AseReader reader)
            : base(frame)
        {
            ColorProfileType = (ColorProfileType)reader.ReadWORD();
            ColorProfileFlags = (ColorProfileFlags)reader.ReadWORD();
            GammaFixed = reader.ReadDWORD();

            // Next 8 bytes are reserved
            reader.ReadBYTEs(8);

            // Do we need to do anything with color profile data?
            // For now, just keep on truckin'
            if (ColorProfileType == ColorProfileType.EmbeddedICC)
            {
                var length = (int)reader.ReadDWORD();
                reader.ReadBYTEs(length);
            }
        }

        public override void Visit(IAseVisitor visitor)
        {
        }
    }
}

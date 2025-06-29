using System.Diagnostics;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AseHeader
    {
        public uint FileSize { get; }
        public ushort MagicNumber { get; }
        public ushort NumFrames { get; }
        public ushort Width { get; }
        public ushort Height { get; }
        public ColorDepth ColorDepth { get; }
        public HeaderFlags Flags { get; }
        public ushort Speed { get; }
        public byte TransparentIndex { get; }
        public ushort NumColors { get; }
        public byte PixelWidth { get; }
        public byte PixelHeight { get; }

        public AseHeader(AseReader reader)
        {
            FileSize = reader.ReadDWORD();
            MagicNumber = reader.ReadWORD();
            NumFrames = reader.ReadWORD();
            Width = reader.ReadWORD();
            Height = reader.ReadWORD();
            ColorDepth = reader.ReadColorDepth();
            Flags = (HeaderFlags)reader.ReadDWORD();
            Speed = reader.ReadWORD();

            // Next two dwords are ignored
            reader.ReadDWORD();
            reader.ReadDWORD();

            TransparentIndex = reader.ReadBYTE();

            // Next 3 bytes are ignored
            reader.ReadBYTEs(3);

            NumColors = reader.ReadWORD();
            PixelWidth = reader.ReadBYTE();
            PixelHeight = reader.ReadBYTE();

            // Last 92 bytes are reserved for future use
            reader.ReadBYTEs(92);

            Debug.Assert(MagicNumber == 0xA5E0);
        }
    }
}

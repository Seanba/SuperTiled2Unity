namespace SuperTiled2Unity.Ase.Editor
{
    public class AseSliceEntry
    {
        public uint FrameNumber { get; }
        public int OriginX { get; }
        public int OriginY { get; }
        public uint Width { get; }
        public uint Height { get; }

        public int CenterX { get; }
        public int CenterY { get; }
        public uint CenterWidth { get; }
        public uint CenterHeight { get; }

        public int PivotX { get; }
        public int PivotY { get; }

        public AseSliceEntry(AseReader reader, SliceFlags flags)
        {
            FrameNumber = reader.ReadDWORD();
            OriginX = reader.ReadLONG();
            OriginY = reader.ReadLONG();
            Width = reader.ReadDWORD();
            Height = reader.ReadDWORD();

            if ((flags & SliceFlags.Is9PatchSlice) != 0)
            {
                CenterX = reader.ReadLONG();
                CenterY = reader.ReadLONG();
                CenterWidth = reader.ReadDWORD();
                CenterHeight = reader.ReadDWORD();
            }

            if ((flags & SliceFlags.HasPivotInformation) != 0)
            {
                PivotX = reader.ReadLONG();
                PivotY = reader.ReadLONG();
            }
        }
    }
}

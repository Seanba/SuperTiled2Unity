namespace SuperTiled2Unity.Ase.Editor
{
    public class AseLayerChunk : AseChunk
    {
        public override ChunkType ChunkType => ChunkType.Layer;

        public LayerChunkFlags Flags { get; }
        public LayerType LayerType { get; }
        public ushort ChildLevel { get; }
        public BlendMode BlendMode { get; }
        public byte Opacity { get; }
        public string Name { get; }
        public int TilesetIndex { get; }
        public byte[] UUID { get; }

        public bool IsVisible => (Flags & LayerChunkFlags.Visible) != 0;
        public bool IsLockMovement => (Flags & LayerChunkFlags.LockMovement) != 0;

        public AseLayerChunk(AseFrame frame, AseReader reader)
            : base(frame)
        {
            Flags = (LayerChunkFlags)reader.ReadWORD();
            LayerType = (LayerType)reader.ReadWORD();
            ChildLevel = reader.ReadWORD();

            // Ignore next two words
            reader.ReadWORD();
            reader.ReadWORD();

            BlendMode = (BlendMode)reader.ReadWORD();
            Opacity = reader.ReadBYTE();

            if ((frame.AseFile.Header.Flags & HeaderFlags.HasLayerOpacity) == 0)
            {
                // Assume full opacity
                Opacity = 255;
            }

            // Ignore next three bytes
            reader.ReadBYTEs(3);

            Name = reader.ReadSTRING();

            if (LayerType == LayerType.Tilemap)
            {
                TilesetIndex = (int)reader.ReadDWORD();
            }
            else
            {
                TilesetIndex = -1;
            }

            if (IsLockMovement)
            {
                UUID = reader.ReadBYTEs(16);
            }
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitLayerChunk(this);
        }
    }
}

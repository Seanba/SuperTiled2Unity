namespace SuperTiled2Unity.Ase.Editor
{
    // For chunks that are ignored (but still advance read pointer)
    public class AseDummyChunk : AseChunk
    {
        public override ChunkType ChunkType { get; }

        public int ChunkSize { get; }
        public byte[] Bytes { get; }

        public AseDummyChunk(AseFrame frame, AseReader reader, ChunkType type, int size) : base(frame)
        {
            ChunkType = type;
            ChunkSize = size;
            Bytes = reader.ReadBYTEs(size);
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitDummyChunk(this);
        }
    }
}

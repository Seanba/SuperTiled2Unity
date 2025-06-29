namespace SuperTiled2Unity.Ase.Editor
{
    public abstract class AseChunk
    {
        public abstract ChunkType ChunkType { get; }

        public AseFrame Frame { get; }
        public string UserDataText { get; set; }
        public byte[] UserDataColor { get; set; }

        protected AseChunk(AseFrame frame)
        {
            Frame = frame;
        }

        public abstract void Visit(IAseVisitor visitor);
    }
}

namespace SuperTiled2Unity.Ase.Editor
{
    public interface IAseVisitor
    {
        void BeginFileVisit(AseFile file);
        void EndFileVisit(AseFile file);

        void BeginFrameVisit(AseFrame frame);
        void EndFrameVisit(AseFrame frame);

        void VisitLayerChunk(AseLayerChunk layer);
        void VisitCelChunk(AseCelChunk cel);
        void VisitFrameTagsChunk(AseFrameTagsChunk frameTags);
        void VisitPaletteChunk(AsePaletteChunk palette);
        void VisitOldPaletteChunk(AseOldPaletteChunk palette);
        void VisitUserDataChunk(AseUserDataChunk userData);
        void VisitSliceChunk(AseSliceChunk slice);
        void VisitTilesetChunk(AseTilesetChunk tileset);
        void VisitDummyChunk(AseDummyChunk dummy);
    }
}

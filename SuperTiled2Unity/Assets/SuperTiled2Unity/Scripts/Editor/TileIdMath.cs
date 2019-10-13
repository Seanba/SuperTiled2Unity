namespace SuperTiled2Unity.Editor
{
    // Helper struct to deal with Tile Id values having burned-in flip flags
    public struct TileIdMath
    {
        // Flip flags from tiled
        private const uint TiledHexagonal120Flag = 0x10000000;
        private const uint TiledDiagonalFlipFlag = 0x20000000;
        private const uint TiledVerticalFlipFlag = 0x40000000;
        private const uint TiledHorizontalFlipFlag = 0x80000000;

        private uint m_ImportedTileId;
        private FlipFlags m_FlipFlags;

        public TileIdMath(uint importedTileId)
        {
            m_ImportedTileId = importedTileId;

            m_FlipFlags = 0;
            m_FlipFlags |= HasHorizontalFlip ? FlipFlags.Horizontal : 0;
            m_FlipFlags |= HasVerticalFlip ? FlipFlags.Vertical : 0;
            m_FlipFlags |= HasDiagonalFlip ? FlipFlags.Diagonal : 0;
            m_FlipFlags |= HasHexagonal120Flip ? FlipFlags.Hexagonal120 : 0;
        }

        // The tileId with baked in flip flags
        public uint ImportedlTileId { get { return m_ImportedTileId; } }

        // Just the raw tileId (no flip flags)
        public int JustTileId
        {
            get { return (int)(m_ImportedTileId & ~(TiledHorizontalFlipFlag | TiledVerticalFlipFlag | TiledDiagonalFlipFlag | TiledHexagonal120Flag)); }
        }

        public FlipFlags FlipFlags { get { return m_FlipFlags; } }

        public bool HasHorizontalFlip
        {
            get { return (m_ImportedTileId & TiledHorizontalFlipFlag) != 0; }
        }

        public bool HasVerticalFlip
        {
            get { return (m_ImportedTileId & TiledVerticalFlipFlag) != 0; }
        }

        public bool HasDiagonalFlip
        {
            get { return (m_ImportedTileId & TiledDiagonalFlipFlag) != 0; }
        }

        public bool HasHexagonal120Flip
        {
            get { return (m_ImportedTileId & TiledHexagonal120Flag) != 0; }
        }

        public bool HasFlip
        {
            get { return HasHorizontalFlip || HasVerticalFlip || HasDiagonalFlip || HasHexagonal120Flip; }
        }
    }
}

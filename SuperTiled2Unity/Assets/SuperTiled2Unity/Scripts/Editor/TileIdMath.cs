using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    // Helper struct to deal with Tile Id values having burned-in flip flags
    public struct TileIdMath
    {
        // Flip flags from tiled
        private const uint TiledDiagonalFlipFlag = 0x20000000;
        private const uint TiledVerticalFlipFlag = 0x40000000;
        private const uint TiledHorizontalFlipFlag = 0x80000000;

        // Placement flip flags that get baked into z position of placed tile
        private const int PlacementDiagonalFlipFlag = 0x00000001;
        private const int PlacementVerticalFlipFlag = 0x00000002;
        private const int PlacementHorizontalFlipFlag = 0x00000004;

        private uint m_ImportedTileId;
        private int m_PlacementZ;

        public TileIdMath(uint importedTileId)
        {
            m_ImportedTileId = importedTileId;

            m_PlacementZ = 0;
            m_PlacementZ |= HasHorizontalFlip ? PlacementHorizontalFlipFlag : 0;
            m_PlacementZ |= HasVerticalFlip ? PlacementVerticalFlipFlag : 0;
            m_PlacementZ |= HasDiagonalFlip ? PlacementDiagonalFlipFlag : 0;
        }

        // The tileId with baked in flip flags
        public uint ImportedlTileId { get { return m_ImportedTileId; } }

        // Just the raw tileId (now flip flags)
        public int JustTileId
        {
            get { return (int)(m_ImportedTileId & ~(TiledHorizontalFlipFlag | TiledVerticalFlipFlag | TiledDiagonalFlipFlag)); }
        }

        // The z-component of tile placement
        public int PlacementZ { get { return m_PlacementZ; } }

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

        public bool HasFlip
        {
            get { return HasHorizontalFlip || HasVerticalFlip || HasDiagonalFlip; }
        }
    }
}

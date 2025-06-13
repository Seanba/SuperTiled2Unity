using System;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AseTilesetChunk : AseChunk
    {
        [Flags]
        public enum TilesetFlags : uint
        {
            IncludeLinkToExternalFile = 1,
            IncludeTilesInsideThisFile = 2,
            UseTiledIdZeroAsEmpty = 4,
            AutoMatchModifiedTilesXFlip = 8,
            AutoMatchModifiedTilesYFlip = 16,
            AutoMatchModifiedTilesDiagonalFlip = 32,
        }

        public override ChunkType ChunkType => ChunkType.Tileset;

        public uint TilesetId { get; }
        public TilesetFlags Flags { get; }
        public int NumberOfTiles { get; }
        public ushort TileWidth { get; }
        public ushort TileHeight { get; }
        public short BaseIndex { get; }
        public string TilesetName { get; }

        public byte[] PixelBytes { get; }

        public AseTilesetChunk(AseFrame frame, AseReader reader) : base(frame)
        {
            TilesetId = reader.ReadDWORD();
            Flags = (TilesetFlags)reader.ReadDWORD();
            NumberOfTiles = (int)reader.ReadDWORD();
            TileWidth = reader.ReadWORD();
            TileHeight = reader.ReadWORD();
            BaseIndex = reader.ReadSHORT();

            // Reserved
            reader.ReadBYTEs(14);

            TilesetName = reader.ReadSTRING();

            if (Flags.HasFlag(TilesetFlags.IncludeLinkToExternalFile))
            {
                reader.ReadDWORD(); // Id of external file
                reader.ReadDWORD(); // tileset id in the external file
            }

            if (Flags.HasFlag(TilesetFlags.IncludeTilesInsideThisFile))
            {
                var dataLength = reader.ReadDWORD();
                var compressed = reader.ReadBYTEs((int)dataLength);

                // tile width * tile height * number of tiles
                PixelBytes = AseCelChunk.ZlibDeflate(compressed);
            }
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitTilesetChunk(this);
        }
    }
}

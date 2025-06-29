using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SuperTiled2Unity.Ase.Editor
{
    public class AseCelChunk : AseChunk
    {
        public override ChunkType ChunkType => ChunkType.Cel;

        public ushort LayerIndex { get; }
        public short PositionX { get; }
        public short PositionY { get; }
        public byte Opacity { get; }
        public CelType CelType { get; }
        public short ZIndex { get; }

        public ushort Width { get; }
        public ushort Height { get; }
        public ushort FramePositionLink { get; }
        public byte[] PixelBytes { get; }

        public AseCelChunk LinkedCel { get; }

        public ushort NumberOfTilesWide { get; }
        public ushort NumberOfTilesHigh { get; }
        public ushort BitsPerTile { get; }
        public uint BitmaskForTileId { get; }
        public uint BitmaskForXFlip { get; }
        public uint BitmaskForYFlip { get; }
        public uint BitmaskForDiagonalFlip { get; }
        public byte[] TileBytes { get; }
        public uint[] TileData32 { get; }

        public AseCelChunk(AseFrame frame, AseReader reader, int size)
            : base(frame)
        {
            // Keep track of read position
            var pos = reader.Position;

            LayerIndex = reader.ReadWORD();
            PositionX = reader.ReadSHORT();
            PositionY = reader.ReadSHORT();
            Opacity = reader.ReadBYTE();
            CelType = (CelType)reader.ReadWORD();
            ZIndex = reader.ReadSHORT();

            // Ignore next 5 bytes
            reader.ReadBYTEs(5);

            if (CelType == CelType.Raw)
            {
                Width = reader.ReadWORD();
                Height = reader.ReadWORD();

                var bytesRead = reader.Position - pos;
                PixelBytes = reader.ReadBYTEs(size - bytesRead);
            }
            else if (CelType == CelType.Linked)
            {
                FramePositionLink = reader.ReadWORD();

                // Get a reference to our linked cell. It should be in a previous frame with a matching layer index.
                Debug.Assert(Frame.AseFile.Frames.Count > FramePositionLink);
                LinkedCel = Frame.AseFile.Frames[FramePositionLink].Chunks.OfType<AseCelChunk>().FirstOrDefault(c => c.LayerIndex == LayerIndex);
                Debug.Assert(LinkedCel != null);
            }
            else if (CelType == CelType.CompressedImage)
            {
                Width = reader.ReadWORD();
                Height = reader.ReadWORD();

                var bytesRead = reader.Position - pos;
                var compressed = reader.ReadBYTEs(size - bytesRead);
                PixelBytes = ZlibDeflate(compressed);
            }
            else if (CelType == CelType.CompressedTilemap)
            {
                NumberOfTilesWide = reader.ReadWORD();
                NumberOfTilesHigh = reader.ReadWORD();
                BitsPerTile = reader.ReadWORD();

                BitmaskForTileId = reader.ReadDWORD();
                BitmaskForXFlip = reader.ReadDWORD();
                BitmaskForYFlip = reader.ReadDWORD();
                BitmaskForDiagonalFlip = reader.ReadDWORD();

                // Reserved
                reader.ReadBYTEs(10);

                // Tilemap data is compressed and needs to be deflated
                var bytesRead = reader.Position - pos;
                var compressed = reader.ReadBYTEs(size - bytesRead);
                TileBytes = ZlibDeflate(compressed);

                // Take for granted that BitsPerTile is always 32 for now
                TileData32 = new uint[TileBytes.Length / 4];
                Buffer.BlockCopy(TileBytes, 0, TileData32, 0, TileBytes.Length);
            }
        }

        public override void Visit(IAseVisitor visitor)
        {
            visitor.VisitCelChunk(this);
        }

        public static byte[] ZlibDeflate(byte[] bytesCompressed) // Todo seanba: put this into helper class
        {
            var streamCompressed = new MemoryStream(bytesCompressed);

            // Nasty trick: Have to read past the zlib stream header
            streamCompressed.ReadByte();
            streamCompressed.ReadByte();

            // Now, decompress the bytes
            using (MemoryStream streamDecompressed = new MemoryStream())
            using (DeflateStream deflateStream = new DeflateStream(streamCompressed, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(streamDecompressed);
                return streamDecompressed.ToArray();
            }
        }
    }
}

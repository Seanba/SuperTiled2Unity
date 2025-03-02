using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    // Interface for resolving assets (textures, sprites, tiles) for importing tilesets
    internal abstract class TilesetAssetResolver
    {
        public string SourceAssetPath { get; }
        public TiledAssetImporter TiledAssetImporter { get; }
        public SuperTileset SuperTileset { get; }

        public int ExpectedWidth { get; private set; }
        public int ExpectedHeight { get; private set; }

        public int InternalId { get; set; }
        public Tile.ColliderType ColliderType { get; set; }

        public TilesetAssetResolver(string sourceAssetPath, TiledAssetImporter tiledAssetImporter, SuperTileset superTileset)
        {
            SourceAssetPath = sourceAssetPath;
            TiledAssetImporter = tiledAssetImporter;
            SuperTileset = superTileset;
        }

        public virtual void Prepare(int expectedWidth, int expectedHeight)
        {
            ExpectedWidth = expectedWidth;
            ExpectedHeight = expectedHeight;
            OnPrepare();
        }

        public abstract bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight);

        protected abstract void OnPrepare();
    }
}

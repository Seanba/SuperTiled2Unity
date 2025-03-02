namespace SuperTiled2Unity.Editor
{
    internal sealed class TilesetAssetResolverAseprite : TilesetAssetResolver
    {
        public TilesetAssetResolverAseprite(string sourceAssetPath, TiledAssetImporter tiledAssetImporter, SuperTileset superTileset)
            : base(sourceAssetPath, tiledAssetImporter, superTileset)
        {
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            return false;
        }

        protected override void OnPrepare()
        {
            // fixit - here's where the real work begings for Aseprite support
            // fixit - individual layers not supported
        }
    }
}

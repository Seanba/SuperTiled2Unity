namespace SuperTiled2Unity.Editor
{
    // The resolver to use when we have an issue with the source asset for tilesets
    internal sealed class TilesetAssetResolverError : TilesetAssetResolver
    {
        public TilesetAssetResolverError(string sourceAssetPath) : base(sourceAssetPath)
        {
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            return false;
        }

        protected override void OnPrepare()
        {
        }
    }
}

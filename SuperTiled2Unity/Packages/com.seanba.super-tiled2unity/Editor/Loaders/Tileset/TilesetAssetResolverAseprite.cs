namespace SuperTiled2Unity.Editor
{
    internal sealed class TilesetAssetResolverAseprite : TilesetAssetResolver
    {
        public TilesetAssetResolverAseprite(string sourceAssetPath) : base(sourceAssetPath)
        {
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            return false; // fixit - add sprites and tile for aseprite
        }

        protected override void OnPrepare()
        {
            // fixit - get aseprite internal we need
        }
    }
}

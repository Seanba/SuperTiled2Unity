namespace SuperTiled2Unity.Editor
{
    // fixit - what if the texture is too small (import settings)? ReportWrongTextureSize
    internal sealed class TilesetAssetResolverTexture : TilesetAssetResolver
    {
        public TilesetAssetResolverTexture(string sourceAssetPath) : base(sourceAssetPath)
        {
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            // fixit - should be easy

            return false;
        }

        protected override void OnPrepare()
        {
            // fixit - what do we need?
        }
    }
}

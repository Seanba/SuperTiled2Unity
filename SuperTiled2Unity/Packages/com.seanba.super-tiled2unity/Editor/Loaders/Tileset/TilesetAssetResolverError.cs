using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // The resolver to use when we have an issue with the source asset for tilesets
    internal sealed class TilesetAssetResolverError : TilesetAssetResolver
    {
        public TilesetAssetResolverError(string sourceAssetPath, TiledAssetImporter tiledAssetImporter, SuperTileset superTileset)
            : base(sourceAssetPath, tiledAssetImporter, superTileset)
        {
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight, Vector2 tiledPivot, Vector2 unityPivot)
        {
            return false;
        }

        protected override void OnPrepare()
        {
        }

        protected override void OnDispose()
        {
        }
    }
}

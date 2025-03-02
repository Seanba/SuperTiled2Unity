using System.IO;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal sealed class TilesetAssetResolverTexture : TilesetAssetResolver
    {
        public TextureImporter TextureImporter { get; }

        private Texture2D m_Texture;

        public TilesetAssetResolverTexture(string sourceAssetPath, TiledAssetImporter tiledImporter, SuperTileset superTileset, TextureImporter textureImporter)
            : base(sourceAssetPath, tiledImporter, superTileset)
        {
            TextureImporter = textureImporter;
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            if (m_Texture == null)
            {
                return false;
            }

            var assetName = Path.GetFileNameWithoutExtension(TiledAssetImporter.assetPath);
            var spriteName = $"{assetName}.Sprite.{tileId}";
            var tileName = $"{assetName}.Tile.{tileId}";
            var rect = new Rect(srcx, srcy, tileWidth, tileHeight);

            Sprite spriteToAdd;
            SuperTile tileToAdd;

            // Create and add the sprite that the tile is based off of
            {
                spriteToAdd = Sprite.Create(m_Texture, rect, Vector2.zero, SuperTileset.m_PixelsPerUnit);
                spriteToAdd.name = spriteName;
            }

            // Create and add the tile
            {
                tileToAdd = SuperTile.CreateSuperTile();
                tileToAdd.m_TileId = tileId;
                tileToAdd.name = tileName;
                tileToAdd.m_Sprite = spriteToAdd;
                tileToAdd.m_Width = rect.width;
                tileToAdd.m_Height = rect.height;
                tileToAdd.m_TileOffsetX = SuperTileset.m_TileOffset.x;
                tileToAdd.m_TileOffsetY = SuperTileset.m_TileOffset.y;
                tileToAdd.m_ObjectAlignment = SuperTileset.m_ObjectAlignment;
                tileToAdd.m_TileRenderSize = SuperTileset.m_TileRenderSize;
                tileToAdd.m_FillMode = SuperTileset.m_FillMode;

                SuperTileset.m_Tiles.Add(tileToAdd);
            }

            // The identifier for the sprite and tile *must* be unique amoung all other objects that are added to the same import context
            if (spriteToAdd)
            {
                string uniqueId = $"{spriteName}.{InternalId}";
                TiledAssetImporter.SuperImportContext.AddObjectToAsset(uniqueId, spriteToAdd);
            }

            if (tileToAdd)
            {
                string uniqueId = $"{tileName}.{InternalId}";
                TiledAssetImporter.SuperImportContext.AddObjectToAsset(uniqueId, tileToAdd);
            }

            return true;
        }

        protected override void OnPrepare()
        {
            // fixit - error checking (size of texture, import settings too low?)
            // fixit - what if texture doesn't exist?
            // fixit - TiledImporter.ReportWrongTextureSize
            m_Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceAssetPath);
        }
    }
}

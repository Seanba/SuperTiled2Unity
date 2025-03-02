using System.IO;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal sealed class TilesetAssetResolverTexture : TilesetAssetResolver
    {
        public TextureImporter TextureImporter { get; }

        private Texture2D m_Texture;

        public TilesetAssetResolverTexture(string sourceAssetPath, TextureImporter textureImporter) : base(sourceAssetPath)
        {
            TextureImporter = TextureImporter;
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            if (m_Texture == null)
            {
                return false;
            }

            var assetName = Path.GetFileNameWithoutExtension(m_Importer.assetPath); // fixit - assetPath of TiledAsset, not the source
            var spriteName = $"{assetName}.Sprite.{tileId}";
            var tileName = $"{assetName}.Tile.{tileId}";
            var rect = new Rect(src, srcy, tileWidth, tileHeight);

            Sprite spriteToAdd;
            SuperTile tileToAdd;

            // fixit - might have multiple sprites (for animating tiles in aseprite files)
            //      Add them all at the end once we determine this will succeed
            // Create and add the sprite that the tile is based off of
            {
                spriteToAdd = Sprite.Create(m_Texture, rect, Vector2.zero, m_SuperTileset.m_PixelsPerUnit);
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
                tileToAdd.m_TileOffsetX = m_SuperTileset.m_TileOffset.x;
                tileToAdd.m_TileOffsetY = m_SuperTileset.m_TileOffset.y;
                tileToAdd.m_ObjectAlignment = m_SuperTileset.m_ObjectAlignment;
                tileToAdd.m_TileRenderSize = m_SuperTileset.m_TileRenderSize;
                tileToAdd.m_FillMode = m_SuperTileset.m_FillMode;

                if (m_Importer is TsxAssetImporter tsxAssetImporter)
                {
                    tileToAdd.m_ColliderType = tsxAssetImporter.m_ColliderType;
                }

                m_SuperTileset.m_Tiles.Add(tileToAdd);
            }

            // The identifier for the sprite and tile *must* be unique amoung all other objects that are added to the same import context
            if (spriteToAdd)
            {
                string uniqueId = $"{spriteName}.{m_InternalId}";
                m_Importer.SuperImportContext.AddObjectToAsset(uniqueId, spriteToAdd);
            }

            if (tileToAdd)
            {
                string uniqueId = $"{tileName}.{m_InternalId}";
                m_Importer.SuperImportContext.AddObjectToAsset(uniqueId, tileToAdd);
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

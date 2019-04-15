using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class TilesetBuilder
    {
        private class SourceTile
        {
            public int Index;
            public Texture2D SourceTexture2D;
            public Rect SourceRectangle;
            public bool IsTransparent;
        }

        private TiledAssetImporter m_TiledAssetImporter;
        private SuperTileset m_TilesetScript;

        private List<SourceTile> m_SourceTiles = new List<SourceTile>();

        public TilesetBuilder(TiledAssetImporter importer, SuperTileset tilesetScript)
        {
            m_TiledAssetImporter = importer;
            m_TilesetScript = tilesetScript;
        }

        public void AddTile(int index, Texture2D texSource, Rect rcSource, bool isTransparent)
        {
            var soureTile = new SourceTile() { Index = index, SourceTexture2D = texSource, SourceRectangle = rcSource, IsTransparent = isTransparent };
            m_SourceTiles.Add(soureTile);
        }

        public void Build()
        {
            if (!m_SourceTiles.Any())
            {
                return;
            }

            // We have everything we need to create our sprites and tiles, including their texture dependencies. Commit.
            Commit();

            // Clear everything out
            m_SourceTiles.Clear();
        }

        private void Commit()
        {
            // Order tiles by Id
            var ordered = m_SourceTiles.OrderBy(t => t.Index);

            // We may end up with one transparent sprite in our collection
            // This limits us to only having to add one sprite representing an "empty" tile to our asset
            Sprite transSprite = null;

            foreach (var t in ordered)
            {
                string tileName = string.Format("Tile_{0}_{1}", m_TilesetScript.name, t.Index + 1);

                Sprite sprite = null;

                if (t.IsTransparent && transSprite != null)
                {
                    // Use the transparent sprite
                    sprite = transSprite;
                }
                else if (t.IsTransparent)
                {
                    // Set up the transprent sprite for the first time in this collection
                    var spriteName = string.Format("Sprite_{0}_trans", m_TilesetScript.name);
                    sprite = Sprite.Create(t.SourceTexture2D, t.SourceRectangle, Vector2.zero, m_TiledAssetImporter.SuperImportContext.Settings.PixelsPerUnit);
                    sprite.name = spriteName;
                    m_TiledAssetImporter.SuperImportContext.AddObjectToAsset(spriteName, sprite);
                    transSprite = sprite;
                }
                else
                {
                    // A regular sprite (with non-transparent pixels)
                    // Create the sprite with the anchor at (0, 0)
                    string spriteName = string.Format("Sprite_{0}_{1}", m_TilesetScript.name, t.Index + 1);
                    sprite = Sprite.Create(t.SourceTexture2D, t.SourceRectangle, Vector2.zero, m_TiledAssetImporter.SuperImportContext.Settings.PixelsPerUnit);
                    sprite.name = spriteName;
                    m_TiledAssetImporter.SuperImportContext.AddObjectToAsset(spriteName, sprite);
                }

                // Create the tile that uses the sprite
                var tile = ScriptableObject.CreateInstance<SuperTile>();
                tile.m_TileId = t.Index;
                tile.name = tileName;
                tile.m_Sprite = sprite;
                tile.m_Width = t.SourceRectangle.width;
                tile.m_Height = t.SourceRectangle.height;
                tile.m_TileOffsetX = m_TilesetScript.m_TileOffset.x;
                tile.m_TileOffsetY = m_TilesetScript.m_TileOffset.y;
                tile.m_IsTransparent = t.IsTransparent;

                m_TilesetScript.m_Tiles.Add(tile);
                m_TiledAssetImporter.SuperImportContext.AddObjectToAsset(tileName, tile);
            }
        }
    }
}

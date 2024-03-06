using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public class BadTileSpriteProvider : ScriptableSingleton<BadTileSpriteProvider>
    {
        public BadTileTextures m_BadTileTextures;

        private static readonly Dictionary<string, int> m_NamesToHash = new Dictionary<string, int>();

        internal bool CreateSpriteAndTile(int tileId, Color tint, int width, int height, SuperTileset superTileset, out Sprite sprite, out SuperBadTile tile)
        {
            Assert.IsNotNull(m_BadTileTextures);
            Assert.IsTrue(m_BadTileTextures.m_Textures.Length > 0);

            // Use the hash of the tileset in an attempt to better randomize the deterministic assignment of bad tiles
            int hash = GetTilesetHash(superTileset.name);
            var badTileTexture = m_BadTileTextures.m_Textures[(tileId + hash) % m_BadTileTextures.m_Textures.Length];

            var spriteRect = Rect.MinMaxRect(0, 0, Mathf.Min(width, badTileTexture.width), Mathf.Min(height, badTileTexture.height));
            sprite = Sprite.Create(badTileTexture, spriteRect, Vector2.zero, superTileset.m_PixelsPerUnit);
            sprite.name = $"error-sprite-{tileId}";

            tile = ScriptableObject.CreateInstance<SuperBadTile>();
            tile.m_TileId = tileId;
            tile.name = $"error-tile-{tileId}";
            tile.m_Sprite = sprite;
            tile.m_Width = width;
            tile.m_Height = height;
            tile.m_TileOffsetX = superTileset.m_TileOffset.x;
            tile.m_TileOffsetY = superTileset.m_TileOffset.y;
            tile.m_ObjectAlignment = superTileset.m_ObjectAlignment;
            tile.m_TileRenderSize = superTileset.m_TileRenderSize;
            tile.m_FillMode = superTileset.m_FillMode;
            tile.m_Color = tint;

            return true;
        }

        private static int GetTilesetHash(string name)
        {
            if (m_NamesToHash.TryGetValue(name, out int hash))
            {
                return hash;
            }

            // C# strings do not generate a deterministic hash code
            hash = Mathf.Abs(Hash128.Compute(name).GetHashCode());
            m_NamesToHash.Add(name, hash);
            return hash;
        }
    }
}
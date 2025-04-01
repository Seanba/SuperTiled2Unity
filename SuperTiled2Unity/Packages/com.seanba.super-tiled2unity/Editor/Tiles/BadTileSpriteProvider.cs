using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public class BadTileSpriteProvider : ScriptableSingleton<BadTileSpriteProvider>
    {
        [SerializeField]
        private BadTileTextures m_BadTileTextures;

        private static readonly Dictionary<string, int> m_NamesToHash = new Dictionary<string, int>();

        internal bool CreateSpriteAndTile(int tileId, Color tint, int width, int height, SuperTileset superTileset, float ppu, out Sprite sprite, out SuperBadTile tile)
        {
            // We can't take for granted that the collection of bad textures is always serialized even though that is my expectation, ffs
            if (m_BadTileTextures == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:BadTileTextures");
                Assert.IsTrue(guids.Length > 0);

                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                m_BadTileTextures = AssetDatabase.LoadAssetAtPath<BadTileTextures>(assetPath);

                Assert.IsNotNull(m_BadTileTextures, "Collection of bad tile textures is null.");
                Assert.IsTrue(m_BadTileTextures.m_Textures.Length > 0);
            }

            // Use the hash of the tileset in an attempt to better randomize the deterministic assignment of bad tiles
            int hash = GetTilesetHash(superTileset.name);
            var badTileTexture = m_BadTileTextures.m_Textures[(tileId + hash) % m_BadTileTextures.m_Textures.Length];

            var spriteRect = Rect.MinMaxRect(0, 0, Mathf.Min(width, badTileTexture.width), Mathf.Min(height, badTileTexture.height));
            sprite = Sprite.Create(badTileTexture, spriteRect, Vector2.zero, ppu);
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

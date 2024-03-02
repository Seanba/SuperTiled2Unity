using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public class BadTileSpriteProvider : ScriptableSingleton<BadTileSpriteProvider>
    {
        public Texture2D m_BadTileTexture;

        private static readonly Color32[] m_Colors =
        {
            NamedColors.HotPink,
            NamedColors.Pink,
            NamedColors.DeepPink,
            NamedColors.LightPink,
            NamedColors.Red,
            NamedColors.Green,
            NamedColors.Blue,
            NamedColors.CornflowerBlue,
            NamedColors.Yellow,
            NamedColors.Purple,
            NamedColors.White,
        };

        internal bool CreateSpriteAndTile(int tileId, int width, int height, SuperTileset superTileset, out Sprite sprite, out SuperBadTile tile)
        {
            Assert.IsNotNull(m_BadTileTexture);

            var spriteRect = Rect.MinMaxRect(0, 0, Mathf.Min(width, m_BadTileTexture.width), Mathf.Min(height, m_BadTileTexture.height));
            sprite = Sprite.Create(m_BadTileTexture, spriteRect, Vector2.zero, superTileset.m_PixelsPerUnit);
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
            tile.m_Color = m_Colors[tileId % m_Colors.Length];

            return true;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal sealed class TilesetAssetResolverAseprite : TilesetAssetResolver
    {
        private class Frame
        {
            public Sprite Sprite { get; set; }
            public float Timestamp { get; set; }
            public float Duration { get; set; }
        }

        private class FrameManager
        {
            public List<Frame> Frames { get; } = new List<Frame>();

            public void AddKey(float timestamp, Sprite sprite, float initialDuration)
            {
                // Can we merge with a privous frame? This will close out the animation data.
                if (Frames.LastOrDefault(f => f.Sprite == sprite) != null)
                {
                    var frame = Frames.Last();
                    frame.Duration = (timestamp + initialDuration) - frame.Timestamp;
                }
                else
                {
                    if (Frames.Any())
                    {
                        // Close the last frame
                        var lastFrame = Frames.Last();
                        lastFrame.Duration = timestamp - lastFrame.Timestamp;
                    }

                    // Add the new frame
                    var frame = new Frame
                    {
                        Sprite = sprite,
                        Timestamp = timestamp,
                        Duration = initialDuration,
                    };

                    Frames.Add(frame);
                }
            }
        }

        //private FrameManager m_FrameManager = new FrameManager(); // fixit - still need this?

        public TilesetAssetResolverAseprite(string sourceAssetPath, TiledAssetImporter tiledAssetImporter, SuperTileset superTileset)
            : base(sourceAssetPath, tiledAssetImporter, superTileset)
        {
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            return false;

            /*
            if (!m_WasSuccessfullyImported)
            {
                return false;
            }

            if (m_AseTexture == null)
            {
                return false;
            }

            if (m_AseSprites?.Any() != true)
            {
                return false;
            }

            if (m_FrameManager.Frames.Count == 0)
            {
                return false;
            }

            // We only add one tile but many sprites
            SuperTile tileToAdd = null;
            var fps = ST2USettings.instance.m_AnimationFramerate;
            var animationBuilder = new AnimationBuilder(fps);

            // Add sprites for each frame
            for (int i = 0; i < m_FrameManager.Frames.Count; i++)
            {
                int x = srcx;
                int y = srcy;

                // In Tiled, texture origin is the top-left. However, in Unity the origin is bottom-left.
                y = (ExpectedHeight - y) - tileHeight;

                var frame = m_FrameManager.Frames[i];

                var sourceTexture = m_AseTexture;
                var sourceSprite = frame.Sprite;

                var assetName = Path.GetFileNameWithoutExtension(TiledAssetImporter.assetPath);
                var spriteName = $"{assetName}.Sprite.{tileId}.f{i}";

                var rect = new Rect(x, y, tileWidth, tileHeight);
                rect.x += sourceSprite.rect.x;
                rect.y += sourceSprite.rect.y;

                // Create and add the sprite that the tile is based off of
                var spriteToAdd = Sprite.Create(sourceTexture, rect, Vector2.zero, SuperTileset.m_PixelsPerUnit);
                spriteToAdd.name = spriteName;

                string spriteUniqueId = $"{spriteName}.{InternalId}";
                TiledAssetImporter.SuperImportContext.AddObjectToAsset(spriteUniqueId, spriteToAdd);

                // Keep track of the frame time on this sprite
                animationBuilder.AddFrames(spriteToAdd, frame.Duration);

                // Create and add the tile (only for the first sprite)
                if (i == 0)
                {
                    var tileName = $"{assetName}.Tile.{tileId}";
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

                    string tileUniqueId = $"{tileName}.{InternalId}";
                    TiledAssetImporter.SuperImportContext.AddObjectToAsset(tileUniqueId, tileToAdd);
                }
            }

            // Only animate if we have more than one frame
            if (tileToAdd != null && m_FrameManager.Frames.Count > 1)
            {
                tileToAdd.m_AnimationSprites = animationBuilder.Sprites.ToArray();
            }

            return true;
            */
        }

        protected override void OnPrepare()
        {
            //m_WasSuccessfullyImported = true; // fixit - what to do here?
        }
    }
}

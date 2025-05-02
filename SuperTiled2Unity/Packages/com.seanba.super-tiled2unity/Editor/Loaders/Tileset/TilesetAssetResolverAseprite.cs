using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
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

        public AsepriteImporter AsepriteImporter { get; }

        private Texture2D m_AseTexture;
        private List<Sprite> m_AseSprites;

        private bool m_WasSuccessfullyImported;
        private FrameManager m_FrameManager = new FrameManager();

        public TilesetAssetResolverAseprite(string sourceAssetPath, TiledAssetImporter tiledAssetImporter, SuperTileset superTileset, AsepriteImporter asepriteImporter)
            : base(sourceAssetPath, tiledAssetImporter, superTileset)
        {
            AsepriteImporter = asepriteImporter;
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
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
        }

        protected override void OnPrepare()
        {
            m_WasSuccessfullyImported = true;

            if (AsepriteImporter.importMode != FileImportModes.AnimatedSprite &&
                AsepriteImporter.importMode != FileImportModes.SpriteSheet)
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "File import mode must be Animated Sprite or Sprite Sheet.");
            }

            if (AsepriteImporter.layerImportMode != LayerImportModes.MergeFrame)
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Layer import mode must be Merge Frame.");
            }

            if (AsepriteImporter.spritePadding != 0)
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Must have Sprite Padding of 0");
            }

            var allObjects = AssetDatabase.LoadAllAssetsAtPath(SourceAssetPath);

            // There should only be one texture (acting as an atlas of all the animation frames)
            m_AseTexture = allObjects.OfType<Texture2D>().FirstOrDefault();
            if (m_AseTexture == null)
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not load Texture2D");
            }

            if (!m_WasSuccessfullyImported)
            {
                // Stop trying to import. We don't have enough to work with.
                return;
            }

            // Get all the sprites that the Aseprite importer created
            m_AseSprites = allObjects.OfType<Sprite>().ToList();

            if (m_AseSprites.Count == 0 && AsepriteImporter.importMode == FileImportModes.SpriteSheet)
            {
                // The Aseprite editor created only a texture
                // We will build our own sprite to use for making tiles and store it in our asset
                float x = AsepriteImporter.mosaicPadding;
                float y = AsepriteImporter.mosaicPadding;
                float w = AsepriteImporter.canvasSize.x;
                float h = AsepriteImporter.canvasSize.y;
                var rect = new Rect(x, y, w, h);
                var sprite = Sprite.Create(m_AseTexture, rect, Vector2.zero);
                sprite.name = "_st2u.aseprite.SpriteSheet";

                m_AseSprites.Add(sprite);
                TiledAssetImporter.SuperImportContext.AddObjectToAsset(sprite.name, sprite);
            }

            if (!m_AseSprites.Any())
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not load any Sprites.");
            }

            // There should only be one animation clip. This is how we know which frames are visible when and for how long.
            var animationClip = allObjects.OfType<AnimationClip>().FirstOrDefault();
            if (animationClip == null)
            {
                // Just use the first sprite. We won't be animating.
                var firstSprite = m_AseSprites.FirstOrDefault();
                if (firstSprite != null)
                {
                    m_FrameManager.AddKey(0.0f, firstSprite, 1.0f);
                }
                else
                {
                    // Do animation clip and no sprites? How is this possible? The AsepriteAssetPostprocessor should make sure we have one.
                    m_WasSuccessfullyImported = false;
                    TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not load Animation Clip or sprites.");
                }
            }
            else
            {
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
                if (bindings?.Any() != true)
                {
                    m_WasSuccessfullyImported = false;
                    TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not find animation curve bindings.");
                }
                else
                {
                    var keys = AnimationUtility.GetObjectReferenceCurve(animationClip, bindings[0]);
                    if (keys?.Any() != true)
                    {
                        m_WasSuccessfullyImported = false;
                        TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not find animation curve keys.");
                    }
                    else if (keys.Any(k => !(k.value is Sprite)))
                    {
                        m_WasSuccessfullyImported = false;
                        TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Animation curve keys do not have sprites.");
                    }
                    else if (keys.Select(k => ((Sprite)k.value).rect.size).Distinct().Count() > 1)
                    {
                        m_WasSuccessfullyImported = false;
                        TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "All frames of the animation must be the same size.");
                    }
                    else
                    {
                        // Finally have the animation data we need
                        float initialDuration = 1.0f / animationClip.frameRate;
                        foreach (var key in keys)
                        {
                            m_FrameManager.AddKey(key.time, key.value as Sprite, initialDuration);
                        }
                    }
                }
            }
        }
    }
}

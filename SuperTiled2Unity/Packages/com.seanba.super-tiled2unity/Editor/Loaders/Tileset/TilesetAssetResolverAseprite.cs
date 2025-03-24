using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal sealed class TilesetAssetResolverAseprite : TilesetAssetResolver
    {
        public AsepriteImporter AsepriteImporter { get; }

        private Texture2D m_AseTexture;
        private AnimationClip m_AseAnimationClip;
        private List<Sprite> m_AseSprites;

        private bool m_WasSuccessfullyImported;

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

            if (m_AseAnimationClip == null)
            {
                return false;
            }

            // fixit - add sprites for each frame

            // fixit - add the tile
            // fixit - The tile must have animation sprites (created just above)

            return false;
        }

        protected override void OnPrepare()
        {
            m_WasSuccessfullyImported = true;

            if (AsepriteImporter.importMode != FileImportModes.AnimatedSprite)
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "File import mode must be Animated Sprite");
            }

            if (AsepriteImporter.layerImportMode != LayerImportModes.MergeFrame)
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Layer import mode must be Merge Frame");
            }

            if (AsepriteImporter.generateAnimationClips == false)
            {
                m_WasSuccessfullyImported = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Must generate Animation Clips");
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
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not load Texture2D");
            }

            // Each sprite is a frame into the texture
            m_AseSprites = allObjects.OfType<Sprite>().ToList();
            if (!m_AseSprites.Any())
            {
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not load any Sprites");
            }

            // There should only be one animation clip. This is how we know which frames are visible when and for how long.
            m_AseAnimationClip = allObjects.OfType<AnimationClip>().FirstOrDefault(); // fixit - does this even need to be a data member?
            if (m_AseAnimationClip == null)
            {
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not load Animation Clip");
            }
            else
            {
                // fixit - every sprite must be big enough (check for that here?)
                // fixit - I think every sprite *must* be the same size. Especially if we are using tilesheets.
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(m_AseAnimationClip);
                if (bindings?.Any() != true)
                {
                    m_WasSuccessfullyImported = false;
                    TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not find animation curve bindings.");
                }
                else
                {
                    var keys = AnimationUtility.GetObjectReferenceCurve(m_AseAnimationClip, bindings[0]);
                    if (keys?.Any() != true)
                    {
                        m_WasSuccessfullyImported = false;
                        TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Could not find animation curve keys.");
                    }
                    else
                    {
                        // Finally have the animation data we need
                        // 1 / m_AseAnimationClip.frameRate == the time of the final frame, I think // fixit
                        //keys[0].time  // absolute start time
                        //keys[1].value // Sprite type expeted
                        Debug.LogError($"fixit - found keys: {keys.Length}");
                    }
                }
            }
        }
    }
}

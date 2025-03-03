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

        private bool m_AreImportSettingsValid;

        public TilesetAssetResolverAseprite(string sourceAssetPath, TiledAssetImporter tiledAssetImporter, SuperTileset superTileset, AsepriteImporter asepriteImporter)
            : base(sourceAssetPath, tiledAssetImporter, superTileset)
        {
            AsepriteImporter = asepriteImporter;
        }

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            if (!m_AreImportSettingsValid)
            {
                return false;
            }

            // fixit - what to do

            return false;
        }

        protected override void OnPrepare()
        {
            if (AsepriteImporter.importMode != FileImportModes.AnimatedSprite)
            {
                m_AreImportSettingsValid = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "File import mode must be Animated Sprite");
                return;
            }

            if (AsepriteImporter.layerImportMode != LayerImportModes.MergeFrame)
            {
                m_AreImportSettingsValid = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Layer import mode must be Merge Frame");
                return;
            }

            if (AsepriteImporter.generateAnimationClips == false)
            {
                m_AreImportSettingsValid = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Must generate Animation Clips");
                return;
            }

            if (AsepriteImporter.spritePadding != 0)
            {
                m_AreImportSettingsValid = false;
                TiledAssetImporter.ReportErrorsInDependency(SourceAssetPath, "Must have Sprite Padding of 0");
                return;
            }

            var allObjects = AssetDatabase.LoadAllAssetsAtPath(SourceAssetPath);

            // There should only be one texture (acting as an atlas of all the animation frames)
            m_AseTexture = allObjects.OfType<Texture2D>().FirstOrDefault();

            // Each sprite is a frame into the texture
            m_AseSprites = allObjects.OfType<Sprite>().ToList();

            // fixit - every sprite must be big enough

            // There should only be one animation clip. This is how we now which frames are visible when and for how long.
            m_AseAnimationClip = allObjects.OfType<AnimationClip>().FirstOrDefault();

            m_AreImportSettingsValid = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;

namespace SuperTiled2Unity.Editor
{
    // This file contains the work-arounds needed to support sprite atlases for our custom assets

    public interface IHasSpriteAtlasPacker
    {
        SpriteAtlasPacker SpriteAtlasPacker { get; }
    }

    [Serializable]
    public class SpriteAtlasPacker
    {
        // We need to use a "marker" to trick-out the sprite atlas into updating itself
        // Note: Unity has a fix for this reportedly coming in 2019.2.0a11
        public const string MarkerFileNameWithoutExtension = "supertiled2unity-atlas-marker";
        private const string DefaultSpriteAtlas = "ST2U Default Atlas";

        private static readonly GUIContent m_SpriteAtlasContent = new GUIContent("Sprite Atlas", "Sprite Atlases reduce draw calls and prevent seams/bands in Tilemaps.");

        // Serializable fields used by importers
        public SpriteAtlas m_RemoveFromAtlas;
        public SpriteAtlas m_AddToAtlas;
        public bool m_UserChanged;

        // fixit - first-time gets default sprite atlas (can that go in constructor?)

        public void AddAndRemoveSprites(string spriteSourcePath)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSourcePath).OfType<Sprite>().ToArray();

            if (sprites.Any())
            {
                // Removes our (old) sprites from the (old) atlas
                if (m_RemoveFromAtlas != null)
                {
                    m_RemoveFromAtlas.Remove(sprites);
                }

                // Adds our (new) sprites to the (new) atlas. Also need the sprite atlas marker.
                if (m_AddToAtlas != null)
                {
                    m_AddToAtlas.Add(sprites);
                    AddSpriteAtlasMarker();
                }
            }
        }

        // fixit - need UI stuff
        public static void ShowEditorGui(SerializedObject serializedObject)
        {
            // This makes some big naming assumptions
            var packerProperty = serializedObject.FindProperty("m_SpriteAtlasPacker");
            Assert.IsNotNull(packerProperty, "Importer requires a SpriteAtlasPacker field named m_SpriteAtlasPacker");

            //var 

        }

        private void AddSpriteAtlasMarker()
        {
            var marker = FindSpriteAtlasMarker();
            if (marker != null)
            {
                m_AddToAtlas.Add(new Sprite[1] { marker });

                // The marker must be re-imported. This is the trick that foces our sprite atlas to be updated.
                var markerPath = AssetDatabase.GetAssetPath(marker);
                AssetDatabase.ImportAsset(markerPath, ImportAssetOptions.ForceUpdate);
            }
        }

        private static SpriteAtlas FindDefaultSpriteAtlas()
        {
            // Are we supporting default sprite atlases?
            if (string.IsNullOrEmpty(DefaultSpriteAtlas))
            {
                return null;
            }

            var find = string.Format("{0} t: spriteatlas", DefaultSpriteAtlas);
            foreach (var guid in AssetDatabase.FindAssets(find))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

                if (atlas != null)
                {
                    return atlas;
                }
            }

            Debug.LogErrorFormat("Could not find default sprite atlas '{0}'. Make sure SuperTiled2Unity was installed correctly.", DefaultSpriteAtlas);
            return null;
        }

        private static Sprite FindSpriteAtlasMarker()
        {
            foreach (var guid in AssetDatabase.FindAssets(MarkerFileNameWithoutExtension))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var marker = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (marker != null)
                {
                    return marker;
                }
            }

            Debug.LogErrorFormat("Could not find sprite atlas marker '{0}'. Make sure SuperTiled2Unity was installed correctly.", MarkerFileNameWithoutExtension);
            return null;
        }
    }

    // fixit - rename this class
    public class ST2USpriteAtlasPostprocessor : AssetPostprocessor
    {
        // fixit - make sure example files are not polluting the default sprite atlas
        private void OnPreprocessAsset()
        {
            // Sprite atlases will not update themselves trough script unless we "dirty" our atlas marker
            // (Remove this hack when Unity releases a better fix)
            if (assetPath.ToLower().Contains(SpriteAtlasPacker.MarkerFileNameWithoutExtension))
            {
                // Simply add some unique user data to the marker.
                // Use current time as context to a human combined with a unique GUID.
                assetImporter.userData = string.Format("{{ {0}, {1} }}", Guid.NewGuid(), DateTime.Now.ToString());
            }
            else if (assetImporter is IHasSpriteAtlasPacker)
            {
                (assetImporter as IHasSpriteAtlasPacker).SpriteAtlasPacker.AddAndRemoveSprites(assetPath);
            }
        }
    }
}

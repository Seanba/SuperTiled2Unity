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
    // fixit - need a command to empty atlas? (Just in general)
    // fixit - maybe start from scratch on another file (and remove the IHasSpriteAtlasPacker crap)

    // This file contains the work-arounds needed to support sprite atlases for our custom assets

    public interface IHasSpriteAtlasPacker
    {
        SpriteAtlasPacker SpriteAtlasPacker { get; }
    }

    // fixit - this is such a ginormous pain in the ass and now I'm wondering if a scritable object saved with the asset is not best, ffs
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
        // Should be called by our postprocessor only
        public void OnPreprocessAsset_AddAndRemoveSprites(string spriteSourcePath)
        {
            //EditorGUILayout.ObjectField

            /* // fixit - clean this up
            var sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSourcePath).OfType<Sprite>().ToArray();

            if (sprites.Any())
            {
                // Removes our sprites and marker from old
                if (m_RemoveFromAtlas != null)
                {
                    m_RemoveFromAtlas.Remove(sprites);
                    AddSpriteAtlasMarker(); // fixit - how do we do this?
                }

                // Add our sprites and marker to new (and re-import marker)
                if (m_AddToAtlas != null)
                {
                    m_AddToAtlas.Add(sprites);
                    AddSpriteAtlasMarker();
                }
            }
            */
        }

        // Should be called by custom Editor class only before base.Apply
        public void Editor_PreApply()
        {
            Debug.LogWarningFormat("fixit - pre apply add: {0}, remove: {1}, userChanged = {2}", m_AddToAtlas, m_RemoveFromAtlas, m_UserChanged);
        }

        public void Editor_PostApply()
        {
            Debug.LogWarningFormat("fixit - post apply add: {0}, remove: {1}, userChanged = {2}", m_AddToAtlas, m_RemoveFromAtlas, m_UserChanged);
        }

        // fixit - need UI stuff
        // Should be invoked by the asset editor only
        public static void Editor_ShowEditorGui(SerializedObject serializedObject)
        {
            // This makes some big naming assumptions
            var packerProperty = serializedObject.FindProperty("m_SpriteAtlasPacker");
            Assert.IsNotNull(packerProperty);

            var addToProperty = packerProperty.FindPropertyRelative("m_AddToAtlas");
            Assert.IsNotNull(addToProperty);

            var removeFromProperty = packerProperty.FindPropertyRelative("m_RemoveFromAtlas");
            Assert.IsNotNull(removeFromProperty);

            var userChangeProperty = packerProperty.FindPropertyRelative("m_UserChanged");
            Assert.IsNotNull(userChangeProperty);

            // Keep track of old // fixit - does this work? (No, but getting close, I think)
            var oldAddToPropertyObject = addToProperty.objectReferenceValue;

            EditorGUILayout.PropertyField(removeFromProperty); // fixit - do not edit this (testing only)
            EditorGUILayout.PropertyField(userChangeProperty); // fixit - do not edit this (testing only)

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(addToProperty, m_SpriteAtlasContent);
            if (EditorGUI.EndChangeCheck())
            {
                // The user initiated this change to keep track of that so the default sprite atlas is not assigned again.
                // (This allows user to select 'None' and have that choice stick)
                // fixit - we cannot do this here. We must do this on apply only.
                removeFromProperty.objectReferenceValue = null;
                userChangeProperty.boolValue = false;
            }
        }

        private void AddSpriteAtlasMarker()
        {
            var marker = FindSpriteAtlasMarker();
            if (marker != null)
            {
                m_AddToAtlas.Add(marker.Yield().ToArray());

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
                (assetImporter as IHasSpriteAtlasPacker).SpriteAtlasPacker.OnPreprocessAsset_AddAndRemoveSprites(assetPath);
            }
        }
    }
}

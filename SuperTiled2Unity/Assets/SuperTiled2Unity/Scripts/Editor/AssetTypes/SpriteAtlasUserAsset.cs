using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace SuperTiled2Unity.Editor
{
    // fixit - don't forget to use default first time (first time means there is no previous SpriteAtlasUserAsset)
    // Asset to help us manage sprite atlases which are kind of a pain for scripting
    public class SpriteAtlasUserAsset : ScriptableObject
    {
        // We need to use a "marker" to trick-out the sprite atlas into updating itself
        // Note: Unity has a fix for this reportedly coming in 2019.2.0a11
        private const string MarkerFileNameWithoutExtension = "supertiled2unity-atlas-marker";
        private const string DefaultSpriteAtlas = "ST2U Default Atlas";

        public SpriteAtlas m_SpriteAtlas;

        public static SpriteAtlasUserAsset CreateSpriteAtlasUserAsset(SpriteAtlas atlas)
        {
            var spriteAtlasUser = ScriptableObject.CreateInstance<SpriteAtlasUserAsset>();
            spriteAtlasUser.name = "SpriteAtlasUser";
            spriteAtlasUser.m_SpriteAtlas = atlas;

            return spriteAtlasUser;
        }

        public static SpriteAtlasUserAsset GetAsset(string path)
        {
            return AssetDatabase.LoadAssetAtPath<SpriteAtlasUserAsset>(path);
        }

        public static bool RemoveSpritesFromAtlas(string path)
        {
            var asset = GetAsset(path);
            if (asset != null)
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                asset.RemoveSprites(sprites);
                return true;
            }

            return false;
        }

        public static void AddSpritesToAtlas(string path)
        {
            var asset = GetAsset(path);
            if (asset != null)
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                asset.AddSprites(sprites);
            }
        }

        public void RemoveSprites(IEnumerable<Sprite> sprites)
        {
            if (m_SpriteAtlas != null)
            {
                m_SpriteAtlas.Remove(sprites.ToArray());

                // Remove our atlas marker
                m_SpriteAtlas.Remove(FindSpriteAtlasMarker().Yield().ToArray());
            }
        }

        public void AddSprites(IEnumerable<Sprite> sprites)
        {
            if (m_SpriteAtlas != null && sprites.Any())
            {
                m_SpriteAtlas.Add(sprites.ToArray());

                // Add our marker and force it to be updated
                var marker = FindSpriteAtlasMarker();
                if (marker != null)
                {
                    m_SpriteAtlas.Add(marker.Yield().ToArray());

                    // This sucks but we have to force our marker to be re-imported
                    // This causes the sprite atlas to be updated
                    var markerPath = AssetDatabase.GetAssetPath(marker);
                    AssetDatabase.ImportAsset(markerPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        public static SpriteAtlas FindDefaultSpriteAtlas()
        {
            foreach (var guid in AssetDatabase.FindAssets(DefaultSpriteAtlas))
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

        // Helper postprocessor
        private class Postprocessor : AssetPostprocessor
        {
            private void OnPreprocessAsset()
            {
                if (assetPath.ToLower().Contains(MarkerFileNameWithoutExtension))
                {
                    // Simply add some unique user data to the marker.
                    // Use current time as context to a human combined with a unique GUID.
                    assetImporter.userData = string.Format("{{ {0}, {1} }}", Guid.NewGuid(), DateTime.Now.ToString());
                }
            }

            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (var assetPath in importedAssets)
                {
                    if (assetPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
                    {
                        // Have we imported a sprite atlas user?
                        AddSpritesToAtlas(assetPath);
                    }
                }
            }
        }

        // Helper commands
        [MenuItem("Assets/SuperTiled2Unity/Create Sprite Atlas (for pixels)")]
        public static void CreateSpriteAtlas()
        {
            string folder = AssetDatabaseEx.GetCurrentFolder();
            var path = folder + "/" + "SpriteAtlas.spriteAtlas";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var atlas = new SpriteAtlas();

            SpriteAtlasPackingSettings packing = new SpriteAtlasPackingSettings();
            packing.enableRotation = false;
            packing.enableTightPacking = false;
            packing.padding = 4;
            packing.blockOffset = 1;
            atlas.SetPackingSettings(packing);

            SpriteAtlasTextureSettings tex = new SpriteAtlasTextureSettings();
            tex.generateMipMaps = false;
            tex.filterMode = FilterMode.Point;
            tex.sRGB = true;
            atlas.SetTextureSettings(tex);

            TextureImporterPlatformSettings platform = new TextureImporterPlatformSettings();
            platform.maxTextureSize = 2048;
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            platform.crunchedCompression = false;
            atlas.SetPlatformSettings(platform);

            AssetDatabase.CreateAsset(atlas, path);
        }

        [MenuItem("Assets/SuperTiled2Unity/Clear Sprite Atlas", true)]
        private static bool ClearSpriteAtlasValidate()
        {
            return Selection.activeObject.GetType() == typeof(SpriteAtlas);
        }

        [MenuItem("Assets/SuperTiled2Unity/Clear Sprite Atlas")]
        private static void ClearSpriteAtlas()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                atlas.Remove(atlas.GetPackables());
            }
        }
    }
}

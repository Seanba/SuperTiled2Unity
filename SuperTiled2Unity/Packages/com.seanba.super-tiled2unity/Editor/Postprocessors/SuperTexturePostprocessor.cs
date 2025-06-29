using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperTexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetImporter.importSettingsMissing && !DoesAssetHavePreset(assetPath))
            {
                // The texture is being imported for the first time
                // Give the imported texture better defaults than provided by stock Unity
                TextureImporter textureImporter = assetImporter as TextureImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Multiple;
                textureImporter.mipmapEnabled = false;
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

                TextureImporterSettings settings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(settings);
                settings.spriteGenerateFallbackPhysicsShape = false;
                textureImporter.SetTextureSettings(settings);
            }
        }

        public static bool DoesAssetPathMatchFilter(string assetPath, string filter)
        {
            string[] guids = AssetDatabase.FindAssets(filter);

            string[] matchingPaths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                matchingPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }

            foreach (string path in matchingPaths)
            {
                if (path == assetPath)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool DoesAssetHavePreset(string assetPath)
        {
            foreach (var pt in Preset.GetAllDefaultTypes())
            {
                if (pt.IsValidDefault() && pt.GetManagedTypeName() == "UnityEditor.TextureImporter")
                {
                    var presets = Preset.GetDefaultPresetsForType(pt);
                    foreach (var p in presets)
                    {
                        if (p.enabled)
                        {
                            if (DoesAssetPathMatchFilter(assetPath, p.filter))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}

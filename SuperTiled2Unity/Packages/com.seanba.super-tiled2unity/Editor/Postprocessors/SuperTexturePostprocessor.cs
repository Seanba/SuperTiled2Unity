using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // fixit - note that 2021.2 or newer has some special needs: https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html
    // fixit - see this for cutting up sprites: https://docs.unity3d.com/Manual/Sprite-data-provider-api.html
    public class SuperTexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetImporter.importSettingsMissing)
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


            // Get the list of sprite rects that our Tiled tilesets expect us to have
            // Remove old ST2U sprite rects that are no longer needed
            // Add new ST2U sprite rects
            SpriteRectsManager.Instance.GetSpriteRectsForTexture(assetPath); // fixit - use this to cut up the sprite
        }
    }
}

using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // fixit - note that 2021.2 or newer has some special needs: https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html
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

            //ApplySpriteDataProvider();
        }

        private void ApplySpriteDataProvider()
        {
            // fixit - The texture knows if it is referenced by Tiled assets here
            if (TiledAssetDependencies.Instance.GetAssetDependencies(assetImporter.assetPath, out AssetDependencies depends))
            {
                foreach (var referenceAssetpath in depends.References)
                {
                    var tilesetSpriteData = AssetDatabase.LoadAssetAtPath<TilesetSpriteData>(referenceAssetpath);
                    if (tilesetSpriteData != null)
                    {
                        Debug.Log($"fixit - found referenceAssetpath = {referenceAssetpath}, texture = {tilesetSpriteData.m_SpriteRectsPerTextures[0].m_Texture2D}");
                    }
                }
            }

            /*
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(assetImporter);
            dataProvider.InitSpriteEditorDataProvider();

            // fixit testing
            SpriteRect spriteRect = new SpriteRect();
            spriteRect.alignment = SpriteAlignment.BottomLeft;
            spriteRect.name = "MySprite32";
            spriteRect.rect = new Rect(Vector2.zero, new Vector2(32, 32));
            dataProvider.SetSpriteRects(new SpriteRect[] { spriteRect });

            dataProvider.Apply();
            */
        }
    }
}

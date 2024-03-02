using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperTexturePostprocessor : AssetPostprocessor
    {
        private static ProfilerMarker ProfilerMarker_AddSprites = new ProfilerMarker("AddSprites");

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

            AddSpritesFromTiledFiles();
        }

        private void AddSpritesFromTiledFiles()
        {
            using (ProfilerMarker_AddSprites.Auto())
            {
                var rects = SpriteRectsManager.Instance.GetSpriteRectsForTexture(assetPath);
                if (!rects.Any())
                {
                    // This texture is not (currently) used by Tiled files (maps or tilesets)
                    return;
                }

                // Texture must be imported so that it is made up of multiple sprites
                TextureImporter textureImporter = assetImporter as TextureImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Multiple;

                // Use the sprite editor data provider to add/remove sprite rects
                using (var provider = new SpriteDataProviderWrapper(assetImporter))
                {
                    //var spriteRects = provider.GetSpriteRects(); // fixit - zero out for now but we should be smarter about adding and removing and detecting changes
                    var spriteRects = new List<SpriteRect>();

                    foreach (var rect in rects)
                    {
                        var newSpriteRect = new SpriteRect
                        {
                            name = TilesetLoader.RectToSpriteName(rect),
                            spriteID = GUID.Generate(),
                            rect = new Rect(rect.x, rect.y, rect.width, rect.height),
                            pivot = Vector2.zero,
                            alignment = SpriteAlignment.BottomLeft,
                        };

                        spriteRects.Add(newSpriteRect);
                    }

                    provider.SetSpriteRects(spriteRects);
                }
            }
        }

        private class SpriteDataProviderWrapper : IDisposable
        {
            private static ProfilerMarker ProfilerMarker_SpriteDataProviderWrapper = new ProfilerMarker("SpriteDataProviderWrapper");

            private ISpriteEditorDataProvider m_DataProvider;

#if UNITY_2021_2_OR_NEWER
            // fixit - ISpriteNameFileIdDataProvider, https://docs.unity3d.com/Manual/Sprite-data-provider-api.html
#endif

            public SpriteDataProviderWrapper(AssetImporter assetImporter)
            {
                ProfilerMarker_SpriteDataProviderWrapper.Begin();

                var factory = new SpriteDataProviderFactories();
                factory.Init();

                m_DataProvider = factory.GetSpriteEditorDataProviderFromObject(assetImporter);
                m_DataProvider.InitSpriteEditorDataProvider();
            }

            public void Dispose()
            {
                // fixit - only apply if changes were made because this is surprisingly expensive
                // fixit - SetNameFileIdPairs (2021 or newer)
                m_DataProvider.Apply();

                ProfilerMarker_SpriteDataProviderWrapper.End();
            }

            public List<SpriteRect> GetSpriteRects()
            {
                return m_DataProvider.GetSpriteRects().ToList();
            }

            public void SetSpriteRects(IEnumerable<SpriteRect> spriteRects)
            {
                // fixit SpriteNameFileIdPair https://docs.unity3d.com/Manual/Sprite-data-provider-api.html
                m_DataProvider.SetSpriteRects(spriteRects.ToArray());
            }
        }
    }
}

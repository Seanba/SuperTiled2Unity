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
                var tilesetRects = SpriteRectsManager.Instance.GetSpriteRectsForTexture(assetPath);
                if (!tilesetRects.Any())
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
                    foreach (var rect in tilesetRects)
                    {
                        provider.RequiresSuperTiled2UnitySpriteRect(rect);
                    }
                }
            }
        }

        private class SpriteDataProviderWrapper : IDisposable
        {
            private static ProfilerMarker ProfilerMarker_SpriteDataProviderWrapper = new ProfilerMarker("SpriteDataProviderWrapper");

            private ISpriteEditorDataProvider m_DataProvider;
            private List<SpriteRect> m_OriginalAllSpriteRects;
            private List<SpriteRect> m_OriginalUserSpriteRects;
            private List<SpriteRect> m_OriginalST2USpriteRects;
            private List<SpriteRect> m_RequiredST2USpriteRects;
            private bool m_ForceUpdate;

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

                // User rects are sprite rectangles the user created in the Sprite Editor
                // ST2U rects are sprite rectangles that Super Tiled2Unity added
                m_OriginalAllSpriteRects = m_DataProvider.GetSpriteRects().ToList();
                m_OriginalUserSpriteRects = m_OriginalAllSpriteRects.Where(r => !r.name.StartsWith(TilesetLoader.SpriteNameRoot)).ToList();
                m_OriginalST2USpriteRects = m_OriginalAllSpriteRects.Where(r => r.name.StartsWith(TilesetLoader.SpriteNameRoot)).ToList();
                m_RequiredST2USpriteRects = new List<SpriteRect>();
            }

            public void RequiresSuperTiled2UnitySpriteRect(Rect rect)
            {
                // Note: We always use a bottom-left pivot
                // Does the user list already have a sprite rect we can use?
                if (m_OriginalUserSpriteRects.Any(r => r.rect == rect && r.pivot == Vector2.zero))
                {
                    return;
                }

                // Does the ST2U already have a sprite we can use? If so then add that to our required list.
                var spriteName = TilesetLoader.RectToSpriteName(rect);
                var requiredSpriteRect = m_OriginalST2USpriteRects.FirstOrDefault(s => s.name == spriteName);
                if (requiredSpriteRect == null)
                {
                    // If we're creating a new sprite rect then we must invoke SetSpriteRects on the data provider
                    m_ForceUpdate = true;
                    requiredSpriteRect = new SpriteRect
                    {
                        name = spriteName,
                        spriteID = GUID.Generate(),
                        rect = new Rect(rect.x, rect.y, rect.width, rect.height),
                        pivot = Vector2.zero,
                        alignment = SpriteAlignment.BottomLeft,
                    };
                }

                m_RequiredST2USpriteRects.Add(requiredSpriteRect);
            }

            public void Dispose()
            {
                if (m_ForceUpdate || m_RequiredST2USpriteRects.Count != m_OriginalST2USpriteRects.Count)
                {
                    // fixit SpriteNameFileIdPair (2021 or newer) https://docs.unity3d.com/Manual/Sprite-data-provider-api.html
                    var updatedSpriteRects = m_OriginalUserSpriteRects;
                    updatedSpriteRects.AddRange(m_RequiredST2USpriteRects);
                    m_DataProvider.SetSpriteRects(updatedSpriteRects.ToArray());

                    m_DataProvider.Apply();
                }

                ProfilerMarker_SpriteDataProviderWrapper.End();
            }
        }
    }
}

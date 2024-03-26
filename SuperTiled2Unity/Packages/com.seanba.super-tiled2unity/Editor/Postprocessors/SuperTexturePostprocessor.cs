//#define ST2U_CLEAR_SPRITES_TESTING // Do not pubish with this defined

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperTexturePostprocessor : AssetPostprocessor
    {
        public const int MaxNumberOfImportingSpriteRects = 1024 * 2;

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

#if ST2U_CLEAR_SPRITES_TESTING
            ClearAllSprites();
#else
            AddSpritesFromTiledFiles();
#endif
        }

#if ST2U_CLEAR_SPRITES_TESTING
        private void ClearAllSprites()
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();

            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(assetImporter);
            dataProvider.InitSpriteEditorDataProvider();
            dataProvider.SetSpriteRects(new SpriteRect[0] { });

#if UNITY_2021_2_OR_NEWER
            var fileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            fileIdDataProvider.SetNameFileIdPairs(new SpriteNameFileIdPair[0] { });
#endif
            dataProvider.Apply();
        }
#endif

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
            private static readonly ProfilerMarker ProfilerMarker_SpriteDataProviderWrapper = new ProfilerMarker("SpriteDataProviderWrapper");

            private readonly string m_AssetFileName;
            private readonly ISpriteEditorDataProvider m_DataProvider;
            
            // The list of sprite rects that already exist (from a previous import) and we want to keep
            private readonly List<SpriteRect> m_PreviouslyImportedSpriteRects;

            // The list of ST2U sprites rects that already exist. We may not want to keep all these.
            private readonly List<SpriteRect> m_PreviouslyImportedST2USpriteRects;

            // The list of newly added sprite rects that ST2U expects us to have
            private readonly List<SpriteRect> m_NewlyImportedST2USpriteRects;

            public SpriteDataProviderWrapper(AssetImporter assetImporter)
            {
                ProfilerMarker_SpriteDataProviderWrapper.Begin();

                m_AssetFileName = Path.GetFileName(assetImporter.assetPath);
                var factory = new SpriteDataProviderFactories();
                factory.Init();

                m_DataProvider = factory.GetSpriteEditorDataProviderFromObject(assetImporter);
                m_DataProvider.InitSpriteEditorDataProvider();

                var originalAllSpriteRects = m_DataProvider.GetSpriteRects().ToList();
                m_PreviouslyImportedSpriteRects = originalAllSpriteRects.Where(r => !r.name.StartsWith(TilesetLoader.SpriteNameRoot)).ToList();
                m_PreviouslyImportedST2USpriteRects = originalAllSpriteRects.Where(r => r.name.StartsWith(TilesetLoader.SpriteNameRoot)).ToList();
                m_NewlyImportedST2USpriteRects = new List<SpriteRect>();
            }

            public void RequiresSuperTiled2UnitySpriteRect(Rect rc)
            {
                // Note: We always use a bottom-left pivot
                // Does the user list already have a sprite rect we can use?
                if (m_PreviouslyImportedSpriteRects.Any(r => r.rect == rc && r.pivot == Vector2.zero))
                {
                    // We are done. The user has provided us with a sprite that will work.
                    return;
                }

                // Does the ST2U list already have a sprite we can use? If so then add that to our required list.
                var spriteName = TilesetLoader.RectToSpriteName(rc);
                var requiredSpriteRect = m_PreviouslyImportedST2USpriteRects.FirstOrDefault(s => s.name == spriteName);
                if (requiredSpriteRect == null)
                {
                    requiredSpriteRect = new SpriteRect
                    {
                        name = spriteName,
                        spriteID = GUID.Generate(),
                        rect = rc,
                        pivot = Vector2.zero,
                        alignment = SpriteAlignment.BottomLeft,
                    };

                    m_NewlyImportedST2USpriteRects.Add(requiredSpriteRect);
                }
                else
                {
                    // We have previously imported this and want to keep it
                    m_PreviouslyImportedSpriteRects.Add(requiredSpriteRect);
                }
            }

            public void Dispose()
            {
                // We can only add so many new sprite rects in an import
                // Otherwise Unity gives us the impression that the importer has locked up or crashed
                if (m_NewlyImportedST2USpriteRects.Count > MaxNumberOfImportingSpriteRects)
                {
                    Debug.LogWarning($"{m_AssetFileName} requires {m_NewlyImportedST2USpriteRects.Count} sprites for Super Tiled2Unity tiles. This is not common. Consider using tilesets with fewer tiles. Only up to {MaxNumberOfImportingSpriteRects} sprites will be added by the default importer.");
                    return;
                }

                var updatedSpriteRects = m_PreviouslyImportedSpriteRects.ToList();
                updatedSpriteRects.AddRange(m_NewlyImportedST2USpriteRects);
                m_DataProvider.SetSpriteRects(updatedSpriteRects.ToArray());

#if UNITY_2021_2_OR_NEWER
                var fileIdDataProvider = m_DataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                var nameFileIdPairs = updatedSpriteRects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID));
                fileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);
#endif
                m_DataProvider.Apply();
                ProfilerMarker_SpriteDataProviderWrapper.End();
            }
        }
    }
}

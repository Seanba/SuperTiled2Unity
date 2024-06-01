using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal static class AddST2USpritesToTexture
    {
        internal static void AddSpritesToTextureAsset(string textureAssetPath, IEnumerable<Rect> rects)
        {
            var texture2d = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            if (texture2d != null)
            {
                var textureAssetName = Path.GetFileName(textureAssetPath);
                EditorUtility.DisplayProgressBar("Adding ST2U Sprites", textureAssetName, 0);

                var factory = new SpriteDataProviderFactories();
                factory.Init();
                var dataProvider = factory.GetSpriteEditorDataProviderFromObject(texture2d);
                dataProvider.InitSpriteEditorDataProvider();

                var spriteRects = dataProvider.GetSpriteRects().ToList();
                spriteRects.Capacity += rects.Count();
                float totalNumberOfSprites = spriteRects.Count + rects.Count();

                if (spriteRects.Count > 0)
                {
                    // Add back all the sprites there were already in the texutre
                    EditorUtility.DisplayProgressBar("Adding ST2U Sprites", $"{textureAssetName} 0 ... {spriteRects.Count}", 0);
                    dataProvider.SetSpriteRects(spriteRects.ToArray());
                    dataProvider.Apply();
                }

                // Add extra sprites piecemeal
                {
                    const int SpritesPerLoop = 128;

                    var additionalSpriteRects = rects.Select(r => new SpriteRect
                    {
                        name = TilesetLoader.RectToSpriteName(r),
                        spriteID = GUID.Generate(),
                        rect = r,
                        pivot = Vector2.zero,
                        alignment = SpriteAlignment.BottomLeft,
                    }).ToList();

                    while (additionalSpriteRects.Count > 0)
                    {
                        // This can take a really long time for ridiculously large tilesets
                        // Give the user a chance to bail
                        bool cancelled = EditorUtility.DisplayCancelableProgressBar("Adding ST2U Sprites", $"{textureAssetName} {spriteRects.Count} ... {spriteRects.Count + SpritesPerLoop} of {totalNumberOfSprites}", spriteRects.Count / totalNumberOfSprites);
                        if (cancelled)
                        {
                            break;
                        }

                        var taken = additionalSpriteRects.Take(SpritesPerLoop);
                        spriteRects.AddRange(taken);
                        additionalSpriteRects.RemoveRange(0, taken.Count());
                        dataProvider.SetSpriteRects(spriteRects.ToArray());
                        dataProvider.Apply();
                    }
                }

#if UNITY_2021_2_OR_NEWER
                {
                    // Add the name file ID pairs
                    EditorUtility.DisplayProgressBar("Adding ST2U Sprites", "Setting file Id data pairs", 1.0f);
                    var fileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                    var nameFileIdPairs = spriteRects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID));
                    fileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);
                }
#endif
                {
                    // Apply all the changes that have been made once last time to catch everything
                    EditorUtility.DisplayProgressBar("Adding ST2U Sprites", "Applying changes", 1.0f);
                    dataProvider.Apply();
                }

                EditorUtility.ClearProgressBar();

                {
                    // Finally reimport the texture asset
                    var assetImporter = dataProvider.targetObject as AssetImporter;
                    assetImporter.SaveAndReimport();
                }
            }
        }
    }
}

using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Xml.Linq;
using System;

namespace SuperTiled2Unity.Editor
{
    // fixit - every image needs to know how it should be split up into tile sprites
    // fixit - how we keep this updated when files are renamed, moved, deleted, etc.?
    internal class SpriteRectsManager
    {
        public static SpriteRectsManager Instance { get; }

        static SpriteRectsManager()
        {
            Instance = CreateInstance();
        }

        internal IEnumerable<SpriteRect> GetSpriteRectsForTexture(string textureAssetPath)
        {
            return Enumerable.Empty<SpriteRect>(); // fixit
        }

        // Seed the collection of sprite rectangles needed to make tiles
        // Further imports will keep this collection updated
        private static SpriteRectsManager CreateInstance()
        {
            // fixit - this should not cause an (re)imports of texture files. A proper (re)import of TMX,TSX files may cause the texture to reimport, however
            var instance = new SpriteRectsManager();

            // Maps (tmx) and tilesets (tsx) may contain tiles and will instruct us on how a texture is to be cut up into sprites
            // We have to look by file extension and not by asset type because these files may not yet be imported
            var tmxFiles = Directory.EnumerateFiles(Application.dataPath, "*.tmx", SearchOption.AllDirectories).ToList();
            foreach (string file in tmxFiles)
            {
                var path = file.SanitizePath();
                instance.ProcessTiledFile(path);
            }

            //var tsxFiles = Directory.EnumerateFiles(Application.dataPath, "*.tsx", SearchOption.AllDirectories).ToList();
            //foreach (string file in tsxFiles)
            //{
            //    var path = file.SanitizePath();
            //    Debug.Log($"fixit - TSX file: {path}");
            //}

            return instance;
        }

        private void ProcessTiledFile(string path)
        {
            path = Path.GetFullPath(path).SanitizePath();

            // Open up the filed file and look for "tileset" elements
            try
            {
                XDocument doc = XDocument.Load(path);
                if (doc == null)
                {
                    Debug.LogError($"'{path}' failed to load.");
                    return;
                }

                foreach (var xTileset in doc.Descendants("tileset"))
                {
                    // External tilesets (which have a source) are ignored
                    var source = xTileset.GetAttributeAs<string>("source");
                    if (string.IsNullOrEmpty(source))
                    {
                        Debug.Log($"fixit internal tileset: {path}, {source}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error processing tiled file '{path}': {e.Message}");
            }

        }

        private class InternalAssetPostprocessor : AssetPostprocessor
        {
            private static readonly string[] TiledExtensions = new string[]
            {
                ".tmx",
                ".tsx",
            };

            private static readonly string[] ImageExtensions = new string[]
            {
                ".bmp",
                ".gif",
                ".jpeg",
                ".jpg",
                ".png",
                ".tga",
                ".tif",
                ".tiff",
            };

            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                // Assets that were imported
                foreach (var imported in importedAssets)
                {
                    if (TiledExtensions.Any(x => imported.EndsWith(x)))
                    {
                        Debug.Log($"fixit Imported tiled file: {imported}");
                    }
                    else if (ImageExtensions.Any(x => imported.EndsWith(x)))
                    {
                        Debug.Log($"fixit Imported image file: {imported}");
                    }
                }

                // Assets that were deleted
                foreach (var deleted in deletedAssets)
                {
                    if (TiledExtensions.Any(x => deleted.EndsWith(x)))
                    {
                        Debug.Log($"fixit Deleted tiled file: {deleted}");
                    }
                    else if (ImageExtensions.Any(x => deleted.EndsWith(x)))
                    {
                        Debug.Log($"fixit Deleted image file: {deleted}");
                    }
                }

                // Assets that were moved or renamed
                for (int i = 0; i < movedAssets.Length; i++)
                {
                    string moved = movedAssets[i];
                    string movedFrom = movedFromAssetPaths[i];

                    if (TiledExtensions.Any(x => moved.EndsWith(x)))
                    {
                        Debug.Log($"fixit Moved tiled file: {movedFrom} -> {moved}");
                    }
                    else if (ImageExtensions.Any(x => moved.EndsWith(x)))
                    {
                        Debug.Log($"fixit Moved image file: {movedFrom} -> {moved}");
                    }
                }
            }
        }
    }
}

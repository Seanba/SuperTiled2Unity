using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Xml.Linq;
using System;

namespace SuperTiled2Unity.Editor
{
    internal class SpriteRectsManager
    {
        private class TsxRectangles
        {
            internal string TsxAbsolutePath { get; }
            internal List<Rect> RectCollection { get; } = new List<Rect>();

            internal TsxRectangles(string tsxAbsolutePath)
            {
                TsxAbsolutePath = tsxAbsolutePath;
            }
        }

        private class TextureData
        {
            internal string TextureAbsolutePath { get; }
            internal List<TsxRectangles> TsxRectanglesCollection { get; } = new List<TsxRectangles>();

            internal TextureData(string textureAbsolutePath)
            {
                TextureAbsolutePath = textureAbsolutePath;
            }

            internal void AddRectangle(string absTsxPath, Rect rectangle)
            {
                absTsxPath = absTsxPath.SanitizePath().ToLower();
                var tsxRectangles = TsxRectanglesCollection.FirstOrDefault(r => r.TsxAbsolutePath == absTsxPath);
                if (tsxRectangles == null)
                {
                    tsxRectangles = new TsxRectangles(absTsxPath);
                    TsxRectanglesCollection.Add(tsxRectangles);
                }

                //tsxRectangles.AddRectangle(rectangle); // fixit:left off here
            }
        }

        private readonly List<TextureData> m_TextureDataCollection = new List<TextureData>();

        internal static SpriteRectsManager Instance { get; }

        static SpriteRectsManager()
        {
            Instance = CreateInstance();
        }

        internal IEnumerable<SpriteRect> GetSpriteRectsForTexture(string textureAssetPath)
        {
            return Enumerable.Empty<SpriteRect>(); // fixit - use a cache when this is figured out
        }

        private void AddRectangle(string absTsxPath, string absTexturePath, Rect rectangle)
        {
            absTexturePath = absTexturePath.SanitizePath().ToLower();
            var textureData = m_TextureDataCollection.FirstOrDefault(t => t.TextureAbsolutePath == absTexturePath);
            if (textureData == null)
            {
                textureData = new TextureData(absTexturePath);
                m_TextureDataCollection.Add(textureData);
            }

            textureData.AddRectangle(absTsxPath, rectangle);
        }

        private void ProcessTiledFile(string path) // fixit - starting point that covers the full contribution/influence of a single tiled resource (so remove that influce here and build it back up)
        {
            var abspath = Path.GetFullPath(path).SanitizePath();

            try
            {
                // Open up the filed file and look for "tileset" elements
                XDocument doc = XDocument.Load(abspath);
                if (doc == null)
                {
                    Debug.LogError($"'Looking for tilesets: {abspath}' failed to load.");
                    return;
                }

                foreach (var xTileset in doc.Descendants("tileset"))
                {
                    // External tilesets (which have a source) are ignored here. They will be processed on their own.
                    var source = xTileset.GetAttributeAs<string>("source");
                    if (string.IsNullOrEmpty(source))
                    {
                        ProcessTileset(abspath, xTileset);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Looking for tilesets. Unknown error processing Tiled file '{abspath}': {e.Message}");
            }
        }

        private void ProcessTileset(string abspath, XElement xTileset)
        {
            // The tileset either has one <image> element or <tile> elements
            var xImage = xTileset.Element("image");
            var xTiles = xTileset.Elements("tile");
            if (xImage != null)
            {
                ProcessTilesetSingle(abspath, xTileset, xImage);
            }
            else
            {
                ProcessTilesetMultiple(abspath, xTileset, xTiles);
            }
        }

        private void ProcessTilesetSingle(string absath, XElement xTileset, XElement xImage)
        {
            // fixit:left
            var relImage = xImage.GetAttributeAs<string>("source");
            var absImage = GetFullPathFromRelative(abspath, relImage);

            var tileCount = xTileset.GetAttributeAs<int>("tilecount");
            var tileWidth = xTileset.GetAttributeAs<int>("tilewidth");
            var tileHeight = xTileset.GetAttributeAs<int>("tileheight");
            var columns = xTileset.GetAttributeAs<int>("columns");
            var spacing = xTileset.GetAttributeAs<int>("spacing", 0);
            var margin = xTileset.GetAttributeAs<int>("margin", 0);

            var imageHeight = xImage.GetAttributeAs<int>("height");

            for (int i = 0; i < tileCount; i++)
            {
                // Get grid x,y coords
                int x = i % columns;
                int y = i / columns;

                // Get x source on texture
                int srcx = x * tileWidth;
                srcx += x * spacing;
                srcx += margin;

                // Get y source on texture
                int srcy = y * tileHeight;
                srcy += y * spacing;
                srcy += margin;

                // In Tiled, texture origin is the top-left. However, in Unity the origin is bottom-left.
                srcy = (imageHeight - srcy) - tileHeight;

                if (srcy < 0)
                {
                    // This is an edge condition in Tiled if a tileset's texture has been resized
                    break;
                }

                // fixit - what to do with this?
                // abspath is adding this rectangle to absimage
                Rect rcSource = new Rect(srcx, srcy, tileWidth, tileHeight);
            }
        }

        private void ProcessTilesetMultiple(string abspath, XElement xTileset, IEnumerable<XElement> xTiles)
        {
            Debug.Log("fixit - figure this out");
        }

        private void ImportTiledFile(string path)
        {
            // How to handle this?
            // We need to (somehow) detect which textures need to be reimported because their sprite rect collection is dirty
            //Debug.Log($"fixit Imported tiled (tsx or tmx) file: {path}");
        }

        private void ImportImageFile(string path)
        {
            // What needs to be updated? Anything?
            //Debug.Log($"fixit Imported image (texture) file: {path}");
        }

        // Seed the collection of sprite rectangles needed to make tiles
        // Further imports will keep this collection updated
        private static SpriteRectsManager CreateInstance()
        {
            var instance = new SpriteRectsManager();

            // Maps (tmx) and tilesets (tsx) may contain tiles and will instruct us on how a texture is to be cut up into sprites
            // We have to look by file extension and not by asset type because these files may not yet be imported
            var tmxFiles = Directory.EnumerateFiles(Application.dataPath, "*.tmx", SearchOption.AllDirectories).ToList();
            foreach (string file in tmxFiles)
            {
                var path = file.SanitizePath();
                instance.ProcessTiledFile(path);
            }

            //var tsxFiles = Directory.EnumerateFiles(Application.dataPath, "*.tsx", SearchOption.AllDirectories).ToList(); // fixit - get working with tmx first
            //foreach (string file in tsxFiles)
            //{
            //    var path = file.SanitizePath();
            //    instance.ProcessTiledFile(path);
            //}

            return instance;
        }


        private static string GetFullPathFromRelative(string absolutionPathParent, string relativeImagePath)
        {
            using (ChDir.FromFilename(absolutionPathParent))
            {
                return Path.GetFullPath(relativeImagePath).SanitizePath();
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
                        SpriteRectsManager.Instance.ImportTiledFile(imported);
                    }
                    else if (ImageExtensions.Any(x => imported.EndsWith(x)))
                    {
                        SpriteRectsManager.Instance.ImportImageFile(imported);
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

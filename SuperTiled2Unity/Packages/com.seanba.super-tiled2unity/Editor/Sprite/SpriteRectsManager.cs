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
        internal static SpriteRectsManager Instance { get; }

        private readonly HashSet<RectangleEntry> m_RectangleEntries = new HashSet<RectangleEntry>();
        private readonly Dictionary<string, List<Rect>> m_RectsCache = new Dictionary<string, List<Rect>>();

        static SpriteRectsManager()
        {
            Instance = CreateInstance();
        }

        internal IEnumerable<Rect> GetSpriteRectsForTexture(string assetPathTexture)
        {
            // fixit - are rects upside down?
            // fixit - use a cache and make the list unique
            var absolutePathTexture = Path.GetFullPath(assetPathTexture).SanitizePath().ToLower();
            return m_RectangleEntries.Where(r => r.AbsolutePathTexture == absolutePathTexture).Select(r => new Rect(r.X, r.Y, r.Width, r.Height));

            /*
            if (m_RectsCache.TryGetValue(absolutePathTexture, out List<Rect> rects))
            {
                return rects;
            }

            // fixit - remove cache entry where necessary
            rects = m_RectangleEntries.Where(r => r.AbsolutePathTexture == absolutePathTexture).Select(r => new Rect(r.X, r.Y, r.Width, r.Height)).ToList();
            m_RectsCache.Add(absolutePathTexture, rects);
            return rects;
            */
        }

        private static string GetSanitizedAbsolutePathFromRelative(string absolutePathParent, string relativeImagePath)
        {
            using (ChDir.FromFilename(absolutePathParent))
            {
                return Path.GetFullPath(relativeImagePath).SanitizePath();
            }
        }

        private void ProcessTiledFile(string path) // fixit - starting point that covers the full contribution/influence of a single tiled resource (so remove that influce here and build it back up)
        {
            var absoluteTsxPath = Path.GetFullPath(path).SanitizePath();

            try
            {
                // Open up the filed file and look for "tileset" elements
                XDocument doc = XDocument.Load(absoluteTsxPath);
                if (doc == null)
                {
                    Debug.LogError($"'Looking for tilesets: {absoluteTsxPath}' failed to load.");
                    return;
                }

                foreach (var xTileset in doc.Descendants("tileset"))
                {
                    // External tilesets (which have a source) are ignored here. They will be processed on their own.
                    var source = xTileset.GetAttributeAs<string>("source");
                    if (string.IsNullOrEmpty(source))
                    {
                        ProcessTileset(absoluteTsxPath, xTileset);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Looking for tilesets. Unknown error processing Tiled file '{absoluteTsxPath}': {e.Message}");
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

        private void ProcessTilesetSingle(string absolutePathTsx, XElement xTileset, XElement xImage)
        {
            var relativePathTexture = xImage.GetAttributeAs<string>("source");
            var absolutePathTexture = GetSanitizedAbsolutePathFromRelative(absolutePathTsx, relativePathTexture);

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

                var entry = new RectangleEntry
                {
                    AbsolutePathTsx = absolutePathTsx.ToLower(),
                    AbsolutePathTexture = absolutePathTexture.ToLower(),
                    X = srcx,
                    Y = srcy,
                    Width = tileWidth,
                    Height = tileHeight,
                };

                m_RectangleEntries.Add(entry);
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
            Debug.Log($"fixit Imported tiled (tsx or tmx) file: {path}");
            ProcessTiledFile(path);
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

            var tsxFiles = Directory.EnumerateFiles(Application.dataPath, "*.tsx", SearchOption.AllDirectories).ToList();
            foreach (string file in tsxFiles)
            {
                var path = file.SanitizePath();
                instance.ProcessTiledFile(path);
            }

            return instance;
        }

        private struct RectangleEntry : IEquatable<RectangleEntry>
        {
            public string AbsolutePathTsx { get; set; }
            public string AbsolutePathTexture { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public static bool operator ==(RectangleEntry lhs, RectangleEntry rhs) => lhs.Equals(rhs);
            public static bool operator !=(RectangleEntry lhs, RectangleEntry rhs) => !(lhs == rhs);

            public override bool Equals(object obj)
            {
                return obj is RectangleEntry other && this.Equals(other);
            }

            public override int GetHashCode()
            {
                return (AbsolutePathTsx, AbsolutePathTexture, X, Y, Width, Height).GetHashCode();
            }

            public bool Equals(RectangleEntry other)
            {
                return AbsolutePathTsx == other.AbsolutePathTsx &&
                    AbsolutePathTexture == other.AbsolutePathTexture &&
                    X == other.X &&
                    Y == other.Y &&
                    Width == other.Width &&
                    Height == other.Height;
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

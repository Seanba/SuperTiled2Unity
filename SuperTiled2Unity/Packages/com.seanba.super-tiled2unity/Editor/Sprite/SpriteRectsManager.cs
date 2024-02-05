using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

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
            var instance = new SpriteRectsManager();

            // Maps (tmx) and tilesets (tsx) may contain tiles and will instruct us on how a texture is to be cut up into sprites
            // We have to look by file extension and not by asset type because these files may not yet be imported
            var tmxFiles = Directory.EnumerateFiles(Application.dataPath, "*.tmx", SearchOption.AllDirectories).ToList();
            foreach (string file in tmxFiles)
            {
                var path = file.SanitizePath();
                Debug.Log($"fixit - TMX file: {path}");
            }

            var tsxFiles = Directory.EnumerateFiles(Application.dataPath, "*.tsx", SearchOption.AllDirectories).ToList();
            foreach (string file in tsxFiles)
            {
                var path = file.SanitizePath();
                Debug.Log($"fixit - TSX file: {path}");
            }

            return instance;
        }
    }
}

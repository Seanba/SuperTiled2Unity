using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity
{
    public class ImportErrors : ScriptableObject
    {
        // Depenency assets that have errors
        public List<string> m_ErrorsInAssetDependencies = new List<string>();

        // Missing sprites in this import
        public List<MissingTileSprites> m_MissingTileSprites = new List<MissingTileSprites>();

        public void ReportErrorsInDependency(string assetPath)
        {
            if (!m_ErrorsInAssetDependencies.Contains(assetPath))
            {
                m_ErrorsInAssetDependencies.Add(assetPath);
            }
        }

        public void ReportMissingSprite(string textureAssetPath, int spriteId, int x, int y, int w, int h)
        {
            var missing = m_MissingTileSprites.FirstOrDefault(m => m.m_TextureAssetPath == textureAssetPath);
            if (missing == null)
            {
                missing = new MissingTileSprites();
                missing.m_TextureAssetPath = textureAssetPath;
                m_MissingTileSprites.Add(missing);
            }

            missing.AddMissingSprite(spriteId, x, y, w, h);
        }

        [Serializable]
        public class MissingTileSprites
        {
            public string m_TextureAssetPath;
            public List<MissingSprite> m_MissingTiles = new List<MissingSprite>();

            public void AddMissingSprite(int spriteId, int x, int y, int w, int h)
            {
                var missing = new MissingSprite
                {
                    m_SpriteId = spriteId,
                    m_Rect = new Rect(x, y, w, h),
                };

                m_MissingTiles.Add(missing);
            }

            [Serializable]
            public class MissingSprite
            {
                public int m_SpriteId;
                public Rect m_Rect;
            }
        }
    }
}

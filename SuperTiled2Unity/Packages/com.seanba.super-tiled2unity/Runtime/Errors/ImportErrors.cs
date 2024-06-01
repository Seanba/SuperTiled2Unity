using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity
{
    public class ImportErrors : ScriptableObject
    {
        // Dependencies that are missing or misplaced
        public List<string> m_MissingDependencies = new List<string>();

        // Depenency assets that have errors
        public List<string> m_ErrorsInAssetDependencies = new List<string>();

        // Missing sprites in this import
        public List<MissingTileSprites> m_MissingTileSprites = new List<MissingTileSprites>();

        // Dependency assets that are using the wrong pixels per unit. Maps, tilesets, and textures must use matching pixels per unit.
        public List<WrongPixelsPerUnit> m_WrongPixelsPerUnits = new List<WrongPixelsPerUnit>();

        public List<string> m_MissingTags = new List<string>();
        public List<string> m_MissingLayers = new List<string>();
        public List<string> m_MissingSortingLayers = new List<string>();

        public List<string> m_GenericErrors = new List<string>();

        public void ReportMissingDependency(string assetPath)
        {
            if (!m_MissingDependencies.Contains(assetPath))
            {
                m_MissingDependencies.Add(assetPath);
            }
        }

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

        public void ReportWrongPixelsPerUnit(string dependencyAssetPath, float dependencyPPU, float ourPPU)
        {
            var wrongPPU = m_WrongPixelsPerUnits.FirstOrDefault(w => w.m_DependencyAssetPath == dependencyAssetPath);
            if (wrongPPU == null)
            {
                wrongPPU = new WrongPixelsPerUnit
                {
                    m_DependencyAssetPath = dependencyAssetPath,
                    m_DependencyPPU = dependencyPPU,
                    m_ExpectingPPU = ourPPU,
                };

                m_WrongPixelsPerUnits.Add(wrongPPU);
            }
        }

        public void ReportMissingTag(string tag)
        {
            if (!m_MissingTags.Contains(tag))
            {
                m_MissingTags.Add(tag);
            }
        }

        public void ReportMissingLayer(string layer)
        {
            if (!m_MissingLayers.Contains(layer))
            {
                m_MissingLayers.Add(layer);
            }
        }

        public void ReportMissingSortingLayer(string sortingLayer)
        {
            if (!m_MissingSortingLayers.Contains(sortingLayer))
            {
                m_MissingSortingLayers.Add(sortingLayer);
            }
        }

        public void ReportGenericError(string error)
        {
            if (!m_GenericErrors.Contains(error))
            {
                m_GenericErrors.Add(error);
            }
        }

        [Serializable]
        public class MissingTileSprites
        {
            public string m_TextureAssetPath;
            public List<MissingSprite> m_MissingSprites = new List<MissingSprite>();

            public void AddMissingSprite(int spriteId, int x, int y, int w, int h)
            {
                var missing = new MissingSprite
                {
                    m_SpriteId = spriteId,
                    m_Rect = new Rect(x, y, w, h),
                };

                m_MissingSprites.Add(missing);
            }

            [Serializable]
            public class MissingSprite
            {
                public int m_SpriteId;
                public Rect m_Rect;
            }
        }

        [Serializable]
        public class WrongPixelsPerUnit
        {
            public string m_DependencyAssetPath;
            public float m_DependencyPPU;
            public float m_ExpectingPPU;
        }

    }
}

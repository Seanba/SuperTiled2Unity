using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // SuperAsset classes give us the ability to search for Tiled asset types
    // They also keep track of references and dependencies
    public class SuperAsset : ScriptableObject
    {
        [SerializeField]
        private List<string> m_AssetDependencies = new List<string>();

        public List<string> AssetDependencies => m_AssetDependencies;

        public void AddDependency(AssetImportContext context, string assetPath)
        {
            if (!m_AssetDependencies.Contains(assetPath))
            {
                context.DependsOnSourceAsset(assetPath);
                context.DependsOnArtifact(assetPath);
                m_AssetDependencies.Add(assetPath);
            }
        }
    }
}

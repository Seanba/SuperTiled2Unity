using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace SuperTiled2Unity.Editor
{
    // SuperAssets give us the ability to search for Tiled asset types
    public class SuperAsset : ScriptableObject
    {
        [SerializeField]
        private List<string> m_AssetDependencies = new List<string>();
        public List<String> AssetDependencies { get { return m_AssetDependencies; } }

        public void AddDependency(AssetImportContext context, string assetPath)
        {
            if (!m_AssetDependencies.Contains(assetPath))
            {
                context.DependsOnSourceAsset(assetPath);
                m_AssetDependencies.Add(assetPath);
            }
        }
    }
}

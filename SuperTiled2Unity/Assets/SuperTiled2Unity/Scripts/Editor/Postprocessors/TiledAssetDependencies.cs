using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    public class TiledAssetDependencies
    {
        private static TiledAssetDependencies m_Instance;

        private Dictionary<string, AssetDependencies> m_AssetDependencies = new Dictionary<string, AssetDependencies>(StringComparer.OrdinalIgnoreCase);

        public static TiledAssetDependencies Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = CreateInstance();
                }

                return m_Instance;
            }
        }

        public void TrackDependencies(string assetPath)
        {
            SuperAsset super = AssetDatabase.LoadAssetAtPath<SuperAsset>(assetPath);
            if (super != null)
            {
                // Keep track of our dependencies
                AssetDependencies depends = AcquireAssetDependencies(assetPath);
                depends.AssignDependencies(super.AssetDependencies);

                // Remove our reference from all other assets
                foreach (var dep in m_AssetDependencies.Values)
                {
                    dep.RemoveReference(assetPath);
                }

                // Add our reference to assets we are now dependent on
                foreach (var path in super.AssetDependencies)
                {
                    AssetDependencies reference = AcquireAssetDependencies(path);
                    reference.AddReference(assetPath);
                }
            }
        }

        public bool GetAssetDependencies(string assetPath, out AssetDependencies depends)
        {
            return m_AssetDependencies.TryGetValue(assetPath, out depends);
        }

        private AssetDependencies AcquireAssetDependencies(string assetPath)
        {
            AssetDependencies depends;
            if (!m_AssetDependencies.TryGetValue(assetPath, out depends))
            {
                depends = new AssetDependencies(assetPath);
                m_AssetDependencies.Add(assetPath, depends);
            }

            return depends;
        }

        // Seed our dependency tracking. Further imports should keep it updated.
        private static TiledAssetDependencies CreateInstance()
        {
            var instance = new TiledAssetDependencies();

            // Load all super assets and build up their dictionaries of dependencies
            foreach (var assetGuid in AssetDatabase.FindAssets("t:SuperAsset"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                instance.TrackDependencies(assetPath);
            }

            return instance;
        }
    }
}

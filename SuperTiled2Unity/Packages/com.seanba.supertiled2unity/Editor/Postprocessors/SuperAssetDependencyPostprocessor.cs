using System;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    public class SuperAssetDependencyPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Refresh dependencies for our imported object
            foreach (var assetPath in importedAssets)
            {
                if (assetPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
                {
                    TiledAssetDependencies.Instance.TrackDependencies(assetPath);
                }
            }
        }
    }
}

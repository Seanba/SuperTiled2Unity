using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                TiledAssetDependencies.Instance.TrackDependencies(assetPath);
            }
        }
    }
}

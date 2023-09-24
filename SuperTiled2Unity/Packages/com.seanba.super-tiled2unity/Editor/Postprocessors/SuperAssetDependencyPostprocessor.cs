using System;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    public class SuperAssetDependencyPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // fixit - implement this for deleted assets too (i.e. what if a TSX file was deleted but the source texture stayed? What happens to its sprites? Maybe things just work out?)

            // Refresh dependencies for our imported object
            foreach (var assetPath in importedAssets)
            {
                if (assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    TiledAssetDependencies.Instance.TrackDependencies(assetPath);
                }
            }
        }
    }
}

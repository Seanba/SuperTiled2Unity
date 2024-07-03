using System;
using System.IO;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class AssetPath
    {
        // Transform an absolute path into Assets/Path/To/Asset.ext
        public static bool TryAbsoluteToAsset(ref string absPath)
        {
            // Application.dataPath is the path to whatever/Project/Assets
            var absPathAssets = Path.GetFullPath(Application.dataPath).Replace('\\', '/');
            var thisPath = Path.GetFullPath(absPath).Replace('\\', '/');

            // Is the absolute path passed in in our Assets folder?
            if (thisPath.StartsWith(absPathAssets, StringComparison.OrdinalIgnoreCase))
            {
                absPath = thisPath.Remove(0, absPathAssets.Length + 1);
                absPath = Path.Combine("Assets/", absPath).Replace('\\', '/');
                return true;
            }

            return false;
        }
    }
}

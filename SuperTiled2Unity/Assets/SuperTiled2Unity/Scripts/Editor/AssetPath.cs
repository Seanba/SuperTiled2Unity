using System;
using System.IO;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class AssetPath
    {
        // Transform an absolute path into Assets/Path/To/Asset.ext
        public static bool TryAbsoluteToAsset(ref string path)
        {
            path = path.SanitizePath();

            if (path.StartsWith(Application.dataPath, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Remove(0, Application.dataPath.Length + 1);
                path = Path.Combine("Assets/", path);
                path = path.SanitizePath();
                return true;
            }

            return false;
        }

        // Transform a relative path into Assets/Path/To/Asset.ext
        public static bool TryRelativeToAsset(ref string path)
        {
            path = Path.GetFullPath(path);
            return TryAbsoluteToAsset(ref path);
        }
    }
}

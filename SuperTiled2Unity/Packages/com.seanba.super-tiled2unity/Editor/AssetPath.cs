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
            absPath = absPath.SanitizePath(); // fixit - watch out for these sanitize path calls. They don't work in Linux/MacOS

            if (absPath.StartsWith(Application.dataPath, StringComparison.OrdinalIgnoreCase))
            {
                absPath = absPath.Remove(0, Application.dataPath.Length + 1);
                absPath = Path.Combine("Assets/", absPath);
                absPath = absPath.SanitizePath();
                return true;
            }

            return false;
        }
    }
}

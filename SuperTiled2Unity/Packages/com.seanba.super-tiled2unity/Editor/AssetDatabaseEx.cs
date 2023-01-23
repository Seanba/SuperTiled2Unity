using System;
using System.Linq;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    public static class AssetDatabaseEx
    {
        public static T LoadFirstAssetByFilter<T>(string filter) where T : UnityEngine.Object // fixit - probably won't need this with fixed icon paths
        {
            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return null;
        }
    }
}

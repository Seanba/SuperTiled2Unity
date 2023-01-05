using System;
using System.Linq;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    public static class AssetDatabaseEx
    {
        public static T LoadFirstAssetByFilter<T>(string filter) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return null;
        }

        public static T LoadFirstAssetByFilterAndExtension<T>(string filter, string extension) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets(filter);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }

            return null;

        }

        // Note this returns the first match so be careful if you have multiple scripts with the same class name
        public static string GetFirstPathOfScriptAsset<T>()
        {
            var name = typeof(T).Name;
            var guids = AssetDatabase.FindAssets("t: script " + name);

            if (guids.Any())
            { 
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            return string.Empty;
        }
    }
}

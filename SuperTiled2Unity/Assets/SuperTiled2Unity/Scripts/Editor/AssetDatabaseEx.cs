using System;
using System.Collections.Generic;
using System.IO;
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

        public static string GetCurrentFolder()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            path.TrimEnd('/', '\\');
            return path;
        }
    }
}

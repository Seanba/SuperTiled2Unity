using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal class SuperTiled2Unity_Config
    {
        public static readonly string Version;

        static SuperTiled2Unity_Config()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            Version = info?.version ?? "unknown";
        }

        public static string GetVersionError()
        {
            return string.Format("SuperTiled2Unity requires Unity 2020.3 or later. You are using {0}", Application.unityVersion);
        }

        [MenuItem("Assets/Super Tiled2Unity/Export ST2U Asset...", true)]
        public static bool ExportSuperAssetValidate()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                return AssetDatabase.LoadAssetAtPath<SuperAsset>(path) != null;
            }

            return false;
        }

        [MenuItem("Assets/Super Tiled2Unity/Export ST2U Asset...")]
        public static void ExportSuperAsset()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var tracker = new RecursiveAssetDependencyTracker(path);
            SuperPackageExport.ShowWindow(Path.GetFileNameWithoutExtension(path), tracker.Dependencies);
        }

        [MenuItem("Assets/Super Tiled2Unity/Apply Default Settings to ST2U Assets")]
        public static void ReimportWithDefaults()
        {
            UnityEngine.Object[] selectedAsset = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            HashSet<TiledAssetImporter> tiledImporters = new HashSet<TiledAssetImporter>();

            foreach (var obj in selectedAsset)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var importer = AssetImporter.GetAtPath(path) as TiledAssetImporter;
                if (importer != null)
                {
                    tiledImporters.Add(importer);
                }
            }

            foreach (var importer in tiledImporters)
            {
                importer.ApplyDefaultSettings();
            }

            foreach (var importer in tiledImporters)
            {
                importer.SaveAndReimport();
            }
        }
    }
}

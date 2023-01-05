using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal class SuperTiled2Unity_Config
    {
        internal const string Version = "1.10.7";
        internal const string DefaultSettingsFileName = "ST2U Settings.asset";

        public static ST2USettings CreateDefaultSettings()
        {
            var scriptPath = AssetDatabaseEx.GetFirstPathOfScriptAsset<SuperTiled2Unity_Config>();
            var settingsPath = Path.GetDirectoryName(scriptPath);
            settingsPath = Path.Combine(settingsPath, DefaultSettingsFileName).SanitizePath();

            var settings = ScriptableObject.CreateInstance<ST2USettings>();
            AssetDatabase.CreateAsset(settings, settingsPath);
            AssetDatabase.SaveAssets();

            return settings;
        }

        public static string GetVersionError()
        {
            return string.Format("SuperTiled2Unity requires Unity 2018.3 or later. You are using {0}", Application.unityVersion);
        }

        [MenuItem("Assets/SuperTiled2Unity/Export ST2U Asset...", true)]
        public static bool ExportSuperAssetValidate()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                return AssetDatabase.LoadAssetAtPath<SuperAsset>(path) != null;
            }

            return false;
        }

        [MenuItem("Assets/SuperTiled2Unity/Export ST2U Asset...")]
        public static void ExportSuperAsset()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var tracker = new RecursiveAssetDependencyTracker(path);
            SuperPackageExport.ShowWindow(Path.GetFileNameWithoutExtension(path), tracker.Dependencies);
        }

        [MenuItem("Assets/SuperTiled2Unity/Apply Default Settings to ST2U Assets")]
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

        // This is only invoked by a deployment batch file
        public static void DeploySuperTiled2Unity()
        {
            var path = string.Format("{0}/../../deploy/SuperTiled2Unity.{1}.unitypackage", Application.dataPath, SuperTiled2Unity_Config.Version);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var files = Directory.GetFiles("Assets/SuperTiled2Unity", "*.*", SearchOption.AllDirectories).ToList();

            // Do not export meta files nor the default settings (which will be created)
            files.RemoveAll(f => f.EndsWith("*.meta", StringComparison.OrdinalIgnoreCase));
            files.RemoveAll(f => f.EndsWith(DefaultSettingsFileName, StringComparison.OrdinalIgnoreCase));

            AssetDatabase.ExportPackage(files.ToArray(), path);
        }
    }
}

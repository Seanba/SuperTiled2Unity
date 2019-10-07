using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal class SuperTiled2Unity_Config
    {
        internal const string Version = "1.7.0";
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

        // This is only invoked by a deployment batch file
        private static void DeploySuperTiled2Unity()
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

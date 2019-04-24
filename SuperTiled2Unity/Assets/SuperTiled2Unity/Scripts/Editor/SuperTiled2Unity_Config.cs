using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal static class SuperTiled2Unity_Config
    {
        internal const string Version = "1.x.x.x";

        // Do not submit this enabled. For testing only.
        [MenuItem("Assets/SuperTiled2Unity/Get Settings")]
        private static void CreateInitialSettings() // fixit - remove this
        {
            ST2USettings.GetOrCreateSettings();
        }

        // This is only invoked by a deployment batch file
        private static void DeploySuperTiled2Unity()
        {
            var settings = ST2USettings.LoadSettings();
            var path = string.Format("{0}/../../deploy/SuperTiled2Unity.{1}.unitypackage", Application.dataPath, settings.Version);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.ExportPackage("Assets/SuperTiled2Unity", path, ExportPackageOptions.Recurse);
        }
    }
}

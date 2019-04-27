using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    [CustomEditor(typeof(SuperSettingsImporter))]
    public class SuperSettingsImporterEditor : ScriptedImporterEditor
    {
        public override bool showImportedObject { get { return false; } }

        public override bool HasPreviewGUI()
        {
            return false;
        }

        public override void OnInspectorGUI()
        {
#if UNITY_2018_3_OR_NEWER
            EditorGUILayout.HelpBox("SuperTiled2Unity Settings have been moved to Project Settings. You can delete this asset.", MessageType.Warning);
            if (GUILayout.Button("Open SuperTiled2Unity Project Settings"))
            {
                SettingsService.OpenProjectSettings(ST2USettings.ProjectSettingsPath);
            }
#else
            string error = SuperTiled2Unity_Config.GetVersionError();
            EditorGUILayout.HelpBox(error, MessageType.Error);
#endif
        }
    }
}

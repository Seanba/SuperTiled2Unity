using UnityEditor;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using ScriptedImporterEditor = UnityEditor.AssetImporters.ScriptedImporterEditor;
#else
using ScriptedImporterEditor = UnityEditor.Experimental.AssetImporters.ScriptedImporterEditor;
#endif

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

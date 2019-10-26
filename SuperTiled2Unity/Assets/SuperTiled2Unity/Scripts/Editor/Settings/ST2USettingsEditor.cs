using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    [CustomEditor(typeof(ST2USettings))]
    public class ST2USettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
#if UNITY_2018_3_OR_NEWER
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

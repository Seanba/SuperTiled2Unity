using System.IO;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    [CustomEditor(typeof(SuperWorld))]
    public class SuperWorldEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (target is SuperWorld world && world.m_ImportErrors != null)
            {
                EditorGUILayout.LabelField("Errors Detected in Tiled World Prefab - Inspect For More Details", EditorStyles.boldLabel);

                using (new GuiScopedBackgroundColor(NamedColors.Red))
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromSource(target);
                    var prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
                    EditorGUILayout.HelpBox($"The following prefab asset has reported import errors.\n{prefabAssetPath}\nInspect the prefab for more details.", MessageType.Error);

                    using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                    {
                        var prefabAssetName = Path.GetFileName(prefabAssetPath);
                        if (GUILayout.Button($"Inspect '{prefabAssetName}'"))
                        {
                            Selection.activeObject = prefab;
                        }
                    }
                }
            }

            base.OnInspectorGUI();
        }
    }
}

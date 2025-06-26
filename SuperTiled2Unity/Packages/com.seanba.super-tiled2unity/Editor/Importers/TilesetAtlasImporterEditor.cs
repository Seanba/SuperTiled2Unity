using UnityEditor;
using UnityEditor.AssetImporters;

namespace SuperTiled2Unity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TilesetAtlasImporter))]
    public class TilesetAtlasImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            if (target is TilesetAtlasImporter importer)
            {
                if (importer.SpriteAtlasWillFail(out string reason))
                {
                    // We need 2021.3 or newer in order to use Sprite Atlas V2
                    EditorGUILayout.LabelField("Import Error:", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox(reason, MessageType.Error);
                    EditorGUILayout.Space();
                }
            }

            base.OnInspectorGUI();
        }
    }
}

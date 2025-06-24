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
            base.OnInspectorGUI();
        }
    }
}

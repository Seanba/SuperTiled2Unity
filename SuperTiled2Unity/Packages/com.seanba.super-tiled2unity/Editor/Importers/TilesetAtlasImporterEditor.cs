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
            // fixit - show an error if our Unity version does not support Sprite Atlas Packing V2 properly (before 2021.3) if we are using a V2 sprite atlas
            base.OnInspectorGUI();
        }
    }
}

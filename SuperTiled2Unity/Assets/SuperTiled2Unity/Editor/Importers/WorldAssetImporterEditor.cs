using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WorldAssetImporter))]
    class WorldAssetImporterEditor : TiledAssetImporterEditor<WorldAssetImporter>
    {
        protected override string EditorLabel
        {
            get { return "Tiled World Importer (.world files)"; }
        }

        protected override string EditorDefinition
        {
            get { return "This imports Tiled world files (*.world) and creates a prefab of your world to be added to your scenes."; }
        }

        protected override void InternalOnInspectorGUI()
        {
            //EditorGUILayout.LabelField("Tiled World Importer Settings", EditorStyles.boldLabel);
            InternalApplyRevertGUI();
        }
    }
}

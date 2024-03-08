using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TsxAssetImporter))]
    public class TsxAssetImporterEditor : TiledAssetImporterEditor<TsxAssetImporter>
    {
        public override bool showImportedObject => false;

        protected override string EditorLabel => "Tileset Importer (.tsx files)";
        protected override string EditorDefinition => "This imports Tiled Map Editor tileset files (.tsx) into Unity projects.\n TSX assets are referenced by Tiled Map (.tmx) assets to build maps.";

        public override bool HasPreviewGUI()
        {
            return false;
        }

        protected override void InternalOnInspectorGUI()
        {
            EditorGUILayout.LabelField("Tileset Importer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // fixit - show errors with tileset (and what if the tileset is internal?)

            ShowTiledAssetGui();
            InternalApplyRevertGUI();
        }
    }
}

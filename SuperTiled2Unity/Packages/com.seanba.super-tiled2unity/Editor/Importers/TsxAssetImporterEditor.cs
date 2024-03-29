using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TsxAssetImporter))]
    public class TsxAssetImporterEditor : TiledAssetImporterEditor<TsxAssetImporter>
    {
        private static readonly GUIContent ColliderTypeContent = new GUIContent("Tile Collider Type", "Tiles created by the importer will use this collider type. This is for developers that swap out ST2U generated colliders with TilemapCollider2D components through a custom importer.");

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
            ShowTiledAssetGui();
            ShowAdvancedSettings();
            InternalApplyRevertGUI();
        }

        private void ShowAdvancedSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);

            var colliderType = serializedObject.FindProperty(TsxAssetImporter.ColliderTypeSerializedName);
            Assert.IsNotNull(colliderType);
            EditorGUILayout.PropertyField(colliderType, ColliderTypeContent);
        }
    }
}

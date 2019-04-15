using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TsxAssetImporter))]
    public class TsxAssetImporterEditor : TiledAssetImporterEditor<TsxAssetImporter>
    {
        public override bool showImportedObject { get { return false; } }

        protected override string EditorLabel
        {
            get { return "Tileset Importer (.tsx files)"; }
        }

        protected override string EditorDefinition
        {
            get
            {
                return "This imports Tiled Map Editor tileset files (.tsx) into Unity projects.\n" +
                    "TSX assets are referenced by Tiled Map (.tmx) assets to build maps.";
            }
        }

        public override bool HasPreviewGUI()
        {
            return false;
        }

        protected override void InternalOnInspectorGUI()
        {
            EditorGUILayout.LabelField("Tileset Importer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            InspectorGUIForAtlasSettings();
            ApplyRevertGUI();
        }

        private void InspectorGUIForAtlasSettings()
        {
            ShowTiledAssetGui();
        }
    }
}

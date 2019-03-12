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
        // Serialized properties
        //private SerializedProperty m_UseSpriteAtlas; // fixit - provided sprite atlas
        //private readonly GUIContent m_UseSpriteAtlasContent = new GUIContent("Use Sprite Atlas for Tiles", "Let SuperTiled2Unity create atlas textures to hold your tiles. This will remove visual seams and bands but at the cost of memory.");

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

        public override void OnEnable()
        {
            base.OnEnable();

            CacheSerializedProperites();
        }

        private void CacheSerializedProperites()
        {
            //m_UseSpriteAtlas = this.serializedObject.FindProperty("m_UseSpriteAtlas"); // fixit
            //Assert.IsNotNull(m_UseSpriteAtlas);
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            CacheSerializedProperites();
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

            //ToggleFromInt(this.m_UseSpriteAtlas, this.m_UseSpriteAtlasContent); // fixit
            //m_ShowAtlasSettings.target = (m_UseSpriteAtlas.boolValue && !m_UseSpriteAtlas.hasMultipleDifferentValues);
            //if (EditorGUILayout.BeginFadeGroup(m_ShowAtlasSettings.faded))
            //{
            //    using (new GuiScopedIndent())
            //    {
            //        // This is ugly but C# does not allow generic constraints on enum types
            //        m_AtlasWidth.intValue = (int)(AtlasSize)EditorGUILayout.EnumPopup(m_AtlasWidthContent, (AtlasSize)m_AtlasWidth.intValue);
            //        m_AtlasHeight.intValue = (int)(AtlasSize)EditorGUILayout.EnumPopup(m_AtlasHeightContent, (AtlasSize)m_AtlasHeight.intValue);

            //        EditorGUILayout.HelpBox("SuperTiled2Unity can automate the creation of sprite atlas used to package tiles.\n" +
            //            "This will eliminate visual artifacts like seams from your maps but some users may wish to handle sprite atlases themselves.\n" +
            //            "It is best practice to reuse tilesets so that multiple atlases containing the same tiles are created.\n" +
            //            "Seams can also be avoided by constraining the game camera. This reduces memory but can be difficult to achieve.",
            //            MessageType.None);
            //        EditorGUILayout.Space();

            //    }
            //}
            //EditorGUILayout.EndFadeGroup();
        }
    }
}

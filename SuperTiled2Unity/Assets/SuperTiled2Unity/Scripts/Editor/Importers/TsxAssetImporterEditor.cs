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
        private readonly AnimBool m_ShowAtlasSettings = new AnimBool();

        // Serialized properties
        private SerializedProperty m_UseSpriteAtlas;
        private readonly GUIContent m_UseSpriteAtlasContext = new GUIContent("Use Sprite Atlas for Tiles", "Let SuperTiled2Unity create atlas textures to hold your tiles. This will remove visual seams and bands but at the cost of memory.");

        private SerializedProperty m_AtlasWidth;
        private readonly GUIContent m_AtlasWidthContext = new GUIContent("Sprite Atlas Width", "Pixel width of sprite atlas used for packing tiles.");

        private SerializedProperty m_AtlasHeight;
        private readonly GUIContent m_AtlasHeightContext = new GUIContent("Sprite Atlas Height", "Pixel height of sprite atlas used for packing tiles.");

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

            m_ShowAtlasSettings.valueChanged.AddListener(Repaint);
            m_ShowAtlasSettings.value = m_UseSpriteAtlas.boolValue;
        }

        private void CacheSerializedProperites()
        {
            m_UseSpriteAtlas = this.serializedObject.FindProperty("m_UseSpriteAtlas");
            Assert.IsNotNull(m_UseSpriteAtlas);

            m_AtlasWidth = this.serializedObject.FindProperty("m_AtlasWidth");
            Assert.IsNotNull(m_AtlasWidth);

            m_AtlasHeight = this.serializedObject.FindProperty("m_AtlasHeight");
            Assert.IsNotNull(m_AtlasHeight);
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
            ToggleFromInt(this.m_UseSpriteAtlas, this.m_UseSpriteAtlasContext);
            m_ShowAtlasSettings.target = (m_UseSpriteAtlas.boolValue && !m_UseSpriteAtlas.hasMultipleDifferentValues);
            if (EditorGUILayout.BeginFadeGroup(m_ShowAtlasSettings.faded))
            {
                using (new GuiScopedIndent())
                {
                    // This is ugly but C# does not allow generic constraints on enum types
                    m_AtlasWidth.intValue = (int)(AtlasSize)EditorGUILayout.EnumPopup(m_AtlasWidthContext, (AtlasSize)m_AtlasWidth.intValue);
                    m_AtlasHeight.intValue = (int)(AtlasSize)EditorGUILayout.EnumPopup(m_AtlasHeightContext, (AtlasSize)m_AtlasHeight.intValue);

                    EditorGUILayout.HelpBox("SuperTiled2Unity can automate the creation of sprite atlas used to package tiles.\n" +
                        "This will eliminate visual artifacts like seams from your maps but some users may wish to handle sprite atlases themselves.\n" +
                        "It is best practice to reuse tilesets so that multiple atlases containing the same tiles are created.\n" +
                        "Seams can also be avoided by constraining the game camera. This reduces memory but can be difficult to achieve.",
                        MessageType.None);
                    EditorGUILayout.Space();

                }
            }
            EditorGUILayout.EndFadeGroup();
        }
    }
}

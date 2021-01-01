using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TmxAssetImporter))]
    class TmxAssetImporterEditor : TiledAssetImporterEditor<TmxAssetImporter>
    {
        private SerializedProperty m_TilesAsObjects;
        private readonly GUIContent m_TilesAsObjectsContent = new GUIContent("Tiles as Objects", "Place each tile as separate game object. Uses more resources but gives you more control. This is ignored for Isometric maps that are forced to use game objects.");

        private SerializedProperty m_SortingMode;
        private readonly GUIContent m_SortingModeContent = new GUIContent("Layer/Object Sorting", "Choose the sorting order scheme applied to imported layers and objects.");

        private SerializedProperty m_CustomImporterClassName;

        private string[] m_CustomImporterNames;
        private string[] m_CustomImporterTypes;
        private int m_SelectedCustomImporter;

        private bool m_ShowAutoImporters;

        protected override string EditorLabel
        {
            get { return "Tiled Map Importer (.tmx files)"; }
        }

        protected override string EditorDefinition
        {
            get { return "This imports Tiled map files (*.tmx) and creates a prefab of your map to be added to your scenes."; }
        }

        public override void OnEnable()
        {
            CacheSerializedProperites();
            EnumerateCustomImporterClasses();
            base.OnEnable();
        }

        protected override void InternalOnInspectorGUI()
        {
            EditorGUILayout.LabelField("Tiled Map Importer Settings", EditorStyles.boldLabel);
            ShowTiledAssetGui();

            EditorGUILayout.PropertyField(m_TilesAsObjects, m_TilesAsObjectsContent);

            m_SortingMode.intValue = (int)(SortingMode)EditorGUILayout.EnumPopup(m_SortingModeContent, (SortingMode)m_SortingMode.intValue);

            if (m_SortingMode.intValue == (int)SortingMode.CustomSortAxis)
            {
                EditorGUILayout.HelpBox("Tip: Custom Sort Axis may require you to set a Transparency Sort Axis for cameras in your project Graphics settings.", MessageType.Info);
            }

            EditorGUILayout.Space();
            ShowCustomImporterGui();

            InternalApplyRevertGUI();
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            CacheSerializedProperites();
        }

        private void CacheSerializedProperites()
        {
            m_TilesAsObjects = serializedObject.FindProperty("m_TilesAsObjects");
            Assert.IsNotNull(m_TilesAsObjects);

            m_SortingMode = serializedObject.FindProperty("m_SortingMode");
            Assert.IsNotNull(m_SortingMode);

            m_CustomImporterClassName = serializedObject.FindProperty("m_CustomImporterClassName");
            Assert.IsNotNull(m_CustomImporterClassName);
        }

        private void EnumerateCustomImporterClasses()
        {
            var importerNames = new List<string>();
            var importerTypes = new List<string>();

            // Enumerate all CustomTmxImporter classes that *do not* have the auto importer attribute on them
            var customTypes = AppDomain.CurrentDomain.GetAllDerivedTypes<CustomTmxImporter>().
                Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomTmxImporter))).
                Where(t => t.GetCustomAttributes(typeof(AutoCustomTmxImporterAttribute), true).Length == 0).
                OrderBy(t => t.GetDisplayName());

            foreach (var t in customTypes)
            {
                importerNames.Add(t.GetDisplayName());
                importerTypes.Add(t.FullName);
            }

            importerNames.Insert(0, "None");
            importerTypes.Insert(0, string.Empty);

            m_CustomImporterNames = importerNames.ToArray();
            m_CustomImporterTypes = importerTypes.ToArray();

            m_SelectedCustomImporter = importerTypes.IndexOf(m_CustomImporterClassName.stringValue);
            if (m_SelectedCustomImporter == -1)
            {
                m_SelectedCustomImporter = 0;
                m_CustomImporterClassName.stringValue = string.Empty;
            }
        }

        private void ShowCustomImporterGui()
        {
            // Show the user-selected custom importer
            EditorGUILayout.LabelField("Custom Importer Settings", EditorStyles.boldLabel);
            var selected = EditorGUILayout.Popup("Custom Importer", m_SelectedCustomImporter, m_CustomImporterNames);

            if (selected != m_SelectedCustomImporter)
            {
                m_SelectedCustomImporter = selected;
                m_CustomImporterClassName.stringValue = m_CustomImporterTypes.ElementAtOrDefault(selected);
            }

            EditorGUILayout.HelpBox("Custom Importers are an advanced feature that require scripting. Create a class inherited from CustomTmxImporter and select it from the list above.", MessageType.None);

            // List all the automatically applied custom importers
            using (new GuiScopedIndent())
            {
                var importers = AutoCustomTmxImporterAttribute.GetOrderedAutoImportersTypes();
                var title = string.Format("Auto Importers ({0})", importers.Count());
                var tip = "This custom importers will be automatically applied to your import process.";
                var content = new GUIContent(title, tip);

                m_ShowAutoImporters = EditorGUILayout.Foldout(m_ShowAutoImporters, content);
                if (m_ShowAutoImporters)
                {
                    foreach (var t in importers)
                    {
                        EditorGUILayout.LabelField(t.GetDisplayName());
                    }

                    EditorGUILayout.HelpBox("Auto Importers are custom importers that run on automatically on every exported Tiled map. Order is controlled by the AutoCustomTmxImporterAttribute.", MessageType.None);
                }
            }
        }
    }
}

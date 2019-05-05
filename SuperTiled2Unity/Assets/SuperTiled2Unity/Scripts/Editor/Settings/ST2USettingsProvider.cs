using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;

#elif UNITY_2018_3_OR_NEWER
using UnityEngine.Experimental.UIElements;

#endif

#if UNITY_2018_3_OR_NEWER
namespace SuperTiled2Unity.Editor
{
    public class ST2USettingsProvider : SettingsProvider
    {
        private ST2USettings m_ST2USettings;
        private SerializedObject m_S2TUSettingsObject;
        private ReorderableList m_PrefabReplacementList;
        private bool m_ShowPrefabReplacements;
        private bool m_ShowLayerColors;

        public class SettingsContent
        {
            public static readonly GUIContent m_PixelsPerUnitContent = new GUIContent("Default Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world. (Default Setting)");
            public static readonly GUIContent m_EdgesPerEllipseContent = new GUIContent("Default Edges Per Ellipse", "How many edges to use when appromixating ellipse/circle colliders. (Default Setting)");
            public static readonly GUIContent m_AnimationFramerateContent = new GUIContent("Animation Framerate", "How many frames per second for tile animations.");
            public static readonly GUIContent m_DefaultMaterialContent = new GUIContent("Default Material", "Set to the material you want to use for sprites and tiles imported by SuperTiled2Unity. Leave empy to use built-in sprite material.");
            public static readonly GUIContent m_ObjectTypesXmlContent = new GUIContent("Object Types Xml", "Set to an Object Types Xml file exported from Tiled Object Type Editor.");
            public static readonly GUIContent m_PrefabReplacmentsContent = new GUIContent("Prefab Replacements", "List of prefabs to replace Tiled Object Types during import.");
            public static readonly GUIContent m_LayerColorsContent = new GUIContent("Layer Colors", "These colors will be used for drawing colliders in your imported Tiled maps.");
        }

        public ST2USettingsProvider() : base(ST2USettings.ProjectSettingsPath, SettingsScope.Project)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            m_ST2USettings = ST2USettings.GetOrCreateST2USettings();
            m_S2TUSettingsObject = new SerializedObject(m_ST2USettings);

            // Prepare our list of prefab replacements
            var replacements = m_S2TUSettingsObject.FindProperty("m_PrefabReplacements");
            m_PrefabReplacementList = new ReorderableList(m_S2TUSettingsObject, replacements, true, false, true, true)
            {
                headerHeight = 0,
                drawElementCallback = OnDrawPrefabReplacementElement,
            };
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUIUtility.labelWidth = 200;

            m_S2TUSettingsObject.Update();

            using (new GuiScopedIndent())
            {
                DoGuiVersion();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                DoGuiSettings();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                DoGuiPrefabReplacements();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                DoGuiColliders();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                DoCustomPropertySettings();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.Space();
                DoGuiReimportAssets();
            }

            if (m_S2TUSettingsObject.ApplyModifiedProperties())
            {
                ST2USettings.GetOrCreateST2USettings().RefreshCustomObjectTypes();
            }
        }

        public override void OnTitleBarGUI()
        {
            var tex = EditorGUIUtility.FindTexture("_Help");
            var content = new GUIContent(tex, "Go to SuperTiled2Unity Documentation");
            if (GUILayout.Button(content, EditorStyles.helpBox))
            {
                Application.OpenURL("https://supertiled2unity.readthedocs.io");
            }
        }

        private void DoGuiVersion()
        {
            EditorGUILayout.LabelField("Version: " + SuperTiled2Unity_Config.Version);
        }

        private void DoGuiSettings()
        {
            var ppuProperty = m_S2TUSettingsObject.FindProperty("m_PixelsPerUnit");
            var edgesProperty = m_S2TUSettingsObject.FindProperty("m_EdgesPerEllipse");
            var materialProperty = m_S2TUSettingsObject.FindProperty("m_DefaultMaterial");
            var animationPrpoerty = m_S2TUSettingsObject.FindProperty("m_AnimationFramerate");

            EditorGUILayout.LabelField("Default Import Settings", EditorStyles.boldLabel);

            // Pixels Per Unit
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ppuProperty, SettingsContent.m_PixelsPerUnitContent);
            if (EditorGUI.EndChangeCheck())
            {
                ppuProperty.floatValue = Mathf.Clamp(ppuProperty.floatValue, 0.01f, 2048);
            }

            // Edges Per Ellipse
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(edgesProperty, SettingsContent.m_EdgesPerEllipseContent);
            if (EditorGUI.EndChangeCheck())
            {
                edgesProperty.intValue = Mathf.Clamp(edgesProperty.intValue, 6, 256);
            }

            // Default Material
            EditorGUILayout.PropertyField(materialProperty, SettingsContent.m_DefaultMaterialContent);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);

            // Animation Framerate
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(animationPrpoerty, SettingsContent.m_AnimationFramerateContent);
            if (EditorGUI.EndChangeCheck())
            {
                animationPrpoerty.intValue = Mathf.Clamp(animationPrpoerty.intValue, 1, 125);
            }

            EditorGUILayout.HelpBox("In frames-per-second. Note: You will need to reimport all your tilesets after making changes to the animation framerate for tiles.", MessageType.None);
        }

        private void DoGuiPrefabReplacements()
        {
            EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);

            m_ShowPrefabReplacements = EditorGUILayout.Foldout(m_ShowPrefabReplacements, SettingsContent.m_PrefabReplacmentsContent);
            if (m_ShowPrefabReplacements)
            {
                m_PrefabReplacementList.DoLayoutList();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add From Object Types Xml"))
                    {
                        m_ST2USettings.AddObjectsToPrefabReplacements();
                    }

                    if (GUILayout.Button("Sort Alphabetically"))
                    {
                        m_ST2USettings.SortPrefabReplacements();
                    }
                }

                EditorGUILayout.HelpBox("When the Tiled import scripts come across a Tiled Object of one of these given types it will be replace, automatically, with the associated prefab.", MessageType.None);
            }
        }

        private void DoGuiColliders()
        {
            EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);

            m_ShowLayerColors = EditorGUILayout.Foldout(m_ShowLayerColors, SettingsContent.m_LayerColorsContent);
            if (m_ShowLayerColors)
            {
                SerializedProperty listProperty = m_S2TUSettingsObject.FindProperty("m_LayerColors");

                using (new GuiScopedIndent())
                {
                    for (int i = 0; i < 32; i++)
                    {
                        string layerName = LayerMask.LayerToName(i);
                        if (string.IsNullOrEmpty(layerName))
                        {
                            continue;
                        }

                        // For each layer that is named give the user a change to modify its color
                        SerializedProperty indexProperty = listProperty.GetArrayElementAtIndex(i);
                        GUIContent indexPropertyContext = new GUIContent(layerName, string.Format("Select color for {0} tile layer colliders", layerName));
                        EditorGUILayout.PropertyField(indexProperty, indexPropertyContext);
                    }
                }
            }
        }

        private void DoCustomPropertySettings()
        {
            var xmlProperty = m_S2TUSettingsObject.FindProperty("m_ObjectTypesXml");

            EditorGUILayout.LabelField("Custom Property Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(xmlProperty, SettingsContent.m_ObjectTypesXmlContent);

            if (!string.IsNullOrEmpty(m_ST2USettings.ParseXmlError))
            {
                EditorGUILayout.HelpBox(m_ST2USettings.ParseXmlError, MessageType.Error);
            }

            EditorGUILayout.Space();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh"))
                {
                    ST2USettings.GetOrCreateST2USettings().RefreshCustomObjectTypes();
                }

                if (GUILayout.Button("View Custom Properties"))
                {
                    CustomPropertiesWindow.ShowCustomPropertiesWindow();
                }
            }
        }

        private void DoGuiReimportAssets()
        {
            EditorGUILayout.LabelField("Reimport Assets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(@"You may want to reimport all Tiled assets after making changes to settings." +
                                @" Be aware this may take a few minutes if you have a lot of Tiled assets." +
                                @" This will force import tilesets, templates, and maps." +
                                @" Note that some assets may be reimported multiple times because of dependencies.",
                                MessageType.Info);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reimport Tiled Assets"))
                {
                    ReimportTiledAssets();
                }
            }
        }

        private void OnDrawPrefabReplacementElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new GuiScopedIndent())
            {
                const int kMargin = 20;
                float fieldWidth = (rect.width - kMargin) / 2;

                var element = m_PrefabReplacementList.serializedProperty.GetArrayElementAtIndex(index);
                var nameProperty = element.FindPropertyRelative("m_TypeName");
                var prefabProperty = element.FindPropertyRelative("m_Prefab");

                rect.y += 2;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + fieldWidth + kMargin, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), prefabProperty, GUIContent.none);
            }
        }

        private static void ReimportTiledAssets()
        {
            // Reimport tilesets first
            foreach (var guid in AssetDatabase.FindAssets("t:SuperAssetTileset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            // Then templates
            foreach (var guid in AssetDatabase.FindAssets("t:SuperAssetTemplate"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            // Then maps
            foreach (var guid in AssetDatabase.FindAssets("t:SuperAssetMap"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider()
        {
            if (ST2USettings.GetOrCreateST2USettings())
            {
                var provider = new ST2USettingsProvider();
                provider.keywords = GetSearchKeywordsFromGUIContentProperties<SettingsContent>();
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}
#endif
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace SuperTiled2Unity.Editor
{
    public class ST2USettingsProvider : SettingsProvider
    {
        private const string k_ProjectSettingsPath = "Project/Super Tiled2Unity";

        private SerializedObject m_SerializedObject;
        private ReorderableList m_MaterialMatchingsList;
        private ReorderableList m_PrefabReplacementList;

        private bool m_ShowMaterialMatchings;
        private bool m_ShowPrefabReplacements;
        private bool m_ShowLayerColors;
        private bool m_ApplyDefaultSettings;

        public ST2USettingsProvider() : base(k_ProjectSettingsPath, SettingsScope.Project)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            ST2USettings.instance.SaveSettings();
            m_SerializedObject = new SerializedObject(ST2USettings.instance);

            // Prepare our list of material matchings
            var matchings = m_SerializedObject.FindProperty(nameof(ST2USettings.m_MaterialMatchings));
            m_MaterialMatchingsList = new ReorderableList(m_SerializedObject, matchings, true, false, true, true)
            {
                headerHeight = 0,
                drawElementCallback = OnDrawMaterialMatchingElement,
            };

            // Prepare our list of prefab replacements
            var replacements = m_SerializedObject.FindProperty(nameof(ST2USettings.m_PrefabReplacements));
            m_PrefabReplacementList = new ReorderableList(m_SerializedObject, replacements, true, false, true, true)
            {
                headerHeight = 0,
                drawElementCallback = OnDrawPrefabReplacementElement,
            };
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUIUtility.labelWidth = 200;

            m_SerializedObject.Update();
            EditorGUI.BeginChangeCheck();

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

            if (EditorGUI.EndChangeCheck())
            {
                if (m_SerializedObject.ApplyModifiedProperties())
                {
                    ST2USettings.instance.RefreshCustomObjectTypes();
                    ST2USettings.instance.SaveSettings();
                }
            }
        }

        public override void OnTitleBarGUI()
        {
            var tex = EditorGUIUtility.FindTexture("_Help");
            var content = new GUIContent(tex, "Go to SuperTiled2Unity Documentation");
            if (GUILayout.Button(content, EditorStyles.helpBox))
            {
                Application.OpenURL("https://github.com/Seanba/SuperTiled2Unity/wiki");
            }
        }

        private void DoGuiVersion()
        {
            EditorGUILayout.LabelField("Version: " + SuperTiled2Unity_Config.Version);
        }

        private void DoGuiSettings()
        {
            EditorGUILayout.LabelField("Default Import Settings", EditorStyles.boldLabel);

            // Pixels Per Unit
            {
                var ppuProperty = m_SerializedObject.FindProperty(nameof(ST2USettings.m_DefaultPixelsPerUnit));
                ppuProperty.floatValue = EditorGUILayout.FloatField(SettingsContent.DefaultPixelsPerUnitContent, ppuProperty.floatValue);
                ppuProperty.floatValue = Mathf.Clamp(ppuProperty.floatValue, 0.001f, 2048);
            }

            // Edges Per Ellipse
            {
                var edgesProperty = m_SerializedObject.FindProperty(nameof(ST2USettings.m_DefaultEdgesPerEllipse));
                edgesProperty.intValue = EditorGUILayout.IntField(SettingsContent.DefaultEdgesPerEllipseContent, edgesProperty.intValue);
                edgesProperty.intValue = Mathf.Clamp(edgesProperty.intValue, 6, 256);
            }

            // Default Material
            {
                var materialProperty = m_SerializedObject.FindProperty(nameof(ST2USettings.m_DefaultMaterial));
                materialProperty.objectReferenceValue = EditorGUILayout.ObjectField(SettingsContent.DefaultMaterialContent, materialProperty.objectReferenceValue, typeof(Material), false);
                EditorGUILayout.Space();

                DoGuiMaterialMatchings();
                EditorGUILayout.Space();
            }

            // Animation settings
            {
                var animationPrpoerty = m_SerializedObject.FindProperty(nameof(ST2USettings.m_AnimationFramerate));

                EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);

                // Animation Framerate
                animationPrpoerty.intValue = EditorGUILayout.IntField(SettingsContent.AnimationFramerateContent, animationPrpoerty.intValue);
                animationPrpoerty.intValue = Mathf.Clamp(animationPrpoerty.intValue, 1, 125);

                EditorGUILayout.HelpBox("In frames-per-second. Note: You will need to reimport all your tilesets after making changes to the animation framerate for tiles.", MessageType.None);
            }
        }

        private void DoGuiMaterialMatchings()
        {
            EditorGUILayout.LabelField("Material Matchings", EditorStyles.boldLabel);

            m_ShowMaterialMatchings = EditorGUILayout.Foldout(m_ShowMaterialMatchings, SettingsContent.MaterialMatchingsContent);
            if (m_ShowMaterialMatchings)
            {
                m_MaterialMatchingsList.DoLayoutList();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Sort Alphabetically"))
                    {
                        ST2USettings.instance.SortMaterialMatchings();
                        EditorUtility.SetDirty(ST2USettings.instance);
                    }
                }

                EditorGUILayout.HelpBox("Fill this out with the names of Tile Layer names and their matching materials. Any imported Tiled Layer with a matching name will use the assigned material.", MessageType.None);
            }
        }

        private void DoGuiPrefabReplacements()
        {
            EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);

            m_ShowPrefabReplacements = EditorGUILayout.Foldout(m_ShowPrefabReplacements, SettingsContent.PrefabReplacmentsContent);
            if (m_ShowPrefabReplacements)
            {
                m_PrefabReplacementList.DoLayoutList();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add From Object Types Xml"))
                    {
                        ST2USettings.instance.AddObjectsToPrefabReplacements();
                        EditorUtility.SetDirty(ST2USettings.instance);
                    }

                    if (GUILayout.Button("Sort Alphabetically"))
                    {
                        ST2USettings.instance.SortPrefabReplacements();
                        EditorUtility.SetDirty(ST2USettings.instance);
                    }
                }

                EditorGUILayout.HelpBox("When the Tiled import scripts come across a Tiled Object of one of these given types it will be replaced, automatically, with the associated prefab.", MessageType.None);
            }
        }

        private void DoGuiColliders()
        {
            EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);

            SerializedProperty geoTypeProperty = m_SerializedObject.FindProperty(nameof(ST2USettings.m_CollisionGeometryType));
            geoTypeProperty.intValue = (int)(CompositeCollider2D.GeometryType)EditorGUILayout.EnumPopup(SettingsContent.CollisionGeometryTypeContent, (CompositeCollider2D.GeometryType)geoTypeProperty.intValue);

            m_ShowLayerColors = EditorGUILayout.Foldout(m_ShowLayerColors, SettingsContent.LayerColorsContent);
            if (m_ShowLayerColors)
            {
                SerializedProperty listProperty = m_SerializedObject.FindProperty(nameof(ST2USettings.m_LayerColors));

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
                        indexProperty.colorValue = EditorGUILayout.ColorField(indexPropertyContext, indexProperty.colorValue);
                    }
                }
            }
        }

        private void DoCustomPropertySettings()
        {
            var xmlProperty = m_SerializedObject.FindProperty(nameof(ST2USettings.m_ObjectTypesXml));

            EditorGUILayout.LabelField("Custom Property Settings", EditorStyles.boldLabel);

            xmlProperty.objectReferenceValue = EditorGUILayout.ObjectField(SettingsContent.ObjectTypesXmlContent, xmlProperty.objectReferenceValue, typeof(TextAsset), false);

            if (!string.IsNullOrEmpty(ST2USettings.instance.m_ParseXmlError))
            {
                EditorGUILayout.HelpBox(ST2USettings.instance.m_ParseXmlError, MessageType.Error);
            }

            EditorGUILayout.Space();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh"))
                {
                    ST2USettings.instance.RefreshCustomObjectTypes();
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
                m_ApplyDefaultSettings = GUILayout.Toggle(m_ApplyDefaultSettings, SettingsContent.ApplyDefaultSettingsContent);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reimport Tiled Assets"))
                {
                    ReimportTiledAssets(m_ApplyDefaultSettings);
                }
            }
        }

        private void OnDrawMaterialMatchingElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (new GuiScopedIndent())
            {
                const int kMargin = 20;
                float fieldWidth = (rect.width - kMargin) / 2;

                var element = m_MaterialMatchingsList.serializedProperty.GetArrayElementAtIndex(index);
                var nameProperty = element.FindPropertyRelative("m_LayerName");
                var materialProperty = element.FindPropertyRelative("m_Material");

                rect.y += 2;

                nameProperty.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), nameProperty.stringValue);
                materialProperty.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x + fieldWidth + kMargin, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, materialProperty.objectReferenceValue, typeof(Material), false);
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

                nameProperty.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), nameProperty.stringValue);
                prefabProperty.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x + fieldWidth + kMargin, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, prefabProperty.objectReferenceValue, typeof(GameObject), false);
            }
        }

        private static void ReimportTiledAssets(bool applyDefaults)
        {
            if (applyDefaults)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:SuperAsset"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetImporter.GetAtPath(path) is TiledAssetImporter importer)
                    {
                        importer.ApplyDefaultSettings();
                    }
                }
            }

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
            var provider = new ST2USettingsProvider()
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<SettingsContent>()
            };

            return provider;
        }

        private class SettingsContent
        {
            public static readonly GUIContent DefaultPixelsPerUnitContent = new GUIContent("Default Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world. (Default Setting)");
            public static readonly GUIContent DefaultEdgesPerEllipseContent = new GUIContent("Default Edges Per Ellipse", "How many edges to use when appromixating ellipse/circle colliders. (Default Setting)");
            public static readonly GUIContent AnimationFramerateContent = new GUIContent("Animation Framerate", "How many frames per second for tile animations.");
            public static readonly GUIContent DefaultMaterialContent = new GUIContent("Default Material", "Set to the material you want to use for sprites and tiles imported by SuperTiled2Unity. Leave empy to use built-in sprite material.");
            public static readonly GUIContent MaterialMatchingsContent = new GUIContent("Material Matchings", "Match these materials by Tiled Layer names.");
            public static readonly GUIContent ObjectTypesXmlContent = new GUIContent("Object Types Xml", "Set to an Object Types Xml file exported from Tiled Object Type Editor.");
            public static readonly GUIContent PrefabReplacmentsContent = new GUIContent("Prefab Replacements", "List of prefabs to replace Tiled Object Types during import.");
            public static readonly GUIContent CollisionGeometryTypeContent = new GUIContent("Collision Geometry Type", "The type of geometry used by CompositeCollider2D components.");
            public static readonly GUIContent LayerColorsContent = new GUIContent("Layer Colors", "These colors will be used for drawing colliders in your imported Tiled maps.");
            public static readonly GUIContent ApplyDefaultSettingsContent = new GUIContent("Apply Default Settings", "Default Import Settings will be applied to every ST2U that is imported. Beware!");
        }
    }
}

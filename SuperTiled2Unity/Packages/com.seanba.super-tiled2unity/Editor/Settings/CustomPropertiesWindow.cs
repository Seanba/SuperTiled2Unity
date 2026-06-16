using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class CustomPropertiesWindow : EditorWindow
    {
        private static readonly GUIContent m_TitleContent = new GUIContent("Loaded Custom Type Properties", "These are the custom type defaults loaded from the configured Tiled type assets.");
        private static readonly GUIContent m_EnumTitleContent = new GUIContent("Loaded Custom Enum Types", "These are the custom enum definitions loaded from the configured Tiled type assets.");

        private Vector2 m_ScrollPosition;

        public static void ShowCustomPropertiesWindow()
        {
            ST2USettings.instance.RefreshCustomObjectTypes();
            GetWindow<CustomPropertiesWindow>("SuperTiled2Unity Custom Properties Viewer");
        }

        private void OnGUI()
        {
            using (var scroll = new GUILayout.ScrollViewScope(m_ScrollPosition))
            {
                m_ScrollPosition = scroll.scrollPosition;
                OnGUICustomProperties();
                OnGUIButtons();
            }
        }

        private void OnGUICustomProperties()
        {
            EditorGUILayout.LabelField(m_TitleContent, EditorStyles.boldLabel);

            foreach (var obj in ST2USettings.instance.m_CustomObjectTypes)
            {
                EditorGUILayout.Space();
                OnGUICustomObjectType(obj);
                EditorGUILayout.Space();
            }

            if (!ST2USettings.instance.m_CustomEnumTypes.IsEmpty())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(m_EnumTitleContent, EditorStyles.boldLabel);

                foreach (var enumType in ST2USettings.instance.m_CustomEnumTypes)
                {
                    EditorGUILayout.Space();
                    OnGUICustomEnumType(enumType);
                }
            }
        }

        private void OnGUICustomObjectType(CustomObjectType obj)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(obj.m_Name);
                GUI.enabled = false;
                EditorGUILayout_ColorFieldNoEdit(new GUIContent("Color"), obj.m_Color);
                GUI.enabled = true;

                EditorGUILayout.Space();

                if (obj.m_CustomProperties.Any())
                {
                    using (new GuiScopedIndent())
                    {
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            foreach (var customProperty in obj.m_CustomProperties)
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField(customProperty.m_Name);
                                    EditorGUILayout.LabelField(customProperty.m_Value, EditorStyles.textField);
                                    EditorGUILayout.LabelField(customProperty.m_Type);
                                    EditorGUILayout.LabelField(customProperty.m_PropertyType);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnGUICustomEnumType(CustomEnumType enumType)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(enumType.m_Name);
                using (new GuiScopedIndent())
                {
                    EditorGUILayout.LabelField("Storage Type", enumType.m_StorageType);
                    EditorGUILayout.Toggle("Values As Flags", enumType.m_ValuesAsFlags);
                    EditorGUILayout.LabelField("Values", string.Join(", ", enumType.m_Values));
                }
            }
        }

        private void OnGUIButtons()
        {
            EditorGUILayout.Space();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh Properties"))
                {
                    ST2USettings.instance.RefreshCustomObjectTypes();
                }

                if (GUILayout.Button("Close Custom Properties Viewer"))
                {
                    Close();
                    return;
                }
            }
        }

        internal static void EditorGUILayout_ColorFieldNoEdit(GUIContent label, Color color)
        {
            EditorGUILayout.ColorField(label, color, false, true, false);
        }

    }
}

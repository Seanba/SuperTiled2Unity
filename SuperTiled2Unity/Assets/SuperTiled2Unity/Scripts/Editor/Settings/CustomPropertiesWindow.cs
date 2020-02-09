using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class CustomPropertiesWindow : EditorWindow
    {
        private static readonly GUIContent m_TitleContent = new GUIContent("Custom Object Types Properties", "These are the objects created in the Object Types Editor in Tiled.");

        private Vector2 m_ScrollPosition;

        public static void ShowCustomPropertiesWindow()
        {
            ST2USettings.GetOrCreateST2USettings().RefreshCustomObjectTypes();
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

            var settings = ST2USettings.GetOrCreateST2USettings();
            foreach (var obj in settings.CustomObjectTypes)
            {
                EditorGUILayout.Space();
                OnGUICustomObjectType(obj);
                EditorGUILayout.Space();
            }
        }

        private void OnGUICustomObjectType(CustomObjectType obj)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(obj.m_Name);
                GUI.enabled = false;
                EditorGUILayout_ColorFieldNoEdit(GUIContent.none, obj.m_Color);
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
                                }
                            }
                        }
                    }
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
                    ST2USettings.GetOrCreateST2USettings().RefreshCustomObjectTypes();
                }

                if (GUILayout.Button("Close Custom Properties Viewer"))
                {
                    Close();
                    return;
                }
            }
        }

#if UNITY_2018_1_OR_NEWER
        internal static void EditorGUILayout_ColorFieldNoEdit(GUIContent label, Color color)
        {
            EditorGUILayout.ColorField(label, color, false, true, false);
        }
#else
        private static ColorPickerHDRConfig m_DummyHDRConfig = new ColorPickerHDRConfig(0, 0, 0, 0);

        internal static void EditorGUILayout_ColorFieldNoEdit(GUIContent label, Color color)
        {
            EditorGUILayout.ColorField(label, color, false, true, false, m_DummyHDRConfig);
        }
#endif
    }
}

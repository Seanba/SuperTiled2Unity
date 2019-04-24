using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace SuperTiled2Unity.Editor
{
    public class ST2USettingsProvider : SettingsProvider
    {
        private SerializedObject m_S2TUSettingsObject;

        public class SettingsContent
        {
            public static readonly GUIContent m_PixelsPerUnitContent = new GUIContent("Default Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world. (Default Setting)");
            public static readonly GUIContent m_EdgesPerEllipseContent = new GUIContent("Default Edges Per Ellipse", "How many edges to use when appromixating ellipse/circle colliders. (Default Setting)");
            public static readonly GUIContent m_AnimationFramerateContent = new GUIContent("Animation Framerate", "How many frames per second for tile animations.");
            public static readonly GUIContent m_DefaultMaterialContent = new GUIContent("Default Material", "Set to the material you want to use for sprites and tiles imported by SuperTiled2Unity. Leave empy to use built-in sprite material.");
            public static readonly GUIContent m_ObjectTypesXmlContent = new GUIContent("Object Types Xml", "Set to an Object Types Xml file exported from Tiled Object Type Editor.");
        }

        public ST2USettingsProvider(string path) : base(path, SettingsScope.Project)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            m_S2TUSettingsObject = ST2USettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            // fixit - consider (small) donation plea (maybe patreon)
            DoGuiVersion();
            EditorGUILayout.Space();

            DoGuiSettings();
            EditorGUILayout.Space();
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

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ppuProperty, SettingsContent.m_PixelsPerUnitContent);
            if (EditorGUI.EndChangeCheck())
            {
                ppuProperty.floatValue = Mathf.Clamp(ppuProperty.floatValue, 0.01f, 2048);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(edgesProperty, SettingsContent.m_EdgesPerEllipseContent);
            if (EditorGUI.EndChangeCheck())
            {
                edgesProperty.intValue = Mathf.Clamp(edgesProperty.intValue, 6, 256);
            }

            EditorGUILayout.PropertyField(materialProperty, SettingsContent.m_DefaultMaterialContent);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(animationPrpoerty, SettingsContent.m_AnimationFramerateContent);
            if (EditorGUI.EndChangeCheck())
            {
                animationPrpoerty.intValue = Mathf.Clamp(animationPrpoerty.intValue, 1, 125);
            }

            EditorGUILayout.HelpBox("In frames-per-second. Note: You will need to reimport all your tilesets after making changes to the animation framerate for tiles.", MessageType.None);
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider()
        {
            if (ST2USettings.GetOrCreateSettings())
            {
                var provider = new ST2USettingsProvider("Project/SuperTiled2Unity");
                provider.keywords = GetSearchKeywordsFromGUIContentProperties<SettingsContent>();
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}

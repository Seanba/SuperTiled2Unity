using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public abstract class TiledAssetImporterEditor<T> : SuperImporterEditor<T> where T : SuperImporter
    {
        private SerializedProperty m_PixelsPerUnit;
        private readonly GUIContent m_PixelsPerUnitContent = new GUIContent("Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world.");

        private SerializedProperty m_EdgesPerEllipse;
        private readonly GUIContent m_EdgesPerEllipseContent = new GUIContent("Edges Per Ellipse", "How many edges to use when appromixating ellipse/circle colliders.");

        public override void OnEnable()
        {
            CacheSerializedProperites();
            base.OnEnable();
        }

        protected void ShowTiledAssetGui()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_PixelsPerUnit, m_PixelsPerUnitContent);
            if (EditorGUI.EndChangeCheck())
            {
                m_PixelsPerUnit.floatValue = Mathf.Clamp(m_PixelsPerUnit.floatValue, 0.01f, 2048);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_EdgesPerEllipse, m_EdgesPerEllipseContent);
            if (EditorGUI.EndChangeCheck())
            {
                m_EdgesPerEllipse.intValue = Mathf.Clamp(m_EdgesPerEllipse.intValue, 6, 256);
            }
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            CacheSerializedProperites();
        }

        private void CacheSerializedProperites()
        {
            m_PixelsPerUnit = serializedObject.FindProperty("m_PixelsPerUnit");
            Assert.IsNotNull(m_PixelsPerUnit);

            m_EdgesPerEllipse = serializedObject.FindProperty("m_EdgesPerEllipse");
            Assert.IsNotNull(m_EdgesPerEllipse);
        }
    }
}

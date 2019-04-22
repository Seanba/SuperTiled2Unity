using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public abstract class TiledAssetImporterEditor<T> : SuperImporterEditor<T> where T : TiledAssetImporter
    {
        private SerializedProperty m_PixelsPerUnit;
        private readonly GUIContent m_PixelsPerUnitContent = new GUIContent("Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world.");

        private SerializedProperty m_EdgesPerEllipse;
        private readonly GUIContent m_EdgesPerEllipseContent = new GUIContent("Edges Per Ellipse", "How many edges to use when appromixating ellipse/circle colliders.");

        private SerializedProperty m_SpriteAtlas;
        private readonly GUIContent m_SpriteAtlasContent = new GUIContent("Sprite Atlas", "The use of a sprite atlas removes visual artifacts like seams and improves runtime performance");

        public override void OnEnable()
        {
            CacheSerializedProperites();
            base.OnEnable();
        }

        protected void ShowTiledAssetGui()
        {
            EditorGUILayout.PropertyField(m_PixelsPerUnit, m_PixelsPerUnitContent);
            EditorGUILayout.PropertyField(m_EdgesPerEllipse, m_EdgesPerEllipseContent);
            EditorGUILayout.PropertyField(m_SpriteAtlas, m_SpriteAtlasContent);
        }

        protected override void Apply()
        {
            m_PixelsPerUnit.floatValue = Clamper.ClampPixelsPerUnit(m_PixelsPerUnit.floatValue);
            m_EdgesPerEllipse.intValue = Clamper.ClampEdgesPerEllipse(m_EdgesPerEllipse.intValue);
            base.Apply();
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

            m_SpriteAtlas = serializedObject.FindProperty("m_SpriteAtlas");
            Assert.IsNotNull(m_SpriteAtlas);
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public abstract class TiledAssetImporterEditor<T> : SuperImporterEditor<T> where T : TiledAssetImporter
    {
        private static readonly GUIContent PixelsPerUnitContent = new GUIContent("Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world.");
        private static readonly GUIContent EdgesPerEllipseContent = new GUIContent("Edges Per Ellipse", "How many edges to use when appromixating ellipse/circle colliders.");

        protected void ShowTiledAssetGui()
        {
            var pixelsPerUnit = serializedObject.FindProperty(TiledAssetImporter.PixelsPerUnitSerializedName);
            var edgesPerEllipse = serializedObject.FindProperty(TiledAssetImporter.EdgesPerEllipseSerializedName);
            Assert.IsNotNull(pixelsPerUnit);
            Assert.IsNotNull(edgesPerEllipse);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pixelsPerUnit, PixelsPerUnitContent);
            if (EditorGUI.EndChangeCheck())
            {
                pixelsPerUnit.floatValue = Mathf.Clamp(pixelsPerUnit.floatValue, 0.01f, 2048);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(edgesPerEllipse, EdgesPerEllipseContent);
            if (EditorGUI.EndChangeCheck())
            {
                edgesPerEllipse.intValue = Mathf.Clamp(edgesPerEllipse.intValue, 6, 256);
            }
        }
    }
}
